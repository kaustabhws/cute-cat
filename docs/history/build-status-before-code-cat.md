# Current build status

## Version 0.3.0 — 12 September 2026

The touch/drag freeze and lag report is addressed in the running native build. The prior version's state tests did not establish interaction recovery or actual frame cadence.

- Taps keep a short greeting and resume walking after about 1.1 seconds. Drops show a soft landing and resume after about 0.7 seconds. Quiet, focus and reduced-motion preferences remain intentional exceptions.
- A new original, friendly surprised pickup pose appears during dragging. Lost capture recovers; monitor/settings refreshes no longer cancel the reaction or restart idle delays.
- Revised left/right walking and running art uses twelve source keys per gait, exported as 48 frames per walk and 36 per run. The complete pack has twelve clips and 234 frame entries.
- Native bitmap decoding/scaling and WPF preview decoding run off the UI thread. Playback reuses prepared HBITMAPs and one memory DC. Atomic saves run in a serialized background queue.
- A monotonic high-resolution Windows waitable timer drives active frame updates, with one queued frame maximum and slower idle/hidden cadence. A dispatcher-timer attempt still measured ~27 ms and was replaced.

| Current check | Result |
| --- | --- |
| Core and pointer/recovery checks | 56/56 passed |
| Native motion/interaction/cadence checks | 10/10 passed |
| Existing native notification/session checks | 14/14 passed; correct rendered contact frame; ~0.44px contact rounding error |
| Walking frame updates | 16.668 ms mean, 17.107 ms p95 (~60 updates/s) |
| Running frame updates | 16.668 ms mean, 17.106 ms p95 (~60 updates/s) |
| Warmed walking/running image-cache misses | 0 / 0 |
| Test display scale | 125% |
| Asset/schema/alpha/timing/contact/link validation | Passed |

Timing is measured in the actual native app with its WPF cat preview visible; it is frame-update evidence, not a universal monitor-presentation guarantee. Native motion checks call the production pointer handlers with supplied coordinates. Physical input automation still returned `GetCursorPos failed: Access is denied (0x80070005)`.

A short tray-mode smoke measured idle at 0.14% total-machine CPU / 91.9 MiB private memory and walking at 0.39% / 95.2 MiB. The native cache was about 44.6 MiB. This deliberately trades a bounded amount of memory for prepared frames; the measurement is not the five-minute representative hardware audit.

The current portable build is `dist/CuteCat-0.3.0`. The user's idle 0.2.0 process was restarted into 0.3.0 with saved settings. Its former launch path also contains the updated build so existing shortcuts/startup paths work. The original binary is retained at `dist/CuteCat-0.2.0-rollback-20260912`; the original source art pack remains in `assets/cat`. New art is in `assets/cat-v003`, new prompt/provenance in `art-source/provenance-v003.json`, and current checks/reviews in `art-previews/v003`.

See [the handoff](../handoff-2026-09-12.md) and [motion contract](../13-companion-and-notifications.md). Windows shell compatibility, real multi-monitor input/accessibility QA, and future browser protection retain the limits below. No permission expansion or paid features were introduced.

## Previous milestone: 0.2.0

11 September 2026. **Runnable native Windows companion, version 0.2.0. Completely free.**

The checkout originally contained documentation only. The earlier status claimed a 0.1.0 application, artwork, extension and test results, but those files were absent. This page reports the source, assets and checks actually produced in this checkout. Broader browser requirements remain specifications.

## Implemented in this milestone

