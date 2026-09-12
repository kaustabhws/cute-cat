# Companion and notification contract

Updated 12 September 2026 for version 0.3.0. The free app is C#/.NET 10, WPF controls, and a small nonactivating Win32 layered cat window. No cloud service or image-generation key is used at runtime.

## Motion

`CuteCat.Core.Companion` accepts a monotonic timestamp, a physical monitor work area, scale, and a bottom-center position. It does not access UI, timers, screenshots, processes or Windows APIs. The adapter advances it on the UI dispatcher. Walk/run phase is derived from distance travelled, with acceleration and braking. Normal roaming stays near the selected work-area edge. Explicit play, groom, sleep, wake and pet actions preempt autonomous motion.

The manifest defines authored key timing, loop entry and a shared 384px canvas. The foot pivot is (192,354). The measured paw contact is (348,69) in `paw-03.png`; the adapter converts its offset to physical pixels using cat size and monitor scale. No bitmap flipping is used. Sleeping breathes using one painted pose; waking reverses the registered settling poses. Reduced motion keeps still poses and preserves dismiss controls. Full-screen, lock, suspend and hide suppress the companion; lock/suspend pause focus sessions.

### Touch, drag and frame pacing (0.3.0)

`PointerGesture` is the shared production input path used by Win32 handlers and deterministic tests. A press holds the cat still; movement beyond five scaled pixels begins dragging and shows a new, original surprised pose. Releasing a tap keeps a brief greeting and resumes walking after 1.1 seconds. Releasing a drag shows a soft landing and resumes after 0.7 seconds. Quiet/focus/reduced-motion preferences continue to suppress autonomous walking. Lost capture cancels the press cleanly. Monitor and preference refreshes are idempotent and do not erase an interaction or restart an idle deadline.

The prior bug reused the explicit 15-second parking delay after a drag, and even a tap invoked monitor refresh, which cancelled its paw pose. Its following idle state could then choose grooming/sleep. Touch and drop now have explicit, bounded recovery instead of entering that long schedule.

Frames are requested from a worker-prepared native bitmap cache. Playback reuses one memory DC and prepared HBITMAPs; it performs no PNG decoding, scaling, or bitmap allocation. WPF animated previews also decode asynchronously, status text is throttled, and JSON saves use a serialized background queue. The native cache has a 64 MiB base budget, increasing only enough to hold one full clip at large physical sizes; obsolete size requests are dropped.

A high-resolution Windows waitable timer schedules monotonic 16.667 ms active deadlines, with at most one queued UI frame. Idle and hidden states use slower cadence. This replaced the WPF dispatcher timer after measurement showed it coalescing to roughly 27 ms despite a 16 ms request. Movement tests gate measured cadence, not the configured interval. Source pack `assets/cat-v003` has 12 clips / 234 frame entries, including 48 frames per walk and 36 per run, plus drag and land. It is packaged as `assets/cat` beside the executable.

## Notification lifecycle

1. The user enables the helper through its explanatory checkbox. Default is off. No OS privacy setting is changed.
2. The worker enumerates visible native windows and restricts traversal to known Windows shell host classes, `ShellExperienceHost`, and its executable inside `%WINDIR%\SystemApps`.
3. It searches only for `ToastView` containers and close buttons with expected automation IDs and InvokePattern. It does not request Name, Value, TextPattern, message content or screenshots. Unknown controls are ignored.
4. An ephemeral target captures HWND, PID, container/button runtime IDs, bounds, epoch and an eight-second maximum lifetime. Targets are never logged or persisted.
5. The cat approaches the measured contact point and plays the paw clip. At contact, the adapter rescans and requires the same identity, process, visible/enabled state, close geometry (within 3px), age and enabled epoch.
6. Only that close control's InvokePattern is invoked. No simulated click, SendInput, window close message, ClearAll notification operation, tab closing, process termination or administrator service is used.
7. Hide, drag, pet/play takeover, changed/missing target, monitor changes, suspend, expiry and helper disable cancel the pending attempt. Cancelled identities remain ignored until that banner disappears. Platform invocation already underway cannot be recalled, so the checks occur immediately before invocation.

## Compatibility and evidence boundaries

Windows does not document shell toast AutomationIds as a stable public contract. This adapter is intentionally narrow and may leave unsupported banners alone. Notification Center entries, arbitrary custom in-app popups, UAC/security prompts, inaccessible/elevated windows, and banners without the recognized close control are not targets. Never broaden this silently to name matching or arbitrary accessibility scraping.

Microsoft's supported UserNotificationListener can remove notifications by ID, but requires manifest capability and user-granted notification access; it does not give the banner close button's screen geometry. A packaged listener would be a separate permission and architecture change, not an invisible fallback.

The app-owned practice card exercises the real motion, native drawing, measured contact and cancellation code. It deliberately has a `practice:` identity and a clearly labeled UI. Passing it proves the controlled pipeline, not real shell compatibility. A separate button emits a Windows test banner through the app's tray icon so interactive compatibility can be tested.

## Tests

Core checks cover acceleration, distance-linked gait, both directions, negative work areas, target supersession, expiry, cancellation, reduced motion, exact identity, PID/window reuse, moved geometry and disabled/inaccessible controls. Native smoke checks exercise every clip, the actual WPF timer buttons via routed events, UpdateLayeredWindow, practice contact frame/alignment, and hide/drag cancellation. See build status for results and environment limitations.
