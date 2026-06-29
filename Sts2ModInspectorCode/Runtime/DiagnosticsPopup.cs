using System.Collections.Generic;
using System.Linq;
using Godot;
using Sts2ModInspector.Discovery;
using Sts2ModInspector.Localization;

namespace Sts2ModInspector.Runtime;

/// <summary>Self-contained details popup, built from Godot primitives (no game scene dependency).
/// Dimmed backdrop + centered panel; findings split across three tabs: Critical conflicts, Warning
/// conflicts, and Heavy mods. Save-report / Close in the footer.</summary>
public static class DiagnosticsPopup
{
    private const string LayerName = "Sts2ModInspectorPopup";
    private const int MaxRows = 40;

    private static CanvasLayer? _layer;
    private static Label? _statusLabel;

    public static void Show(DiagnosticResult result)
    {
        Callable.From(() => DoShow(result)).CallDeferred();
    }

    private static void DoShow(DiagnosticResult result)
    {
        if (Engine.GetMainLoop() is not SceneTree tree) return;
        Close(); // refresh if already open

        var layer = new CanvasLayer { Name = LayerName, Layer = 128 };

        var dim = new ColorRect { Color = new Color(0, 0, 0, 0.65f), MouseFilter = Control.MouseFilterEnum.Stop };
        dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(dim);

        var center = new CenterContainer();
        center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        dim.AddChild(center);

        var panel = new PanelContainer();
        center.AddChild(panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        panel.AddChild(margin);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        margin.AddChild(vbox);

        var title = new Label { Text = Strings.Get("title") };
        title.AddThemeFontSizeOverride("font_size", 26);
        vbox.AddChild(title);

        var summary = new Label
        {
            Text = Strings.Get("summary", result.LoadedModCount, result.Conflicts.Count,
                result.HighConflictCount, result.HeavyMods.Count),
        };
        summary.AddThemeColorOverride("font_color", new Color(0.75f, 0.80f, 0.88f));
        vbox.AddChild(summary);

        // Environment status line: dev console (debug mode) + any debug/cheat mods.
        var envText = result.Env.DevConsoleEnabled
            ? Strings.Get("env_devconsole_on")
            : Strings.Get("env_devconsole_off");
        if (result.Env.DebugLikeMods.Count > 0)
            envText += "    " + Strings.Get("env_debug_mods", string.Join(", ", result.Env.DebugLikeMods));
        var envLabel = new Label { Text = $"🐞 {envText}", AutowrapMode = TextServer.AutowrapMode.WordSmart };
        envLabel.AddThemeColorOverride("font_color", result.Env.HasNotable
            ? new Color(1.0f, 0.80f, 0.35f)   // amber when something notable is on
            : new Color(0.6f, 0.64f, 0.7f));  // dim when clean
        vbox.AddChild(envLabel);

        var high = result.Conflicts.Where(c => c.Severity == ConflictSeverity.High).ToList();
        var soft = result.Conflicts.Where(c => c.Severity == ConflictSeverity.Soft).ToList();

        var tabs = new TabContainer { CustomMinimumSize = new Vector2(760, 460) };
        vbox.AddChild(tabs);

        var criticalTab = MakeScrollTab(tabs);
        BuildConflictList(criticalTab, high, "conflicts_none_high");
        var warningTab = MakeScrollTab(tabs);
        BuildConflictList(warningTab, soft, "conflicts_none_soft");
        var heavyTab = MakeScrollTab(tabs);
        BuildHeavyList(heavyTab, result);

        tabs.SetTabTitle(0, $"{Strings.Get("tab_critical")} ({high.Count})");
        tabs.SetTabTitle(1, $"{Strings.Get("tab_warning")} ({soft.Count})");
        tabs.SetTabTitle(2, $"{Strings.Get("tab_heavy")} ({result.HeavyMods.Count})");
        // Open on the most important non-empty tab.
        tabs.CurrentTab = high.Count > 0 ? 0 : (soft.Count > 0 ? 1 : 2);

        _statusLabel = new Label { Text = "" };
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.7f));
        vbox.AddChild(_statusLabel);

