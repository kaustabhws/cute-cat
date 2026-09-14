# App guard repair — 1.1.2

The user reported that selected applications did not close, named Android Studio and clarified that the problem affected other apps too. The repair was installed locally first; the user authorized publishing its source and installer on 14 September 2026. See [release notes](releases/v1.1.2.md).

## Confirmed failures and changes

1. **Custom window captions were rejected.** The inspected Android Studio `SunAwtFrame` returned a successful `WM_GETTITLEBARINFOEX` query with empty button rectangles and no usable UIA close button. Windows nevertheless returned `HTCLOSE` for its actual native close area. A read-only probe of the Codex window also resolves through native hit-testing. These are generic frame capabilities, not executable-name exceptions.
2. **Notification scanning could block every app-rule check.** The same asynchronous loop awaited shell UIA discovery before it could scan the next foreground app. A deliberately stalled notification query reproduced the failure for both a standard window and a custom-caption window, despite valid rules added through the actual UI. Shell discovery now has its own single in-flight task, polled without blocking app detection. Late results from cancelled epochs are discarded. There is no growing queue of accessibility tasks.
3. **Dialog-based main windows were blanket-rejected.** The old `#32770` exclusion also removed legitimate main windows used by desktop utilities. Such a window is now eligible only when it is unowned, has a native minimize box and is the process's sole visible non-tool main window. Owned dialogs and ordinary message/save-style dialogs remain ineligible. The native fixture covers both an allowed main dialog and a rejected prompt-style dialog.

`NativeCaptionLocator` starts with a DWM caption hint or bounded caption-corner probes, accepts only Windows `HTCLOSE` (20), measures the contiguous hit region and rechecks its centre/corners. A hint alone never authorizes a close. Limit: 64 queries, 20 ms per native send and a 120 ms probe budget. Geometry, process identity, foreground, current rule/epoch and close availability are revalidated. Disabled system-menu close commands, unresponsive probes, client-area controls, maximize/minimize results and implausible regions are rejected.

The existing normal `SC_CLOSE` or supported accessibility close action remains after painted paw contact. No process kill, injected mouse/keyboard input, document reading, screenshot/OCR or app-specific coordinate guess was added. Save/exit prompts and close vetoes stay with the user. Unsupported frames and apps outside the selected cat monitor remain bounded by the existing scope.

App guard now distinguishes no selected rules and unavailable foreground state. Notification status reports a slow control query instead of indefinitely claiming to be watching normally.

## Verification

- The new `--app-guard-checks --app-fixture PATH` test opens the real Add app dialog, selects the running fixture, enables the guard, uses the actual foreground window and enables the production settling/fullscreen logic. Both helpers are enabled while notification discovery is deliberately left pending. Standard and custom-caption windows close after paw contact with **no foreground override** in these flows.
- Separate refusal/disabled-close safety fixtures explicitly override foreground identity; they are reported separately. They assert one normal close request for a veto/save prompt and none for a disabled close command.
- `--guard-events-only` explicitly enables a foreground override for the full UI flow when the desktop cannot provide activation; its report identifies that override. The original standard/custom flows passed without it. A later RDP run had no foreground window, so the expanded three-frame matrix uses this explicit controlled mode, including in CI.
- Late cancelled notification observations cannot start a paw; only one shell scan remains in flight.
- Deterministic tests cover native close geometry, DPI/negative coordinates, query limits, timeout, invalid geometry, other hit-test results and DWM hints.
- `AppControlProbe --app-controls PID OUTPUT` is an explicit read-only developer tool: structural IDs, geometry and native hit results for one supplied process. It does not record titles, names or user content. Probe outputs remain local under ignored artifacts.

Before: `artifacts/app-guard-blocked-notifications-before` failed both end-to-end close checks. After: `artifacts/app-guard-blocked-notifications-after` passed, with contact in about 2.73 s (standard frame) and 0.97 s (nearby custom frame). The real Android Studio and Codex detection probes report `NativeHitTest`; those read-only results are not claims that their live windows were closed.

Current suite counts, installer hash and remaining desktop checks belong in build status and the handoff. Do not relabel controlled-window checks as a user-app live-close result.

## Microsoft references checked 13 September 2026

- [WM_NCHITTEST](https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest): `HTCLOSE` is the Close button; this message queries a position, it does not click it.
- [DWM window attributes](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute): caption-button bounds are a geometry hint, not a close command.
- [WM_SYSCOMMAND](https://learn.microsoft.com/en-us/windows/win32/menurc/wm-syscommand): `SC_CLOSE` requests a normal window close.
- [GetGUIThreadInfo](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getguithreadinfo): consulted while diagnosing foreground availability; no cached or guessed foreground fallback was introduced.
