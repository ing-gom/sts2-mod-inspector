using System.Collections.Generic;

namespace Sts2ModInspector.Discovery;

/// <summary>How worried the user should be about a shared-method patch.</summary>
public enum ConflictSeverity
{
    /// <summary>2+ mods touch the same method, but only with prefixes/postfixes that usually compose.</summary>
    Soft,
    /// <summary>Multiple transpilers or multiple skip-capable prefixes on one method — likely to break.</summary>
    High,
}

/// <summary>One game method that more than one mod patches.</summary>
public sealed record MethodConflict(
    string MethodFqn,
    ConflictSeverity Severity,
    string Kind,                       // "transpiler" | "prefix" | "overlap"
    IReadOnlyList<string> ModIds);     // display names of the mods involved, sorted

/// <summary>Aggregate result of one diagnostic pass — patch conflicts only.</summary>
public sealed record DiagnosticResult(
    IReadOnlyList<MethodConflict> Conflicts,
    int LoadedModCount)
{
    public int HighConflictCount
    {
        get
        {
            var n = 0;
            foreach (var c in Conflicts) if (c.Severity == ConflictSeverity.High) n++;
            return n;
        }
    }

    public bool HasFindings => Conflicts.Count > 0;
}
