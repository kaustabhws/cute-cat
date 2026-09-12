# Code artwork production

The production assets are C# paths and rig equations. No image generation, raster sprite library or downloaded artwork is required. The previously authorized Azure workflow remains documented in [its setup page](art/azure-image-generation.md) but is not used by this revision. Never store or bundle its key.

## Source and provenance

| Source | Purpose |
| --- | --- |
| `src/CuteCat.Core/CatRig.cs` | Continuous poses, gait, face motion, paw endpoint |
| `src/CuteCat.Core/TurnTransition.cs` | Head/body/tail orientation curves for planted turns |
| `src/CuteCat.App/CatPainter.cs` | Original contour, palette and layer order |
| `src/CuteCat.App/CatSurface.cs` | Native premultiplied rendering |
| `src/CuteCat.App/CatPreview.cs` | Same painter in WPF |
| `src/CuteCat.App/MeowSound.cs` | Optional original synthesized chirp |
| `src/CuteCat.App/QualityChecks.cs` | Reproducible stills, action-frame exports and UI exports |

Created 12 September 2026 in this repository from the user's high-level style guidance. No competitor art/source, named-artist imitation, external font or audio pack is included. System Segoe UI and Georgia fonts are used by Windows; font files are not redistributed.

Revision 2 in 0.5.0 follows the user's request for a cuter toy-like cat and proper turns. It retains the same original identity and uses continuous projected geometry rather than horizontal mirroring. `--art-preview PATH` renders source artwork and a turn sequence without launching the desktop companion; `--qa` additionally checks the native integration.

## Reproduce previews

```powershell
./scripts/build.ps1
dotnet src/CuteCat.App/bin/Release/net10.0-windows/CuteCat.dll --qa artifacts/qa-local
python -m venv .venv
.venv/Scripts/python.exe -m pip install -r scripts/requirements-preview.txt
.venv/Scripts/python.exe scripts/render-previews.py artifacts/qa-local
```

The first command builds; the QA command paints the native cat, exercises its production handlers, emits a character sheet, 96/128/160-size sheet, action frames, UI XPS exports and check results. The final script rasterizes XPS using PyMuPDF and makes lossless animated WebP previews with Pillow. These two packages are development tooling, not app dependencies.

WPF `RenderTargetBitmap` produced transparent output in this session, including in software mode. XPS exports from the actual controls were rasterized for layout/color review instead. These are **control renders**, not screenshots of the user's desktop. Neither path captures other apps.

The new runtime contract is [code-cat-rig.json](contracts/code-cat-rig.json). Historical asset manifests and generation prompts are retained for context; do not rebuild the old sprite pipeline unless the user changes direction.

## Checks

Inspect for clipped curves, alpha halos, doubled tails, hidden grooming paws, misplaced contact and inconsistent scale. Compare both themes, minimum UI size, focus-button contrast and popup/combobox contrast. Measure native draw cadence and resources. Keep diagnostic recordings outside the portable app; no renderer output is read back for animation.
