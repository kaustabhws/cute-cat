# Handoff — code-drawn companion 0.4.0

## Task / requirement IDs

CAT-01–09, NOT-01–03, FREE-01; companion/session portions of M01–04/M06/M08/M09. Latest explicit user request supersedes the hand-painted raster pipeline with original code art and smooth continuous animation. Browser protection remains subsequent work under M05.

## Final behavior

An original oat-coloured block-like cat is painted with cubic paths in a small nonactivating Win32 window. A continuous rig drives walk/run, blinking, breathing, grooming, sleeping/waking, meow, play, pickup, landing and measured paw contact. WPF provides focus, cat, notification and settings pages in light/dark themes, plus tray controls. The app is local and free.

Focus sessions support 25/50/custom durations, pause/resume/end and five-minute breaks; only completed focus contributes to local totals. Atomic state is bounded. Prior flat Version 1 state is backed up and migrated; unrecognized formats are preserved read-only. Lock/suspend recovers paused. Cat visibility, quiet mode, reduced motion and selected monitor remain independent from session timing.

Automatic paw dismissal is opt-in and restricted to known shell toast close controls. Approaches revalidate identity, bounds, epoch and age; successful painting/contact precedes InvokePattern. Practice is explicitly app-owned. The current real-banner test found no supported banner and did not invoke one; this remains a real-desktop compatibility gate.

## Files changed

- `src/CuteCat.Core`: deterministic geometry, rig, movement, pointer/notification state, focus sessions, atomic storage/migration.
- `src/CuteCat.App`: WPF controls/theme, native surface/window APIs, waitable frame clock, shared painter/preview/icon, tray, scoped shell adapter, practice card, synthesized optional sound and native checks.
- `tests/CuteCat.Checks`: deterministic transition, identity, cancellation, recovery and migration checks.
- `scripts`: build/publish/run, diagnostic XPS/WebP rendering and documentation validation.
- `assets/app.ico`: icon generated from the original code geometry, with provenance.
- `docs`: current art direction/production, rig contract, companion behavior, requirements/plan/architecture/decisions, current sources, startup instructions, build status and this handoff. Earlier sprite descriptions preserved under `docs/history`.

## Checks actually performed

- `.NET 10.0.400` Release compilation; self-contained Windows x64 publish.
- `dotnet run --project tests/CuteCat.Checks -c Release --no-build`: **61 passed**.
- Native `--qa`: **29 passed**, including production pointer handlers, focus controls, moved target/disable cancellation, actual layered painting, all ten actions, reduced-motion cadence and resuming 60 Hz. The earlier failing reduced-motion transition was fixed by resetting timer deadlines and waking the worker on rate changes.
- Native practice contact error approximately **0.16 physical pixels** at 125% scale. This is controlled geometry/integration evidence, not a physical mouse test.
- All four WPF pages exported in both themes, plus the minimum-size layout; original renderer sheets at 96/128/160 sizes and ten actions inspected. XPS used because WPF bitmap export was transparent in this environment. The small window scrolls vertically for lower controls.
- Five-minute idle and moving workloads, with private/working memory and GDI counts; current results linked from build status.
- Published `CuteCat.exe` ran the same native suite: **29/29 passed**. Five-minute final measurements: **0.25% CPU / 34.4 MiB private idle**, **0.74% / 47.1 MiB moving**, 17 GDI handles in each case, with the control window hidden.
- Real Windows test emitted via the tray; shell scan reported `NoBanner`, no target and no invocation. No message text/screen contents collected.

## Visual evidence

Current source-build artifacts are in `artifacts/qa-v040`: character sheet, desktop sizes, icon, action WebPs, WPF control PNGs/XPS and native-checks JSON. `artifacts/shell-v040/shell-check.json` records the real-banner limitation. Performance artifacts record exact durations and workload, not universal hardware guarantees.

## Limitations / next task

Verify real Windows banner detection/dismissal on the normal desktop, physical mouse alpha click-through and focus preservation, Narrator, high contrast, Explorer restart, mixed DPI/monitor hot-plug and startup behavior. No universal shell compatibility claim. The portable build is unsigned and no public name/store release is cleared.

Next dependency-ready product work after companion feedback: M05, an explicit Edge/Chrome Shorts extension plus native messaging, document identity, reversible shield and cleanup. Existing browser contracts are design, not implemented code. Do not import a competitor renderer or reinstate generated image frames to expand animation.

## Contract / decision changes

D21–26 and CAT-09: code-only character and rig contract replace raster frame contracts. D23 retains a high-resolution clock but uses a reusable drawing buffer. D24 makes reduced-motion suspension of paw dismissal explicit. D25 preserves old data. D26 separates practice proof from real shell compatibility. Research sources remain dated vendor observations.
