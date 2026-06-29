using System;
using System.IO;
using System.Text;
using Godot;
using Sts2ModInspector.Discovery;

namespace Sts2ModInspector.Runtime;

/// <summary>Renders a <see cref="DiagnosticResult"/> as a plain-text report and writes it to the
/// game user-data dir so users can attach it to a bug report.</summary>
public static class DiagnosticsReport
{
    public static string Build(DiagnosticResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Mod Inspector — diagnostic report ===");
        sb.AppendLine($"Loaded mods: {r.LoadedModCount}");
        sb.AppendLine($"Conflicts: {r.Conflicts.Count} (high: {r.HighConflictCount})");
        sb.AppendLine($"Heavy mods: {r.HeavyMods.Count}");
        sb.AppendLine($"Dev console (debug mode): {(r.Env.DevConsoleEnabled ? "ON — achievements may be disabled" : "off")}");
        if (r.Env.DebugLikeMods.Count > 0)
            sb.AppendLine($"Debug/cheat mods: {string.Join(", ", r.Env.DebugLikeMods)}");
        sb.AppendLine();

        sb.AppendLine("-- Patch conflicts --");
        if (r.Conflicts.Count == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (var c in r.Conflicts)
            {
                var sev = c.Severity == ConflictSeverity.High ? "HIGH" : "soft";
                sb.AppendLine($"[{sev}] {c.MethodFqn}  ({c.Kind})");
                sb.AppendLine($"        mods: {string.Join(", ", c.ModIds)}");
            }
        }
        sb.AppendLine();

        sb.AppendLine("-- Heaviest mods (boot footprint) --");
        if (r.HeavyMods.Count == 0)
        {
            sb.AppendLine("(none above threshold)");
        }
        else
        {
            foreach (var w in r.HeavyMods)
            {
                var init = w.InitMs.HasValue ? $"{w.InitMs.Value:F0} ms (measured)" : "not measured (loaded before Mod Inspector)";
                sb.AppendLine($"{w.DisplayName}");
                sb.AppendLine($"        pck: {Mb(w.PckBytes)} / {w.PckEntries} entries, dll: {Mb(w.DllBytes)}, patches: {w.PatchCount}, score: {w.Score:F1}");
                sb.AppendLine($"        init: {init}");
            }
        }
        sb.AppendLine();
        sb.AppendLine("Note: init time is only measurable for mods that load AFTER Mod Inspector.");
        sb.AppendLine("Mods loaded earlier are ranked by static footprint (pck/dll/patch count).");
        return sb.ToString();
    }

    /// <summary>Writes the report and returns the on-disk path, or null on failure.</summary>
    public static string? Save(DiagnosticResult r)
    {
        try
        {
            var dir = OS.GetUserDataDir();
            var path = Path.Combine(dir, "ModInspector_report.txt");
            File.WriteAllText(path, Build(r), Encoding.UTF8);
            return path;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"DiagnosticsReport.Save failed: {ex.Message}");
            return null;
        }
    }

    public static string Mb(long bytes)
    {
        if (bytes <= 0) return "0 MB";
        var mb = bytes / (1024.0 * 1024.0);
        return mb >= 0.1 ? $"{mb:F1} MB" : $"{bytes / 1024.0:F0} KB";
    }
}
