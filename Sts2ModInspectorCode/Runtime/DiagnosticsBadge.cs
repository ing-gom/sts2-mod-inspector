using System;
using Godot;
using Sts2ModInspector.Discovery;
using Sts2ModInspector.Localization;

namespace Sts2ModInspector.Runtime;

/// <summary>The small clickable badge pinned to the main-menu's bottom-right corner (lifted above the
/// game's mod-loading / version notice). Only attached when there's something to report. Click →
/// details popup.</summary>
public static class DiagnosticsBadge
{
    private const string NodeName = "Sts2ModInspectorBadge";

    public static void Attach(Node mainMenu, DiagnosticResult result)
    {
        Callable.From(() => DoAttach(mainMenu, result)).CallDeferred();
    }

    private static void DoAttach(Node mainMenu, DiagnosticResult result)
    {
        try
        {
            if (mainMenu == null || !GodotObject.IsInstanceValid(mainMenu)) return;
            var existing = mainMenu.GetNodeOrNull<Button>(NodeName);
            if (existing != null && GodotObject.IsInstanceValid(existing)) return; // already shown

            var badge = new Button
            {
                Name = NodeName,
                Text = BadgeText(result),
                TooltipText = Strings.Get("badge_tooltip"),
                ZIndex = 1000,
                FocusMode = Control.FocusModeEnum.None,
            };

            // Pin to the bottom-right corner, lifted up so it clears the game's "loading mods"
            // / version notice text along the bottom edge. Grows up-and-left to fit the text.
            badge.AnchorLeft = 1.0f;
            badge.AnchorRight = 1.0f;
            badge.AnchorTop = 1.0f;
            badge.AnchorBottom = 1.0f;
            badge.GrowHorizontal = Control.GrowDirection.Begin;
            badge.GrowVertical = Control.GrowDirection.Begin;
            badge.OffsetRight = -16;
            badge.OffsetLeft = -16;
            badge.OffsetBottom = -72;
            badge.OffsetTop = -72;

            // Tint by worst severity (badge only shows when there's at least one conflict).
            var accent = result.HighConflictCount > 0
                ? new Color(1.0f, 0.45f, 0.40f)   // red-ish (critical present)
                : new Color(1.0f, 0.80f, 0.35f);  // amber (warnings only)
            badge.AddThemeColorOverride("font_color", accent);
            badge.AddThemeColorOverride("font_hover_color", accent);

            badge.Pressed += () =>
            {
                try { DiagnosticsPopup.Show(result); }
                catch (Exception ex) { MainFile.Logger.Warn($"popup open failed: {ex.Message}"); }
            };

            mainMenu.AddChild(badge);
            MainFile.Logger.Info($"[{MainFile.ModId}] badge attached to {mainMenu.Name}.");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"badge attach failed: {ex.Message}");
        }
    }

    private static string BadgeText(DiagnosticResult r)
    {
        var icon = r.HighConflictCount > 0 ? "⚠" : "🔎";
        return $"{icon} {Strings.Get("badge_conflicts", r.Conflicts.Count)}";
    }
}
