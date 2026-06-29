# Mod Inspector (Sts2ModInspector)

A small, **read-only** mod for **Slay the Spire 2** that does one thing well: it tells you when two
mods **patch the same game method** (a Harmony conflict — the usual cause of "it worked alone but
breaks together").

A couple of seconds after boot it scans, and if it finds any conflict it shows a small badge in the
main-menu **bottom-right** corner (kept clear of the game's mod-loading notice). Click it for a tabbed
list.

- **Critical** — multiple transpilers (IL rewrites stack and usually break) or multiple prefixes
  (a prefix can skip the original / the other mods).
- **Warning** — a prefix + postfix from different mods, which normally compose fine.
- Known frameworks (BaseLib / RitsuLib / KitLib) that deliberately co-patch the same init methods are
  capped at *Warning* so a normal setup doesn't read as a scary red conflict.

Instead of the usual "disable your mods one by one" bisection, you get the conflicting method and the
mods involved at a glance. A conflict is a *possibility*, not a proven break — the severity is a
triage hint, not a verdict.

Lightweight, client-side, **read-only** — no gameplay change, no load-order or settings writes, no
multiplayer-sync impact (`affects_gameplay: false`). 14 languages.

## How it works

- `Discovery/PatchConflictScanner.cs` — walks `Harmony.GetAllPatchedMethods()`, attributes each patch
  to its owning mod via the mod's loaded assembly, and groups methods with 2+ distinct owners.
- `Runtime/ScanService.cs` — runs the scan 2s after init (so every mod has finished patching) and
  attaches the badge to `NMainMenu`.

## Building

Requires the shared `Sts2.ModKit` (sibling folder) and a local STS2 install.

```
dotnet build Sts2ModInspector.csproj -c Release
```

The build auto-deploys `Sts2ModInspector.dll` + `Sts2ModInspector.json` + `Sts2.ModKit.dll` to
`<STS2>/mods/Sts2ModInspector/`. Or grab the release zip and drop the `Sts2ModInspector` folder into
your game's `mods/` directory. Launch with **Load with Mods**.

## License

MIT © 2026 inggom

Author: inggom · `v1.0.1`
