using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Modding;

namespace Sts2ModInspector.Discovery;

/// <summary>One loaded mod, flattened from <see cref="MegaCrit.Sts2.Core.Modding.Mod"/> so the rest
/// of the diagnostic code never touches the game type directly.</summary>
public sealed record LoadedMod(
    string ModId,
    string DisplayName,
    string? Author,
    string FolderPath,
    string? AssemblyName,
    bool HasPck,
    bool HasDll);

/// <summary>Reads <c>ModManager.Mods</c> once and exposes the loaded set plus an
/// assembly-name → mod-id map. The map is built from <c>Mod.assembly</c> directly — no disk walk
/// needed (the game already resolved each mod's assembly at load time).</summary>
public static class LoadedModScanner
{
    public static List<LoadedMod> Scan()
    {
        var result = new List<LoadedMod>();
        IReadOnlyList<Mod> mods;
        try { mods = ModManager.Mods; }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"LoadedModScanner: ModManager.Mods failed: {ex.Message}");
            return result;
        }

        foreach (var mod in mods)
        {
            if (mod == null) continue;
            if (mod.state != ModLoadState.Loaded) continue;
            var m = mod.manifest;
            if (m?.id == null) continue;

            string? asmName = null;
            try { asmName = mod.assembly?.GetName().Name; } catch { /* ignore */ }

            result.Add(new LoadedMod(
                ModId: m.id,
                DisplayName: string.IsNullOrWhiteSpace(m.name) ? m.id : m.name!,
                Author: m.author,
                FolderPath: mod.path ?? "",
                AssemblyName: asmName,
                HasPck: m.hasPck,
                HasDll: m.hasDll));
        }
        return result;
    }

    /// <summary>assembly simple-name (case-insensitive) → mod display name. Used to attribute a
    /// Harmony patch's owning assembly back to a human-readable mod.</summary>
    public static Dictionary<string, LoadedMod> BuildAssemblyToMod(IReadOnlyList<LoadedMod> loaded)
    {
        var map = new Dictionary<string, LoadedMod>(StringComparer.OrdinalIgnoreCase);
        foreach (var lm in loaded)
        {
            if (string.IsNullOrEmpty(lm.AssemblyName)) continue;
            map[lm.AssemblyName!] = lm;
        }
        return map;
    }
}
