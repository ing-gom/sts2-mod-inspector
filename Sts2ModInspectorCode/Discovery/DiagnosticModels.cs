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

/// <summary>A loaded mod's boot footprint. <see cref="InitMs"/> is null when the mod loaded
/// before us (its TryLoadMod ran before our timing patch was applied) — see ScanService.</summary>
public sealed record ModWeight(
    string ModId,
    string DisplayName,
    long PckBytes,
    int PckEntries,
    long DllBytes,
    int PatchCount,
    double Score,
    double? InitMs);

/// <summary>Aggregate result of one diagnostic pass.</summary>
public sealed record DiagnosticResult(
    IReadOnlyList<MethodConflict> Conflicts,
    IReadOnlyList<ModWeight> HeavyMods,
    int LoadedModCount,
    EnvironmentInfo Env)
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

    public bool HasFindings => Conflicts.Count > 0 || HeavyMods.Count > 0 || Env.HasNotable;
}
