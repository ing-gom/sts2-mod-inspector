using Godot;
using MegaCrit.Sts2.Core.Modding;
using Sts2.ModKit.Bootstrap;
using Sts2ModInspector.Runtime;

namespace Sts2ModInspector;

/// <summary>
/// Mod Inspector — read-only, client-side diagnostics for installed mods:
///   (A) PatchConflictScanner — flags game methods that more than one mod Harmony-patches.
///   (B) ModWeightScanner — ranks the heaviest mods at boot by static footprint (pck/dll/patch
///       count), which covers EVERY mod regardless of load order. Real per-mod init ms is attached
///       opportunistically for mods that happen to load after us — a bonus, not the primary signal.
///   (C) EnvironmentScanner — dev console (debug mode) + debug/cheat mods.
/// Results surface as a corner badge + tabbed popup on the main menu, only when there's something to
/// report. Mod Inspector never changes the load order or settings — it only reads.
/// </summary>
[ModInitializer(nameof(Initialize))]
public class MainFile
{
    public const string ModId = "Sts2ModInspector";

    public static readonly MegaCrit.Sts2.Core.Logging.Logger Logger
        = ModBootstrap.CreateLogger(ModId);

    public static void Initialize() =>
        ModBootstrap.Run(ModId, Logger, typeof(MainFile).Assembly, body: () =>
        {
            if (Engine.GetMainLoop() is not SceneTree tree) return;
            ScanService.Schedule(tree);
        });
}
