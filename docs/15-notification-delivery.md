# Windows notification delivery — 0.6.1

The user requested that a cat anywhere on its selected desktop run visibly to a new Windows 11 notification and touch the X before dismissal. The user then explicitly asked to complete this without the computer plugin. The app and its native tests use Windows APIs directly.

## Pipeline

1. The opt-in helper scans at 100 ms intervals on a worker, with one scan in flight. Only verified `ShellExperienceHost.exe` and `ShellHost.exe` paths are considered. Direct `FindWindowEx` class discovery reaches immersive windows omitted by `EnumWindows`; PID-filtered UIA roots remain a fallback. Dormant zero-size roots are skipped.
2. Resolve a visible `ToastView`, `NormalToastView` or `FlexibleToastView`, including when it is the root itself. Reject exact Notification Center history ancestors, including `NotificationCenterGrid`; permit the live `ToastCenterScrollViewer`. Known close IDs must be small, contained and right-aligned. In a flexible toast the header can sit below a hero image. Unknown IDs retain the narrower upper-right Windows close-icon label fallback. Message text is never requested.
3. Require 120 ms of stable geometry, preserving first-seen age. This prevents the shell's entrance animation from immediately invalidating an otherwise valid approach.
4. `NotificationApproach.Plan` computes a valid body position and paw endpoint together. The endpoint can range from (224,50) to (300,229), so a large cat can touch a bottom-edge X without extending below the work area.
5. Start at the actual current position. A cubic-eased straight path takes 0.35–1.55 seconds; urgent direction changes use the same head/body/tail turn over 0.42 seconds. No position reset or teleport occurs. Gait phase remains continuous with a six-cycle/s cap for fast sprints.
6. After arrival and final facing, reach for the computed point. Only a successfully painted contact within 1.5 physical pixels can request dismissal. Revalidate process image/start time, HWND, root/view/button runtime IDs, geometry, visibility, enabled state and cancellation epoch immediately before InvokePattern.
7. Confirm that the targeted close control/view disappears after invocation. Hide/drag/disable, a different/moving target, expiry, lock/suspend and user takeover cancel pending actions. Animation and dismissal remain separate.

## Real test notifications

The prior test used tray-balloon APIs and could not establish modern toast behavior. The new button uses the Windows WinRT toast platform through Community Toolkit Notifications. It registers only this app's own identity and never requests the UserNotificationListener capability.

Microsoft documents elevated senders as unsupported. The coding environment runs elevated; the app therefore relaunches at standard-user privilege when necessary. On systems without a linked limited token, it creates a restricted medium-integrity copy for itself, with a current-user-owned default DACL. It changes no account, UAC policy or existing object permissions. This also makes ordinary launches from an elevated terminal usable. Tests verify the actual child process runs unelevated.

The test distinguishes `Sent`, a discovered visible banner, UIA invocation/disappearance, and Windows' `UserCanceled` dismissal event. Submission alone is not reported as successful dismissal. Windows busy/away/Do not disturb policies can send a test to Notification Center without exposing a banner. The app does not override those settings.

## Verification boundaries

Core tests cover the path, adaptive endpoint, no start teleport, bounded arrival, negative monitor coordinates and every size at 100–250% scale. Native tests cover actual painting/contact and UIA control invocation, options-button rejection, Notification Center exclusion, and refusal to invoke a test app masquerading as the shell. These fixtures validate the pipeline and Windows API integration.

The earlier own-app WinRT test was accepted but suppressed while Windows reported busy. In 0.6.1 the running production helper discovered an actual Windows toast matching the user's Snipping Tool capture, painted contact after 1.865 seconds, invoked the dismiss control and confirmed its disappearance after 3.957 seconds. Evidence: `artifacts/live-v061/notification-observation.jsonl`. This confirms UIA invocation/disappearance, not a WinRT UserCanceled event from the other app.

The standard cat is in desktop z-order band 1; the real Windows toast is in band 4. TOPMOST cannot make the paw visibly overlap it. The user explicitly approved the separate signed UIAccess installation, which now reports UIAccess=true and places the cat in band 2 at native z-order index 0, above the notification. See `16-uiaccess-review.md` for permission scope, expiry and rollback. The earlier standard-build DIB contact record is not evidence of visible overlap; installed window-order evidence and final user feedback are separate checks. Unknown/custom/security notifications and banners outside the selected monitor remain outside scope; no mouse injection, app closing or arbitrary window dismissal is used.
