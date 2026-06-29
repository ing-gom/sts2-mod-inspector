using System;
using Godot;
using Sts2ModInspector.Discovery;

namespace Sts2ModInspector.Runtime;

/// <summary>
/// Orchestrates the one-shot diagnostic pass and coordinates it with the main-menu badge.
///
/// Timing: we schedule the scan 2 seconds after init so every mod has finished applying its Harmony
/// patches (same deferred-pass trick as Sts2SkinManager's DLL-skin detection). The main menu's
/// _Ready usually fires before the scan completes, so we remember the menu node and attach the badge
/// once results are ready; re-entering the menu later (Latest already set) attaches immediately.
/// </summary>
public static class ScanService
{
    private const float ScanDelaySeconds = 2.0f;

    public static DiagnosticResult? Latest { get; private set; }

    private static Node? _mainMenu;
    private static bool _scheduled;

    /// <summary>Called from MainFile. Schedules the deferred scan exactly once.</summary>
    public static void Schedule(SceneTree tree)
    {
        if (_scheduled) return;
        _scheduled = true;
        try
        {
            tree.CreateTimer(ScanDelaySeconds).Timeout += RunSafely;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"ScanService.Schedule failed: {ex.Message}");
        }
    }

    /// <summary>Called from the NMainMenu._Ready postfix.</summary>
    public static void OnMainMenuReady(Node mainMenu)
    {
        _mainMenu = mainMenu;
        if (Latest != null) TryAttachBadge();
    }

    private static void RunSafely()
    {
        try { Run(); }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"ScanService.Run failed: {ex.Message}");
            MainFile.Logger.Warn(ex.ToString());
        }
    }

    private static void Run()
    {
        var loaded = LoadedModScanner.Scan();
        var asmToMod = LoadedModScanner.BuildAssemblyToMod(loaded);

        var conflicts = PatchConflictScanner.Scan(asmToMod, out var patchCounts);
        var heavy = ModWeightScanner.Scan(loaded, patchCounts);
        var env = EnvironmentScanner.Scan(loaded);

        Latest = new DiagnosticResult(conflicts, heavy, loaded.Count, env);

        MainFile.Logger.Info(
            $"[{MainFile.ModId}] scan: {loaded.Count} mods, {conflicts.Count} conflicts " +
            $"(high {Latest.HighConflictCount}), {heavy.Count} heavy, {BootTimings.Count} timed, " +
            $"devconsole={env.DevConsoleEnabled}, debugmods={env.DebugLikeMods.Count}.");

        TryAttachBadge();
    }

    private static void TryAttachBadge()
    {
        if (Latest == null) return;
        if (_mainMenu == null || !GodotObject.IsInstanceValid(_mainMenu)) return;
        if (!Latest.HasFindings) return; // nothing to report → no badge
        DiagnosticsBadge.Attach(_mainMenu, Latest);
    }
}
