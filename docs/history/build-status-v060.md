# Current build status

## Version 0.6.0 — 12 September 2026

**Notification run/reach and Windows API fixes are implemented and packaged.** The resumed work used source edits, command-line builds and native Windows test programs; no computer plugin was used.

## Changed

- The cat starts from its actual position, takes a continuous eased straight run and uses smooth urgent turns. It does not park or teleport to the banner.
- Adaptive body placement and paw reach handle bottom-edge banners, including the large cat size. The native far-corner fixture reached contact in about two seconds.
- A 120 ms entrance-settling check prevents the banner's own slide-in from immediately cancelling an approach. Cancellation/identity/expiry checks remain independent of animation.
- Scoped Win32/UIA discovery covers verified ShellExperienceHost and ShellHost processes. Known close IDs plus a constrained OS close-icon label fallback identify the X; options/history/foreign-app controls are rejected.
- The close action occurs only after painted contact and revalidation, then checks that the target disappears. No mouse injection, cursor movement, arbitrary app close or notification message reading.
- The Windows test button uses a real WinRT toast and reports submission/dismissal separately. A standard-user relaunch handles elevated development launches without changing OS/account/UAC settings.
- Fixed native visibility after hidden launchers and removed the cat menu's dependency on a global cursor query. The cat art, ordinary turns, focus sessions and settings remain intact.

## Checks actually run

| Check | Result |
| --- | --- |
| Release compilation / Windows x64 self-contained publish | Passed, no warnings/errors |
| Core checks | 92/92 passed |
| Standard-user native suite | 44/44 passed |
| Packaged executable native suite | 44/44 passed; actual process unelevated |
| Far-corner, bottom-edge approach | 2.006 s to contact, 122 painted frames |
| Initial position / largest frame step | No initial jump; maximum about 26.7 physical pixels per frame |
| Adaptive large-cat contact | Approximately 0.46 physical pixels center error |
| Direct UIA fixture | Correct control invoked; close-label fallback works; options/history/foreign-app rejection passed |
| Cancellation / recovery | Hidden, drag, disabled/moving/replaced targets, reduced motion and ordinary turn tests passed |
| Visual review | Actual WPF control exports and renderer outputs; current code art remains consistent |

The native suite uses real windows, the production painter/motion engine and an app-owned UIA fixture. It is not a physical mouse test or proof of an observed Windows shell banner.

## Real Windows banner check

The WinRT test ran **unelevated**, was accepted by Windows, and remained **unconfirmed as a visible/dismissed banner**. Windows reported `Busy (2)`; the shell scan returned `NoBanner`. See `artifacts/shell-v060/shell-check.json`. The app does not change Do Not Disturb or other Windows notification policies. A received toast or Notification Center entry is not reported as a successful paw dismissal.

Consequently, this build has verified movement, contact, cancellation and native UIA invocation, while **real-shell dismissal still needs confirmation when Windows exposes a visible banner**. Unknown layouts/locales, inaccessible/security notifications and banners outside the selected monitor are left alone.

## Files and delivery

- `dist/CuteCat-0.6.0/CuteCat.exe`: runnable self-contained Windows x64 build.
- Installed locally and started with the notification helper enabled, following the user's request. Cat visibility, nickname and saved session progress were preserved. The 0.4/0.5 launch paths also contain the update; original 0.5 binaries remain in `dist/CuteCat-0.5.0-rollback`. See `artifacts/package-v060/restart-check.json`.
- `artifacts/package-v060/native-checks.json`: final packaged checks and exact timings.
- `artifacts/package-v060/ui/*.png`: renders from actual WPF controls; no desktop screen capture.
- `artifacts/qa-v060-final`: development native results and visual outputs.
- [Notification delivery contract](../15-notification-delivery.md), [current handoff](../handoff-2026-09-12-notifications.md), [getting started](../getting-started.md).
- [Previous build record](build-status-v050.md): earlier performance/art history, not fresh 0.6.0 measurements.

The package includes the MIT notice for Community Toolkit Notifications and preserves Microsoft runtime notices. It is an unsigned local build; public naming/signing/store installation and browser feed protection remain future work. The app stays completely free and local.
