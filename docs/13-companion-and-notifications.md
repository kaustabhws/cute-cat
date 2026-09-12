# Companion and notification contract

Version 0.6.1, 12 September 2026. The current follow-up recognizes the actual Windows 11 flexible toast template and has confirmed a live dismissal without the computer plugin. The user-approved [UIAccess installation](16-uiaccess-review.md) places the cat above Windows notifications; the standard portable build retains ordinary layering. See the [notification delivery contract](15-notification-delivery.md). Historical sprite timing/pivots are superseded by [the rig contract](contracts/code-cat-rig.json).

## Deterministic motion

`Companion` accepts monotonic timestamps, physical monitor bounds, size and explicit actions. It has no dependency on UI, Windows, browser processes or notification message text. `CatRig` maps action, age, gait phase and time to continuous pose parameters. `CatPainter` maps that pose to cubic paths. All action changes preserve the preceding pose and blend over 320 ms.

`TurnTransition` supplies continuous body/head/tail orientation from +1 (right) through 0 (front) to -1 (left). It takes 860 ms; the head leads, body follows after 100 ms, and the tail trails after 140 ms. The body retains a round cross-section at the front, the head stays full-width and depth separates the paws. There is no negative horizontal scale transform. Movement waits while the feet pivot in place. A boundary is reached before turning; the old code reversed early and mirrored immediately.

Turns are interruptible: pet/drag/hide/park/configuration changes cancel queued travel and keep the actual partial orientation. A new move turns smoothly from there. Tap/drop recovery resumes travel or the needed turn within its previous reaction budget; the turn adds up to 860 ms before translation. The Your cat page includes an explicit Turn around action.

Gait phase follows displacement, with acceleration/braking and a linear stance segment. The frame clock uses a high-resolution Windows waitable timer and queues at most one UI frame. Active actions target 60 updates/s; idle/sleep 15; reduced motion 1; hidden/suppressed 0. The single native 32-bit DIB is reused, with one DC and cached pens/brushes. No frame decoding, bitmap swapping or image-generation requests occur during playback. UI preview drawing is capped at 30 Hz and stops when the control window is hidden.

The native overlay is a small topmost layered tool window with `WS_EX_NOACTIVATE`. Transparent exterior pixels remain zero alpha. Input uses its own window messages and signed coordinates, not global hooks or injected clicks. Down holds position; movement past five scaled pixels drags; release gives a greeting or landing. Taps resume autonomous motion within two seconds and drops within one, unless quiet/reduced/focus settings apply. A required turn is motion, rather than an idle delay. Lost capture releases the gesture. Monitor/settings refresh is idempotent.

Normal roaming follows the chosen work-area bottom. Accessible controls select a monitor, park, show/hide and perform all actions. Lock/suspend pauses focus; fullscreen hides the companion. Actual mixed-DPI physical input, hot-plug and Narrator require release QA.

## Notification lifecycle

1. Automatic helper is off by default; enabling its explanatory product checkbox authorizes dismissal of supported visible Windows banners. Migrated prior permission is retained.
2. A serialized worker scans at 100 ms intervals, considering only verified ShellExperienceHost / ShellHost binaries and known shell window classes. PID-filtered direct UIA roots supplement Win32 discovery.
3. It searches visible `ToastView`, `NormalToastView` and `FlexibleToastView` containers for known close IDs. Flexible toasts may place a small right-aligned dismiss button below a hero image. A constrained fallback may read the Windows close icon's accessibility label at the upper-right corner. Message text, TextPattern, Value and screenshots are never read. Exact Notification Center ancestors, including `NotificationCenterGrid`, are excluded; live `ToastCenterScrollViewer` is permitted.
4. An ephemeral candidate holds process start time, HWND/PID, root/container/button runtime identity, rectangle, first-seen time and cancellation epoch. Require 120 ms of stable entrance geometry and retain the eight-second lifetime. No notification identity is normally persisted or logged.
5. The body position and paw endpoint are solved together. Start from the actual current position, use an eased straight run lasting 0.35–1.55 s, and use 0.42 s urgent turns. The paw endpoint adapts to bottom-edge targets. `Arrived` remains false until travel and final facing are complete; reaching holds at ages 0.48–0.82 seconds.
6. After `UpdateLayeredWindow` succeeds and the painted endpoint is within 1.5 physical pixels, revalidate image path/process start, HWND, runtime IDs, geometry, visibility, enabled state and epoch. Invoke that control and confirm its disappearance. No cursor movement or simulated mouse click occurs.
7. Hide, drag/touch, action takeover, disable, missing/moved/replaced target, monitor change, lock/suspend and expiry cancel pending work. A cancelled identity is ignored until it disappears. An OS invocation already in progress cannot be recalled.
8. After a completed tap, the cat returns by continuous motion. A newer interaction invalidates the delayed return.

Reduced motion suspends automatic paw approaches; the UI says so. The user can still close notifications normally. This explicit behavior supersedes earlier notes about preserving animated dismissal in reduced mode. There is no automatic fallback with broader notification-history access.

## Evidence and compatibility

The app-owned practice card uses the same motion, painting, contact and cancellation pipeline, with a clearly marked practice identity. Its close rectangle is read from its actual WPF button in screen coordinates. It never establishes shell compatibility on its own.

The Windows test button now creates a real WinRT toast under standard-user privileges, rather than a legacy tray balloon. It tracks submission and the OS dismissal reason separately. The current real-shell check was unelevated and submitted successfully but returned `NoBanner` while Windows reported busy. It does not prove live dismissal. Native UIA fixture/contact checks are reported separately. Historical user reports from other builds are not attributed to this version.

Shell automation IDs are undocumented and may change. Notification Center history, custom app popups, security/UAC prompts, other monitors' unreachable controls and unknown shell hosts are not targets. Microsoft UserNotificationListener requires separate capability/consent and exposes notification IDs but no close-button geometry. Do not silently add it, use generic close messages, simulate input, close tabs or terminate processes.

See [build status](build-status.md) and [Windows notification research](research/04-windows-notifications.md) for executed checks and remaining work.
