using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Sts2ModInspector.Discovery;

/// <summary>
/// Walks every currently-applied Harmony patch (across ALL mods, via the global
/// <see cref="Harmony.GetAllPatchedMethods"/>) and reports game methods that more than one mod
/// patches. Generalises Sts2SkinManager's HarmonyPatchInspector, which only looked at a
/// character-spine whitelist, to the whole patched surface.
///
/// Severity heuristic (decompile-grounded — STS2 mods use HarmonyX):
///   - 2+ <b>transpilers</b> on one method  → High (transpilers rewrite IL; stacking almost always breaks).
///   - 2+ <b>prefixes</b> on one method      → High (a prefix may return false and skip the original/others).
///   - otherwise 2+ owners of any kind       → Soft (prefix+postfix from different mods usually compose).
/// Frameworks that deliberately co-patch the same init/registration methods are capped at Soft so a
/// normal RitsuLib+KitLib setup doesn't read as a scary red conflict.
/// </summary>
public static class PatchConflictScanner
{
    // Shared framework libraries that legitimately patch the same engine init/registration methods.
    private static readonly HashSet<string> FrameworkModIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "BaseLib", "RitsuLib", "KitLib",
    };

    /// <summary>Runs the scan. <paramref name="patchCountByModId"/> is filled with the number of
    /// applied patches each mod owns (a footprint signal reused by the weight scanner).</summary>
    public static List<MethodConflict> Scan(
        IReadOnlyDictionary<string, LoadedMod> assemblyToMod,
        out Dictionary<string, int> patchCountByModId)
    {
        patchCountByModId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var conflicts = new List<MethodConflict>();

        IEnumerable<MethodBase> patched;
        try { patched = Harmony.GetAllPatchedMethods(); }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"PatchConflictScanner: GetAllPatchedMethods failed: {ex.Message}");
            return conflicts;
        }

        foreach (var method in patched)
        {
            HarmonyLib.Patches? info;
            try { info = Harmony.GetPatchInfo(method); }
            catch { continue; }
            if (info == null) continue;

            var prefixOwners = OwnersOf(info.Prefixes, assemblyToMod, patchCountByModId);
            var postfixOwners = OwnersOf(info.Postfixes, assemblyToMod, patchCountByModId);
            var transpilerOwners = OwnersOf(info.Transpilers, assemblyToMod, patchCountByModId);
            var finalizerOwners = OwnersOf(info.Finalizers, assemblyToMod, patchCountByModId);

            var allOwners = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            allOwners.UnionWith(prefixOwners.Keys);
            allOwners.UnionWith(postfixOwners.Keys);
            allOwners.UnionWith(transpilerOwners.Keys);
            allOwners.UnionWith(finalizerOwners.Keys);

            // A method only one mod patches is not a conflict.
            if (allOwners.Count < 2) continue;

            ConflictSeverity severity;
            string kind;
            if (transpilerOwners.Count >= 2) { severity = ConflictSeverity.High; kind = "transpiler"; }
            else if (prefixOwners.Count >= 2) { severity = ConflictSeverity.High; kind = "prefix"; }
            else { severity = ConflictSeverity.Soft; kind = "overlap"; }

            // Cap framework-only co-patches at Soft (RitsuLib/KitLib/BaseLib touching the same init).
            if (severity == ConflictSeverity.High && allOwners.All(id => FrameworkModIds.Contains(id)))
                severity = ConflictSeverity.Soft;

            // Resolve mod ids to display names for the UI.
            var names = allOwners
                .Select(id => DisplayNameFor(id, assemblyToMod))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            conflicts.Add(new MethodConflict(
                MethodFqn: DescribeMethod(method),
                Severity: severity,
                Kind: kind,
                ModIds: names));
        }

        // High severity first, then by number of mods involved.
        conflicts.Sort((a, b) =>
        {
            var bySev = b.Severity.CompareTo(a.Severity);
            if (bySev != 0) return bySev;
            return b.ModIds.Count.CompareTo(a.ModIds.Count);
        });
        return conflicts;
    }

    // Maps each patch in a kind-list to its owning mod id, tallying the global patch count.
    // Skips our own patches, HarmonyLib internals, and patches whose assembly we couldn't map to a
    // loaded mod (base-game or transitively-loaded helpers).
    private static Dictionary<string, byte> OwnersOf(
        IEnumerable<Patch> patches,
        IReadOnlyDictionary<string, LoadedMod> assemblyToMod,
        Dictionary<string, int> patchCountByModId)
    {
        var owners = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        foreach (var patch in patches)
        {
            var asmName = patch.PatchMethod?.DeclaringType?.Assembly.GetName().Name;
            if (string.IsNullOrEmpty(asmName)) continue;
            if (string.Equals(asmName, MainFile.ModId, StringComparison.OrdinalIgnoreCase)) continue;
            if (asmName!.StartsWith("0Harmony", StringComparison.Ordinal)) continue;
            if (!assemblyToMod.TryGetValue(asmName, out var lm)) continue;

            owners[lm.ModId] = 1;
            patchCountByModId.TryGetValue(lm.ModId, out var c);
            patchCountByModId[lm.ModId] = c + 1;
        }
        return owners;
    }

    private static string DisplayNameFor(string modId, IReadOnlyDictionary<string, LoadedMod> assemblyToMod)
    {
        foreach (var kv in assemblyToMod)
            if (string.Equals(kv.Value.ModId, modId, StringComparison.OrdinalIgnoreCase))
                return kv.Value.DisplayName;
        return modId;
    }

    private static string DescribeMethod(MethodBase method)
    {
        var type = method.DeclaringType?.Name ?? "?";
        return $"{type}.{method.Name}";
    }
}
