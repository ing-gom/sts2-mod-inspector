using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sts2ModInspector.Discovery;

/// <summary>
/// Estimates each loaded mod's boot cost. The <b>static</b> score (always available, deterministic)
/// combines the things that actually drive STS2 startup time: total .pck bytes + entry count (the
/// game byte-reads pck indices at boot — the root cause of the SkinManager 26–46s stall), the mod's
/// DLL size, and how many Harmony patches it applies. When a real per-mod <see cref="BootTimings"/>
/// sample exists (mods loaded after us), it is attached as ground truth; otherwise the mod is
/// reported on static score alone and flagged as un-timed.
/// </summary>
public static class ModWeightScanner
{
    private const double WPckPerMb = 1.0;
    private const double WPckPerEntry = 0.001;
    private const double WDllPerMb = 0.5;
    private const double WPatch = 0.05;

    // Only surface mods whose static footprint is non-trivial (~2 MB-equivalent), or that recorded a
    // slow real init. Keeps light QoL mods out of the "heavy" list.
    private const double MinReportScore = 2.0;
    private const double SlowInitMsFloor = 150.0;
    private const int TopN = 8;

    public static List<ModWeight> Scan(
        IReadOnlyList<LoadedMod> loaded,
        IReadOnlyDictionary<string, int> patchCountByModId)
    {
        var all = new List<ModWeight>(loaded.Count);

        foreach (var lm in loaded)
        {
            long pckBytes = 0;
            var pckEntries = 0;
            long dllBytes = 0;

            if (!string.IsNullOrEmpty(lm.FolderPath) && Directory.Exists(lm.FolderPath))
            {
                foreach (var pck in SafeEnumerate(lm.FolderPath, "*.pck"))
                {
                    pckBytes += SafeLength(pck);
                    var idx = PckFileExtractor.TryReadIndex(pck);
                    if (idx != null) pckEntries += idx.Count;
                }
                foreach (var dll in SafeEnumerate(lm.FolderPath, "*.dll"))
                {
                    // Don't count the shared ModKit DLL each mod ships — it isn't the mod's own weight.
                    var name = Path.GetFileNameWithoutExtension(dll);
                    if (string.Equals(name, "Sts2.ModKit", StringComparison.OrdinalIgnoreCase)) continue;
                    dllBytes += SafeLength(dll);
                }
            }

            patchCountByModId.TryGetValue(lm.ModId, out var patchCount);
            var initMs = BootTimings.Get(lm.ModId);

            var score =
                (pckBytes / (1024.0 * 1024.0)) * WPckPerMb +
                pckEntries * WPckPerEntry +
                (dllBytes / (1024.0 * 1024.0)) * WDllPerMb +
                patchCount * WPatch;

            all.Add(new ModWeight(
                ModId: lm.ModId,
                DisplayName: lm.DisplayName,
                PckBytes: pckBytes,
                PckEntries: pckEntries,
                DllBytes: dllBytes,
                PatchCount: patchCount,
                Score: score,
                InitMs: initMs));
        }

        // Report a mod if it is heavy by static score OR it recorded a slow real init.
        var reported = all
            .Where(w => w.Score >= MinReportScore || (w.InitMs.HasValue && w.InitMs.Value >= SlowInitMsFloor))
            .ToList();

        // Static footprint is the primary, universal signal (covers every mod regardless of load
        // order). Rank by it; the real init ms, when present, is shown as a bonus annotation.
        reported.Sort((a, b) => b.Score.CompareTo(a.Score));

        return reported.Take(TopN).ToList();
    }

    private static IEnumerable<string> SafeEnumerate(string root, string pattern)
    {
        try { return Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories); }
        catch { return Array.Empty<string>(); }
    }

    private static long SafeLength(string path)
    {
        try { return new FileInfo(path).Length; }
        catch { return 0; }
    }
}
