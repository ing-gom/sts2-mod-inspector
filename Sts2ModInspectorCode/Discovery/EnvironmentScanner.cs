using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Saves;

namespace Sts2ModInspector.Discovery;

/// <summary>Modding-environment status worth flagging to the user. Currently: whether the game's
/// developer console (debug mode) is enabled — when on, achievements are typically disabled — plus
/// any loaded mod that looks like a dev/cheat/debug tool.</summary>
public sealed record EnvironmentInfo(bool DevConsoleEnabled, IReadOnlyList<string> DebugLikeMods)
{
    public bool HasNotable => DevConsoleEnabled || DebugLikeMods.Count > 0;
}

public static class EnvironmentScanner
{
    // Substrings (lowercased) that mark a mod as a dev/cheat/debug tool.
    private static readonly string[] Hints = { "devconsole", "dev console", "cheat", "debug", "trainer" };

    public static EnvironmentInfo Scan(IReadOnlyList<LoadedMod> loaded)
    {
        var consoleOn = false;
        try { consoleOn = SaveManager.Instance?.SettingsSave?.FullConsole ?? false; }
        catch (Exception ex) { MainFile.Logger.Warn($"EnvironmentScanner: FullConsole read failed: {ex.Message}"); }

        var debugMods = new List<string>();
        foreach (var lm in loaded)
        {
            // Don't flag ourselves.
            if (string.Equals(lm.ModId, MainFile.ModId, StringComparison.OrdinalIgnoreCase)) continue;
            var hay = ($"{lm.ModId} {lm.DisplayName}").ToLowerInvariant();
            foreach (var h in Hints)
            {
                if (hay.Contains(h)) { debugMods.Add(lm.DisplayName); break; }
            }
        }

        return new EnvironmentInfo(consoleOn, debugMods);
    }
}
