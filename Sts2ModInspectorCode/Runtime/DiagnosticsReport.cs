using System;
using System.IO;
using System.Text;
using Godot;
using Sts2ModInspector.Discovery;

namespace Sts2ModInspector.Runtime;

/// <summary>Renders a <see cref="DiagnosticResult"/> as a plain-text patch-conflict report and writes
/// it to the game user-data dir so users can attach it to a bug report.</summary>
public static class DiagnosticsReport
{
    public static string Build(DiagnosticResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Mod Inspector — patch-conflict report ===");
        sb.AppendLine($"Loaded mods: {r.LoadedModCount}");
        sb.AppendLine($"Conflicts: {r.Conflicts.Count} (critical: {r.HighConflictCount})");
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
                var sev = c.Severity == ConflictSeverity.High ? "CRITICAL" : "warning";
                sb.AppendLine($"[{sev}] {c.MethodFqn}  ({c.Kind})");
                sb.AppendLine($"        mods: {string.Join(", ", c.ModIds)}");
            }
        }
        sb.AppendLine();
        sb.AppendLine("A conflict is a possibility, not a proven break — two mods sharing a method");
        sb.AppendLine("often work. Severity is a triage hint, not a verdict.");
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
}
