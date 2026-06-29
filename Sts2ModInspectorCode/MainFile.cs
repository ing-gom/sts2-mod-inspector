using Godot;
using MegaCrit.Sts2.Core.Modding;
using Sts2.ModKit.Bootstrap;
using Sts2ModInspector.Runtime;

namespace Sts2ModInspector;

/// <summary>
/// Mod Inspector — lightweight, read-only patch-conflict diagnostics for installed mods.
/// PatchConflictScanner flags game methods that more than one mod Harmony-patches, by severity
/// (Critical / Warning). Results surface as a bottom-right main-menu badge + a tabbed popup, only
/// when there's at least one conflict. Mod Inspector never changes the load order or settings — it
/// only reads.
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
