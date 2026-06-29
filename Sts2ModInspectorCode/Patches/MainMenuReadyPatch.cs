using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using Sts2ModInspector.Runtime;

namespace Sts2ModInspector.Patches;

/// <summary>Notifies the ScanService each time the main menu is ready, so the diagnostics badge can
/// attach (immediately if the scan already finished, or when it does). NMainMenu is the confirmed
/// main-menu screen type (decompile).</summary>
[HarmonyPatch(typeof(NMainMenu), "_Ready")]
internal static class MainMenuReadyPatch
{
    private static void Postfix(NMainMenu __instance)
    {
        try { ScanService.OnMainMenuReady(__instance); }
        catch (Exception ex) { MainFile.Logger.Warn($"MainMenuReadyPatch error: {ex.Message}"); }
    }
}
