# Handoff — Windows notification fix 0.6.0

## Task / requirements

NOT-01–06, CAT-07/08/10. The user wants the cat to visibly run from wherever it is to a Windows 11 banner, touch its X and dismiss it. The resumed request explicitly asks to finish without the computer plugin. This resumed implementation used code, command-line builds and native test executables only.

## Implemented

- New deterministic notification planner: current position is unchanged when a target is accepted; cubic-eased direct travel takes 0.35–1.55 s, urgent turns 0.42 s, and the procedural running gait stays continuous.
- Solve body position and paw endpoint together. This fixes the old rejection of low banners when the fixed high reach would put the cat below the work area.
- Stabilize entrance geometry for 120 ms before beginning. Preserve first-seen expiry and cancel on user takeover, hide/drag/disable, replacement/movement or expiry.
- UIA/Win32 discovery of verified Windows shell processes. Exact process start time, HWND and root/view/button identities prevent stale/reused targets. Check close-icon role/corner geometry and use known IDs or narrowly scoped OS close-icon labels. Exclude Notification Center history.
- After actual rendered contact, revalidate and invoke that button through InvokePattern; confirm disappearance. No injected input, cursor movement, arbitrary app/window close or notification message reading.
- Real WinRT test notifications using Community Toolkit 7.1.2 with its MIT notice, separate submitted/dismissed outcomes and visible test status. No UserNotificationListener permission.
- Standard-user relaunch for elevated starts, including a restricted medium-integrity fallback for development accounts without a linked token. Correct current-user object ownership/default DACL; no OS or account security setting changes.
- Explicit native Show handling survives a hidden launcher; cat context menus use their window-message coordinates rather than a global cursor query.

## Files

`NotificationApproach.cs`, `Notifications.cs`, `Companion.cs`, `TurnTransition.cs`, `CatRig.cs`; native `ShellNotifications.cs`, `WindowsTestNotification.cs`, `StandardUserProcess.cs`, `CompanionHost.cs`, `PawJourneyMetrics.cs`, `CatSurface.cs`, `MainWindow.xaml.cs`; `NotificationControlFixture.cs`, `QualityChecks.cs`, core checks and the development-only `tests/CuteCat.ShellProbe`; project/manifest, third-party notices, requirements, architecture, privacy, source register, rig/delivery contracts, build status and getting started.

## Checks run

Release build and Windows x64 self-contained publish succeeded. **92/92 core checks** passed. The standard-user native suite passed **44/44 checks**, including actual UIA invocation on a test-owned close control, fallback label recognition, options rejection, Notification Center exclusion, rejection of a spoofing app, visibility, motion/contact and cancellation.

The final self-contained **0.6.0 package also passed 44/44 checks**, reporting `elevated: false`. Its distant approach reached contact in **2.006 s** with **122 painted frames** and **0.458 px** error. The user did not need to use a computer plugin for these checks.

The far-corner bottom-edge fixture completed contact in about **2.00 seconds**, with **122 painted frames**, maximum frame displacement about **26.7 physical pixels**, no initial teleport and approximately **0.46 px** contact error at the large cat size. Planner tests cover 96/128/160 sizes and 100–250% DPI, plus negative-coordinate monitors.

The real WinRT diagnostic ran unelevated, submitted successfully, and Windows reported notification state **Busy (2)**. No visible shell banner was exposed, so real-shell dismissal is **not claimed as confirmed**. Submission/Notification Center history is not equivalent to a visible/dismissed toast. The app does not override Windows notification policy. The earlier computer-use attempt was stopped by the user; no such plugin was used in the resumed fix.

## Evidence / limits

`artifacts/qa-v060-final`, `artifacts/package-v060`, `artifacts/shell-v060`; diagnostic screenshots are app-owned control renders, not desktop screen captures. `docs/15-notification-delivery.md` defines the new contract. No new performance claim is inferred from earlier versions' five-minute measurements.

Remaining: confirm a visible real Windows banner in an ordinary interactive session; additional Windows shell layouts/locales, actual multi-monitor/input/accessibility release QA. The selected-monitor boundary remains intentional. Next product work after this companion verification is the previously planned M05 browser extension/native messaging slice. No browser blocking is claimed.

Delivery: 0.6.0 is running with the helper enabled under the user's explicit request. Nickname/session progress were compared and preserved. The previous 0.5 build and local state were backed up; old 0.4/0.5 launch directories also receive 0.6 for existing shortcuts. User-data backups remain in LocalAppData, not in the package or repository.
