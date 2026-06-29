using System.Diagnostics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using Sts2ModInspector.Discovery;

namespace Sts2ModInspector.Patches;

/// <summary>Wraps <c>ModManager.TryLoadMod(Mod)</c> — the per-mod load entry point confirmed by
/// decompile — with a stopwatch so we can attribute boot cost to individual mods. Records only
/// mods that actually ended up Loaded (skips disabled/failed/duplicate early-returns).</summary>
[HarmonyPatch(typeof(ModManager), "TryLoadMod")]
internal static class TryLoadModTimingPatch
{
    private static void Prefix(out long __state)
    {
        __state = Stopwatch.GetTimestamp();
    }

    private static void Postfix(Mod mod, long __state)
    {
        try
        {
            if (mod?.state != ModLoadState.Loaded) return;
            var id = mod.manifest?.id;
            if (string.IsNullOrEmpty(id)) return;
            var ms = (Stopwatch.GetTimestamp() - __state) * 1000.0 / Stopwatch.Frequency;
            BootTimings.Record(id!, ms);
        }
        catch
        {
            // Never let timing instrumentation interfere with mod loading.
        }
    }
}
