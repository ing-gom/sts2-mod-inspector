# Mod Inspector (Sts2ModInspector)

A **read-only** diagnostics mod for **Slay the Spire 2**. A couple of seconds after boot it runs a
single pass and, if it finds anything worth knowing, shows a small badge in the main-menu
**bottom-right** corner (kept clear of the game's mod-loading notice). Click it for a tabbed report.

It surfaces three things mod users hit but have no in-game tool for:

1. **Patch conflicts** — game methods that *more than one mod* Harmony-patches. Today the only way to
   find these is the "disable mods one by one" bisection. Mod Inspector lists them by severity:
   - **Critical** — multiple transpilers (IL rewrites stack and usually break) or multiple prefixes
     (a prefix can skip the original / the other mods).
   - **Warning** — a prefix + postfix from different mods, which normally compose fine.
   - Known frameworks (BaseLib / RitsuLib / KitLib) that deliberately co-patch the same init methods
     are capped at *warning* so a normal setup doesn't read as a scary red conflict.

2. **Heaviest mods at boot** — ranks mods by **static footprint** (covers *every* mod regardless of
   load order): total `.pck` bytes + entry count (the game byte-reads pck indices at startup), DLL
   size, and number of applied patches. A real measured init time (ms) is attached as a bonus for any
   mod that happens to load after Mod Inspector — earlier mods are labeled un-timed (no silent gaps).

3. **Environment status** — whether the **developer console / debug mode** is on
   (`SettingsSave.full_console`; achievements are usually disabled while it is), and any loaded mod
   that looks like a dev / cheat / debug tool.

The report popup splits findings into **Critical / Warning / Heavy mods** tabs, in **14 languages**.

Everything is client-side and read-only — no gameplay change, no load-order/settings writes, no
multiplayer-sync impact (`affects_gameplay: false`).

## How it works

- `Discovery/PatchConflictScanner.cs` — walks `Harmony.GetAllPatchedMethods()`, attributes each patch
  to its owning mod via the mod's loaded assembly, and groups methods with 2+ distinct owners.
- `Discovery/ModWeightScanner.cs` + `Discovery/PckFileExtractor.cs` — measures pck/dll footprint.
- `Discovery/EnvironmentScanner.cs` — reads `SaveManager.Instance.SettingsSave.FullConsole` + flags
  debug-like mods.
- `Patches/TryLoadModTimingPatch.cs` — wraps `ModManager.TryLoadMod(Mod)` with a stopwatch for the
  opportunistic per-mod init time.
- `Runtime/ScanService.cs` — runs the pass 2s after init (so every mod has finished patching) and
  attaches the badge to `NMainMenu`.

## Building

Requires the shared `Sts2.ModKit` (sibling folder) and a local STS2 install.

```
dotnet build Sts2ModInspector.csproj -c Release
```

The build auto-deploys `Sts2ModInspector.dll` + `Sts2ModInspector.json` + `Sts2.ModKit.dll` to
`<STS2>/mods/Sts2ModInspector/`. Or grab the release zip and drop the `Sts2ModInspector` folder into
your game's `mods/` directory. Launch with **Load with Mods**.

## Limitations

- Init-time measurement only covers mods that load **after** Mod Inspector (load-order dependent); all
  other metrics cover every loaded mod via static footprint.
- A reported conflict is a *possibility*, not a proven break — two mods sharing a method often work.
  The severity is a triage hint, not a verdict.

## License

MIT © 2026 inggom

Author: inggom · `v1.0.0`
