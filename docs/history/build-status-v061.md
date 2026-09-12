# Current build status

## Version 0.6.1 — 12 September 2026

**Real Windows notification detection/dismissal confirmed. The user-approved UIAccess installation now places the cat above the protected notification layer.** All work used source, CLI and native Windows APIs, without the computer plugin.

## Actual fix

- Direct `FindWindowEx` discovery reaches immersive shell windows that `EnumWindows` and desktop UIA enumeration omitted on this machine. Zero-size dormant roots are skipped.
- The real Snipping Tool capture exposed `NormalToastView` / `FlexibleToastView` inside `ToastCenterScrollViewer`. The old resolver only recognized `ToastView`.
- The real dismiss button was below a notification image, outside the old top-80-DIP check. Known close IDs in the flexible template now require a small, contained, right-aligned control. Broad action/settings buttons remain excluded; the label fallback keeps its narrow corner rule.
- Live scrolling toast containers are accepted. Exact Notification Center history containers, including the observed `NotificationCenterGrid`, remain excluded.
- Existing current-position travel, continuous turns, painted paw contact, exact target revalidation and cancellation are preserved.

## Executed evidence

| Check | Result |
| --- | --- |
| Release compilation / standard Windows x64 publish | Passed |
| Core suite | 92 passed, 0 failed |
| Native suite | 52 passed, 0 failed, including the observed template and geometry |
| Live production helper, ordinary user token | Detected real Windows toast and confirmed disappearance after invoking its dismiss control |
| Live start to first movement | 64.3 ms |
| Live start to painted paw contact | 1.865 s |
| Live contact center error | 0.157 physical pixels |
| Live maximum frame step | 14.45 physical pixels |
| Live start to confirmed dismissal | 3.957 s, 177 painted journey frames |
| Native measured walk/run cadence | Mean ~16.66 ms; p95 ~16.70 / 16.68 ms |
| Approved UIAccess installation | Signed payload installed in Program Files; actual runtime UIAccess=true; cat band 2 at native z-order index 0 above notification band 4 |
| Live installed run with real notification | Cat above the toast in all 28 visible-toast samples, including 11 overlapping samples; notification disappeared, then cat returned |

The live record is `artifacts/live-v061/notification-observation.jsonl`, from the running production helper against a Windows shell toast. It records structure/geometry, not notification content. Disappearance after UIA InvokePattern is confirmed; no WinRT `UserCanceled` event is available for another app's toast. This is distinct from the app-owned test toast, which Windows previously suppressed while busy.

The standard native suite is `artifacts/qa-v061/native-checks.json`. Actual WPF/renderer outputs are under its `ui` and art folders. The rig and artwork are unchanged in this fix; no screenshot is used as proof of control identity or dismissal.

## Windows layer and approved local installation

Read-only native diagnostics found the cat in desktop band 1 and the Windows notification in band 4. Ordinary TOPMOST does not cross this boundary. The standard build can run and dismiss, but its paw/body may be hidden where they overlap the notification.

The user explicitly selected **Install the UIAccess test build** after reviewing its broader permission and machine-wide certificate trust change. The signed package is now installed at `C:/Program Files/CuteCat-Local-0.6.1-38FF99FF`. Its actual token reports UIAccess=true, and native enumeration places the cat first in band 2, above the Windows toast in band 4. Evidence: `artifacts/layers-v061-uiaccess.json`; bounded live window geometry/order observation: `artifacts/layers-v061-uiaccess-live.jsonl`.

The installed process's foreground layer is verified during a live notification. The native observer captured the cat running from x=244 to x=1611, staying above the toast in all 28 visible-toast samples (11 with overlapping window rectangles), followed by banner disappearance and the cat's return. Sampled visible duration was 4.67 seconds. See `artifacts/uiaccess-live-summary-v061.json`. This observer measured window geometry/order, not a separate UIA invocation result or notification dismissal event; the earlier production invocation/disappearance record is separate evidence. User visual feedback was requested and is not invented. No undocumented band creation, injected input, token theft or security-policy bypass is implemented.

The public signing certificate was added to machine roots with explicit consent; the private signing key was deleted after packaging. The certificate expires **12 October 2026**. This local test installation needs replacement or removal by then. Microsoft discourages UIAccess for ordinary non-accessibility apps. See [access scope and rollback](../16-uiaccess-review.md); it is not a public-release recommendation.

## Delivery

- Running installed build: `C:/Program Files/CuteCat-Local-0.6.1-38FF99FF/CuteCat.exe`. Existing local settings and paused focus progress were preserved; a before-update state backup exists in the user data folder.
- The desktop Cute Cat shortcut opens that installation. `scripts/run.ps1` prefers the matching installed build while its signature is valid/trusted; `-Standard` selects `dist/CuteCat-0.6.1/CuteCat.exe`. Previous portable versions remain available.
- [Notification contract](../15-notification-delivery.md), [current handoff](../handoff-2026-09-12-toast-layout.md), [getting started](../getting-started.md).
- [Previous 0.6.0 evidence](build-status-v060.md) is historical, including its then-unconfirmed shell gate.

The standard package remains an unsigned local alpha. Naming/signing for public distribution, browser protection and the broader support matrix remain future work.
