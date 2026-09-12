# Handoff — softer character and continuous turns, 0.5.0

## Request / requirements

The user rejected instant left/right rotation and asked for a cuter, less realistic version of the existing original art. CAT-10 adds proper turns; CAT-01/05/07/09 retain a coherent code-only character, gait and responsive interaction. No permissions or monitoring changed.

## Final behavior

The cat reaches either work-area boundary, plants its feet, looks toward the new direction, shuffles its paws, turns its torso through a rounded front view, and lets its tail follow. The transition takes 860 ms. There is no whole-character horizontal mirror, disappearing width or root-position jump. A Turn around button replays it.

Interrupted turns preserve the displayed head/body/tail orientation and cancel queued motion. A later action starts from that orientation. Notification approaches turn toward their travel direction, then finish any turn needed to face the close button before reporting arrival. Paw contact uses the same projection as the drawing.

The original oat/cream character now has a shorter plump body, larger rounded cheeks, small low-set eyes with catchlights, smaller-looking bean paws and a shorter plush tail. Its three forehead marks, simple smile and code-only production remain consistent across desktop, preview and icon.

## Changed files

- `src/CuteCat.Core/TurnTransition.cs`: independent deterministic orientation transition.
- `src/CuteCat.Core/Companion.cs`: queued travel, planted turns, boundary behavior, interruption and final facing at notification arrival.
- `src/CuteCat.Core/CatRig.cs`: turn pose/paw shuffle, orientation parameters, shared projection/contact math.
- `src/CuteCat.App/CatPainter.cs`: softer original contours, non-mirrored body/head/tail projection and consistent icon.
- `CatSurface.cs`, `CatPreview.cs`, `MainWindow.xaml.cs`: shared pose rendering, projected painted endpoint, replay control.
- `QualityChecks.cs`, `tests/CuteCat.Checks/Program.cs`: turn continuity, both boundaries, cancellation, native intermediate poses, cadence and existing recovery/contact checks; diagnostic turn recording and sequence sheet.
- `BuildInfo.cs`, project/manifest and build/run scripts: version 0.5.0 and version-derived packaging.
- `assets/app.ico`, provenance and affected docs/contracts: character revision 2, CAT-10, D27/D28 and rig contract v2.

## Verification

Release compile and **77/77 core checks** passed. **34/34 native candidate checks** passed, including a rendered front view, bounded orientation steps, planted pivot, measured turn cadence, all eleven actions, pointer recovery, reduced-motion exit and practice contact/cancellation. Turning measured approximately **16.67 ms mean / 17.13 ms p95** at 125% scale in the candidate run. See build status for final packaged checks and resource results.

The self-contained **0.5.0 executable also passed 34/34 checks**, with turning at **16.675 ms mean / 17.077 ms p95** and practice contact within **0.16 pixels**. The five-minute movement audit measured **0.66% total-machine CPU, 46.6 MiB private memory and 17 GDI handles**. `./scripts/build.ps1 -Publish`, the packaged `--qa`, `--benchmark ... --benchmark-mode active`, preview renderer and documentation validator were run.

The previous touch/drop tests expected a Walk enum even when reversing immediately at the edge. The current checks require active travel or a queued planted turn within the reaction budget; a turn adds at most 860 ms before translation. Separate edge tests prove that the turn completes and travel resumes. This is an explicit CAT-07/CAT-10 contract change, not an idle delay hidden by a test.

Actual code-rendered character sheets, desktop sizes and turn sequence were inspected on light/dark backgrounds; WPF control exports were inspected. Diagnostic WebPs are recordings of the renderer, not runtime inputs. Native QA calls production handlers; physical cross-process mouse/click-through remains a separate check.

## Delivery / limits / next work

Self-contained portable build: `dist/CuteCat-0.5.0`. Previous source-build evidence remains in `artifacts/qa-v040`; current artwork/candidate checks in `artifacts/qa-v050`, final package evidence in `artifacts/package-v050`, and the five-minute movement audit in `artifacts/performance-v050`.

Saved settings/session schema is unchanged. Preserve the earlier installed build and user state before replacing the running app. Do not stop an actively running focus session to perform the update. The prior automatic Windows-banner test was unverified; this art/turn revision does not establish new shell compatibility. Public-release input/DPI/accessibility QA and future M05 browser protection remain outstanding.

Delivery performed: the old instance was confirmed paused, its binary folder and local state were backed up, and it was restarted into 0.5.0. Settings and paused elapsed progress compared equal after restart. Original binaries are in `dist/CuteCat-0.4.0-rollback`; the old `dist/CuteCat-0.4.0` launch folder was also updated for existing shortcuts. The user-data backup stays under LocalAppData and is not in the repository or package. The restart report confirms version 0.5.0.0 running.
