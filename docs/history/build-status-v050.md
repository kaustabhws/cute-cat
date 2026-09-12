# Current build status

## Version 0.5.0 — 12 September 2026

**Softer original character and a proper animated turn.** The user rejected the immediate left/right flip in 0.4.0 and asked for greater cuteness without realism. This build replaces the flip and refines the code-drawn artwork. The existing native companion, focus sessions, tray, settings and scoped notification helper remain available.

## Changed behavior

- The cat reaches either edge, plants its feet and turns through a round front view over 860 ms. Its head leads, paws shuffle and tail follows. The drawing never mirrors or collapses to zero width.
- Travel waits for the turn. Pet, drag, hide and settings changes cancel queued motion while preserving the displayed orientation. The Your cat page includes **Turn around** replay.
- The original cream/three-oat identity now has a shorter plump body, larger round cheeks, low-set eyes with tiny catchlights, small bean paws and a shorter plush tail.
- Notification approach and return use the same turns. Arrival waits for the final direction; contact uses the actual projected paw endpoint. The controlled practice test still passes.
- Everything remains code-drawn with native WPF and a small Win32 layered surface. There are no generated animation frames, new permissions or runtime services.

## Executed checks

| Check | Actual result |
| --- | --- |
| Release build / self-contained Windows x64 publish | Passed, zero warnings/errors |
| Deterministic core checks | 77/77 passed |
| Native candidate suite | 34/34 passed |
| Packaged executable native suite | 34/34 passed |
| Packaged turn cadence | 16.675 ms mean / 17.077 ms p95, about 60 updates/s |
| Turn continuity | Intermediate front view, bounded orientation steps, fixed pivot, both edges, retarget/cancel and final orientation verified |
| Practice notification contact | Painted before dismissal; approximately 0.16 physical pixels center error |
| Recovery | Tap/drop, lost capture, hidden/reduced motion, moved/disabled target cancellation passed |
| Visual review | Revised action sheet, both themes, 96/128/160 sizes, turn sequence and animation export, native control renders and icon inspected |

The new CAT-07/CAT-10 contract distinguishes the start of autonomous motion from translation: taps/drops resume walking or the required planted turn within the reaction budget; turning adds at most 860 ms before travel. It does not add an idle delay.

## Resource measurement

A five-minute run alternated walking/running and included edge turns, with the control window hidden and notification scanning disabled. Windows build 26200, 8 logical processors, 125% scale:

| Duration | CPU, share of total machine | Private memory | Working set | GDI handles | Native drawing buffer |
| --- | --- | --- | --- | --- | --- |
| 300.03 s | 0.66% | 46.6 MiB | 108.9 MiB | 17 | 259,920 bytes (~254 KiB) |

This is a local measurement of the current renderer, not a universal hardware guarantee. The previous five-minute idle measurement belongs to 0.4.0 and is retained in [its build record](../history/build-status-v040.md).

## Delivery and evidence

- `dist/CuteCat-0.5.0/CuteCat.exe`: self-contained portable executable; keep its folder together.
- The paused 0.4.0 instance was replaced and 0.5.0 restarted. Saved settings and paused progress were compared and preserved. The previous binary folder remains in `dist/CuteCat-0.4.0-rollback`; the former `dist/CuteCat-0.4.0` launch path also contains 0.5.0 for existing shortcuts/startup. See `artifacts/package-v050/restart-check.json`.
- `artifacts/package-v050/native-checks.json`: final packaged scenarios and frame timing.
- `artifacts/package-v050/turn.webp`, `turn-sequence.png`, `character-sheet.png`, `desktop-sizes.png`, `ui/*.png`: diagnostic visuals from the actual code painter / WPF controls.
- `artifacts/performance-v050/performance.json`: exact five-minute resource result.
- [Current handoff](../handoff-2026-09-12-turns.md): files changed, contract changes, commands and remaining work.
- [Getting started](../getting-started.md): launch and controls.

The PNG/WebP recordings are review outputs, not runtime animation inputs. WPF control images use XPS export/rasterization because RenderTargetBitmap returned transparent output in this environment; they are not screenshots of other apps.

## Remaining limits

The current task changes artwork and movement, not Windows shell coverage. The prior real-banner test reported `NoBanner`; automatic shell dismissal remains unverified in this session. Practice success must not be represented as proof of shell compatibility. Unknown/custom/security notifications, Notification Center history, arbitrary window/tab closing and input injection remain outside scope.

Physical mouse click-through/focus preservation, broad mixed-DPI/hot-plug coverage, Narrator/high contrast and Explorer restart still need interactive release QA. Browser protection, public naming, signing and store installation remain future work. The app is local and completely free.