        var buttons = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.End };
        buttons.AddThemeConstantOverride("separation", 10);
        vbox.AddChild(buttons);

        var saveBtn = new Button { Text = Strings.Get("btn_save") };
        saveBtn.Pressed += () => OnSave(result);
        buttons.AddChild(saveBtn);

        var closeBtn = new Button { Text = Strings.Get("btn_close") };
        closeBtn.Pressed += Close;
        buttons.AddChild(closeBtn);

        dim.GuiInput += @event =>
        {
            if (@event is InputEventMouseButton { Pressed: true }) Close();
        };

        tree.Root.AddChild(layer);
        _layer = layer;
    }

    // A scrollable VBox added as a new tab; returns the inner VBox to populate.
    private static VBoxContainer MakeScrollTab(TabContainer tabs)
    {
        var scroll = new ScrollContainer();
        scroll.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tabs.AddChild(scroll);

        var body = new VBoxContainer();
        body.AddThemeConstantOverride("separation", 6);
        body.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        scroll.AddChild(body);
        return body;
    }

    private static void BuildConflictList(VBoxContainer body, List<MethodConflict> list, string emptyKey)
    {
        if (list.Count == 0)
        {
            body.AddChild(Dim(Strings.Get(emptyKey)));
            return;
        }

        var shown = 0;
        foreach (var c in list)
        {
            if (shown >= MaxRows)
            {
                body.AddChild(Dim(Strings.Get("note_more", list.Count - shown)));
                break;
            }
            var sev = c.Severity == ConflictSeverity.High ? Strings.Get("sev_high") : Strings.Get("sev_soft");
            var kind = c.Kind switch
            {
                "transpiler" => Strings.Get("kind_transpiler"),
                "prefix" => Strings.Get("kind_prefix"),
                _ => Strings.Get("kind_overlap"),
            };
            var row = new Label
            {
                Text = $"[{sev}] {c.MethodFqn}  ({kind})\n        {string.Join(", ", c.ModIds)}",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            row.AddThemeColorOverride("font_color", c.Severity == ConflictSeverity.High
                ? new Color(1.0f, 0.55f, 0.50f)
                : new Color(1.0f, 0.84f, 0.45f));
            body.AddChild(row);
            shown++;
        }
    }

    private static void BuildHeavyList(VBoxContainer body, DiagnosticResult result)
    {
        if (result.HeavyMods.Count == 0)
        {
            body.AddChild(Dim(Strings.Get("heavy_none")));
        }
        else
        {
            foreach (var w in result.HeavyMods)
            {
                var init = w.InitMs.HasValue
                    ? Strings.Get("heavy_init_measured", $"{w.InitMs.Value:F0}")
                    : Strings.Get("heavy_init_unmeasured");
                var detail = $"pck {DiagnosticsReport.Mb(w.PckBytes)} / {w.PckEntries} · " +
                             $"dll {DiagnosticsReport.Mb(w.DllBytes)} · {Strings.Get("heavy_patches", w.PatchCount)}";
                body.AddChild(new Label
                {
                    Text = $"{w.DisplayName}\n        {detail}\n        {init}",
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                });
            }
        }
        body.AddChild(Dim(Strings.Get("heavy_note")));
    }

    private static void OnSave(DiagnosticResult result)
    {
        var path = DiagnosticsReport.Save(result);
        if (_statusLabel == null || !GodotObject.IsInstanceValid(_statusLabel)) return;
        _statusLabel.Text = path != null ? Strings.Get("save_ok", path) : Strings.Get("save_fail");
    }

    private static Label Dim(string text)
    {
        var l = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        l.AddThemeColorOverride("font_color", new Color(0.6f, 0.64f, 0.7f));
        return l;
    }

    private static void Close()
    {
        if (_layer != null && GodotObject.IsInstanceValid(_layer)) _layer.QueueFree();
        _layer = null;
        _statusLabel = null;
    }
}