- Native C#/.NET 10 WPF app with Today, Your cat, Quiet desktop and Settings; warm light and dark themes, ordinary Windows controls, local nickname and startup opt-in.
- Small Win32 per-pixel-alpha, nonactivating companion window; pet, drag, park, monitor selection, show/hide and a tray menu.
- One original v002 cream/taupe kitten, generated through the authorized Azure image deployment. The character has separate anatomical left/right walk and run art, play/pounce, grooming, settling to sleep, breathing, waking and a reaching paw.
- Ten timed clips with 109 manifest frame entries: original keys, reviewed offline gait in-betweens, breathing deformations and reused wake frames. Source sheets, prompts, cleanup, hashes and provenance are retained. No competitor art or runtime image service.
- Motion acceleration/braking and gait phase tied to displacement. Calm roaming, quiet focus behavior, explicit action replay, reduced motion, optional Windows motion preference, lock/suspend/full-screen suppression.
- Opt-in Windows banner helper: scoped shell toast close-control detection, approach, measured paw contact, exact target revalidation and InvokePattern dismissal. No notification message text, global input injection or arbitrary window closing.
- Clearly labeled practice card plus a button that emits an actual Windows test banner.
- Hide/drag, interaction takeover, target change, expiry, monitor changes and helper disable cancel pending attempts. Animation and session state are separate.
- 25/50-minute and custom 5–180-minute focus sessions; pause/resume/end; optional five-minute break; local today/week totals; paused recovery.
- Atomic local JSON with bounded 30-day records, malformed-state backup and preservation of an unsupported future schema.
- Portable Windows x64 distribution including the .NET runtime at `dist/CuteCat-0.2.0`, solution, build scripts, and test tooling.

## Verification performed

| Check | Result |
| --- | --- |
| Release compilation and self-contained publish | Passed |
| Deterministic core/session/motion/identity/storage checks | 43/43 passed |
| Native integration smoke | 14/14 scenario checks passed; UpdateLayeredWindow succeeded |
| Paw contact | `paw-03.png` was painted before dismissal; contact calculated from actual HWND bounds was within 0.44 physical pixels of the practice close control (rounding tolerance 1px) |
| Cancellation | Practice target did not dismiss after hide or drag; no pending target remained |
| Real Windows banner | User performed the interactive test and reported “The cat dismisses the Windows banner” |
| Native accessibility | Running WPF control tree read successfully |
| Artwork validation | v2 schema, SHA-256, paths, canvas, alpha, isolated components, timing and paw anchor passed |
| Visual review | Actual-size light/dark art sheets; gait in-betweens; icon; all four WPF screens in both themes |
| Documentation links | Passed |

The native smoke uses real windows/rendering and routed WPF control events. It is not a physical mouse or keyboard test. Windows computer-use input returned `GetCursorPos failed: Access is denied (0x80070005)`; screenshot capture returned `IGraphicsCaptureItemInterop.CreateForMonitor ... (0x80070057)`. Actual WPF controls were exported to XPS and rasterized for visual review. The user's real-banner result is recorded separately from these automated checks.

Final artifacts include `art-previews/native-smoke.json`, `art-previews/runtime-metrics.json`, `art-previews/character-review.png`, motion GIFs, and `art-previews/app/*.png`.

A short tray-mode measurement on this 8-logical-processor Windows machine recorded:

| Workload | Duration | CPU, total-machine share | Private memory | Working set |
| --- | --- | --- | --- | --- |
| Idle breathing | 30.02 s | 0.20% | 31.9 MiB | 101.5 MiB |
| Walking | 15.00 s | 0.25% | 30.4 MiB | 101.8 MiB |

This is a short local smoke measurement, not the planned five-minute representative hardware benchmark.

## Limits and remaining release work

Windows shell AutomationIds are not a stable public API. This build dismisses recognized banner controls and leaves unknown/custom notifications alone. Notification Center history, arbitrary in-app popups, UAC/security prompts and unsupported/elevated controls are not targets.

Broader mixed-DPI, hot-plug, physical mouse click-through/focus behavior, Narrator, high-contrast interaction and Explorer-restart testing remain release QA. The local test environment cannot establish those by synthetic control events alone. No claim of perfection across all Windows configurations is made.

The browser extension/native messaging host described in older planning documents is not implemented in this checkout. Browser feed protection, signing, a public installer, store distribution and name clearance are separate future work. The app remains completely free; there is no account or monetization infrastructure.
