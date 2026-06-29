using System;
using System.Collections.Generic;

namespace Sts2ModInspector.Discovery;

/// <summary>Collects per-mod wall-clock load time recorded by <c>TryLoadModTimingPatch</c>.
///
/// Coverage caveat (surfaced in the UI, never hidden): our timing patch is only applied once our
/// own [ModInitializer] runs, which happens partway through ModManager's load loop. Mods sorted
/// before us in the load order already finished <c>TryLoadMod</c> and are therefore NOT timed —
/// they fall back to the static weight score. Only mods loaded after us carry an InitMs.</summary>
public static class BootTimings
{
    private static readonly object Lock = new();
    private static readonly Dictionary<string, double> Ms = new(StringComparer.OrdinalIgnoreCase);

    public static void Record(string modId, double ms)
    {
        if (string.IsNullOrEmpty(modId)) return;
        lock (Lock)
        {
            // Keep the largest sample if TryLoadMod somehow runs twice for an id.
            if (!Ms.TryGetValue(modId, out var prev) || ms > prev) Ms[modId] = ms;
        }
    }

    public static double? Get(string modId)
    {
        lock (Lock)
        {
            return Ms.TryGetValue(modId, out var v) ? v : null;
        }
    }

    public static int Count
    {
        get { lock (Lock) { return Ms.Count; } }
    }
}
