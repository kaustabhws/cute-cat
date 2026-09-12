# Current build status

## Version 0.4.0 — 12 September 2026

**A runnable native Windows companion with an original cat drawn and animated entirely in code.** The latest user request supersedes the hand-painted image-frame pipeline. At task entry this checkout contained documentation only; earlier source/art/test claims were not verifiable here. Historical status is preserved in [the prior record](../history/build-status-before-code-cat.md), not treated as evidence for this build.

## Implemented

- Original cream/oat cat with rounded body, ink contour, tiny face and three forehead marks; all geometry, motion and optional sound are authored in C#.
- Continuous walking/running in both directions, breathing/blinking, grooming with a visible raised paw, curled sleep/wake, meow, small playful hop, pickup/landing and reaching paw. Quintic pose transitions and distance-linked gait; no generated animation frames, raster cache, WebView or game engine.
- Small nonactivating Win32 layered window, zero-alpha exterior, click/drag/right-click, park, show/hide, monitor selection, quiet focus, reduced motion, lock/suspend/fullscreen suppression and tray controls.
- Native WPF Focus / Your cat / Quiet desktop / Settings pages, light/dark/system appearance, nickname and size, startup opt-in and optional synthesized meow.
- 25/50/custom 5–180-minute focus sessions, pause/resume/end, optional five-minute break and local today/week completed totals.
- Atomic bounded JSON, previous-version backup, malformed/future format preservation and migration of discovered legacy Version 1 state with a separate original backup. Active sessions restore paused.
- Opt-in Windows banner compatibility adapter: known shell host/control IDs, no message reading, eight-second target lifetime, actual rendered paw contact and immediate identity/geometry/epoch revalidation before InvokePattern. The practice card uses the same motion/contact pipeline. **Real Windows banner dismissal is not yet verified in this session.**
- Self-contained portable x64 build in `dist/CuteCat-0.4.0`. No administrator or installed .NET runtime required; unsigned local build.

## Verified

| Check | Actual result |
| --- | --- |
| Release compilation | Passed, zero warnings/errors |
| Deterministic core checks | 61/61 passed |
| Native source-build checks | 29/29 passed |
| Self-contained packaged app checks | 29/29 passed through the published executable |
| Native walk cadence | 16.671 ms mean / 16.991 ms p95 |
| Native run cadence | 16.669 ms mean / 16.892 ms p95 |
| Practice paw | Painted contact before dismissal; approximately 0.16 px center error |
| Recovery/cancellation | Tap/drop/lost capture; hide; moved target; disabled helper; reduced-motion exit passed |
| Rendering | All ten actions painted through UpdateLayeredWindow; zero-alpha padding; hidden rendering stopped |
| Native drawing buffer | 259,920 bytes (~254 KiB) at 128-DIP design size / 125% scale |
| Visual review | Action sheet, 96/128/160 sizes on light/dark, original icon, all four pages in both themes and minimum-size vertical scrolling |
| Documentation | Current links and documentation JSON checked by `scripts/validate-docs.py` |
| Real Windows banner | Test emitted; `NoBanner`; no target or confirmed invocation |

Timing measures delivered native frame updates on this machine, not universal monitor presentation. The expanded tests discovered a reduced-motion timer-deadline bug; the current frame clock resets deadlines and wakes on rate changes. Pointer checks call production handlers with supplied coordinates, not physical mouse injection. They cannot prove cross-process click-through or uninterrupted typing.

## Resource measurements

Five-minute idle and walking/running measurements run with the control window hidden, Windows build 26200, 8 logical processors, 125% scale. Final clock measurements are stored in `artifacts/performance-idle/performance.json` and `artifacts/performance-active/performance.json`. CPU is the process share of total machine capacity; private memory and working set are distinct. These are local measurements, not a claim about all hardware.

| Workload | Duration | CPU, total machine | Private memory | Working set | GDI handles |
| --- | --- | --- | --- | --- | --- |
| Idle / breathing | 300.02 s | 0.25% | 34.4 MiB | 96.7 MiB | 17 |
| Alternating walk / run | 300.01 s | 0.74% | 47.1 MiB | 109.2 MiB | 17 |

The two final workloads run in separate isolated app processes during the same five-minute period; other QA/build activity also occurred. No notification scanning is enabled for these resource cases. Resource budgets with the helper enabled on a busy real shell remain release QA.

## Evidence and build

- `artifacts/qa-v040/native-checks.json`: current source-build scenarios and timing.
- `artifacts/package-qa/native-checks.json`: the same 29 checks run from the self-contained executable.
- `artifacts/qa-v040/character-sheet.png`, `desktop-sizes.png`, action WebPs and `ui/*.png`: visual review.
- `artifacts/shell-v040/shell-check.json`: real-banner limitation; no message text logged.
- `dist/CuteCat-0.4.0/CuteCat.exe`: portable entry point; keep its distribution folder together.
- [Current handoff](../handoff-2026-09-12-code-cat.md) and [getting started](../getting-started.md): commands, files and next work.

WPF bitmap export returned transparent output in the automated session; actual control XPS exports were rasterized for review. The UI images are control renders, not screenshots of the user's screen. The cat sheets/recordings are direct output of its production code painter and are not used for runtime playback.

## Remaining limits

The Windows shell close-control layout is not a stable public API. Unknown/custom/inaccessible banners and out-of-reach targets are left alone. Notification Center history, arbitrary popups, security prompts, app termination, tab closing, input injection and notification text collection are outside scope. An automated `NoBanner` result may reflect session restrictions or notification policy; it is not a success claim. A prior user's report concerned another build.

Physical click-through/focus preservation, Narrator, high-contrast interaction, multi-monitor mixed DPI/hot-plug, Explorer restart, clean-profile startup and representative hardware coverage remain release QA. Browser extensions/native messaging, feed shields, signing, a public installer, store distribution and naming clearance are separate future work. The app remains completely free.
