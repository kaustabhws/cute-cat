# Desktop menu dismissal — 0.9.3 local fix

**Publication update:** the user has now requested this correction in the 1.0.0 GitHub release. Earlier local-only statements below describe the development stage; they no longer block this authorized publication. Native visibility and simulated activation evidence remain distinct.

## 0.9.3 regression correction

The user confirmed that 0.9.2 prevented menus from appearing from both the cat and hidden tray. Its activation-failure guard was wrong: a refused foreground request closed the requested menu immediately. The old simulated test even asserted that incorrect behavior. Do not restore that guard.

The menu now lives in its own real, borderless, activatable WPF Window. `PetMenuItems` derives from MenuBase so entries retain context-menu item roles, submenu behavior and accessibility; a regular Menu would instead give them menu-bar roles. The same themed visuals are retained. Cat/tray callbacks defer opening until their mouse-up handler has returned. Activation and capture are best effort, and failure cannot hide the menu. The cat window is no longer activated as a temporary owner. Ordinary petting stays non-activating.

The menu window closes on deactivation after opening, outside-capture events, Escape, selection and cancellation. Initial activation/deactivation during opening is ignored; submenu windows are recognized as part of the same menu. Cleanup releases owned capture and resumes autonomy. A local window message hook handles cancellation only; there is no global input hook or polling.

Current validation: 10 opening regression checks passed in `artifacts/menu-v093-opening-final`, verifying real native window visibility through the actual cat right-click callback and tray callback, with main panel hidden, cat hidden, repeated opening and explicit activation refusal. These do not require simulated foreground success. The 19 lifecycle checks also passed in `artifacts/menu-v093-lifecycle-final`; those separately simulate activation transitions for dismissal/submenu/focus-return checks. A live user outside click still needs an active desktop. The earlier 0.9.2 evidence below is historical and does not prove the corrected opening behavior.

The same checks were repeated against the signed installed UIAccess build: ten opening checks passed in both normal and tray-only startup modes (including a main window never shown), and 19 lifecycle checks passed. Fourteen installation/signature/preservation checks passed. The local installed version is 0.9.3; publication has not been requested for this correction. User preferences and the existing signing identity were preserved.

## Historical 0.9.2 attempt

The user reported that the cat's right-click menu remained open when clicking elsewhere. The menu is shared by the pet and tray icon. This task fixes the local application; it does not publish a new release automatically.

## Cause and behavior

The cat intentionally uses `WS_EX_NOACTIVATE`, while a desktop popup needs foreground ownership for full mouse capture. Microsoft documents this exact notification-area menu failure and the foreground/capture requirement. Opening a WPF ContextMenu without an active owner can also leave its internal menu mode incomplete.

Only an explicit menu request temporarily activates the owner and then the popup. Regular petting/dragging keep the non-activating cat surface. `StaysOpen=false` remains explicit. The menu handles outside-capture events, native cancellation/deactivation, Escape and leaf selections directly, so dismissal also cleans up if WPF's internal menu mode is incomplete. Escape closes an open submenu first. Item actions still execute normally.

A hook exists only on the popup's own HWND while it is open. There are no global mouse/keyboard hooks, polling loops, injected system input, title reads or new monitoring permissions. Closing removes the hook, closes submenus, releases owned mouse capture and resumes the cat's autonomy. An old menu's deferred events cannot close a newer one. Prior focus is restored only if the menu/temporary owner still holds focus; an outside click that activates another window keeps that new focus.

References checked during this fix: [TrackPopupMenu remarks](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-trackpopupmenu), [SetCapture foreground requirement](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setcapture), and the WPF ContextMenu/MenuBase source. These describe Windows/WPF behavior, not additional product scope.

## Validation

Build succeeds without warnings. `--qa PATH --menu-checks --menu-events-only --app-fixture FIXTURE` passed 19 event/lifecycle checks in `artifacts/menu-v092-verified`. These use actual WPF menus, a test-owned second process, native cancellation/deactivation messages on the app's own HWND, WPF outside-capture/key events and accessibility invocation of menu items. Foreground activation is explicitly simulated; the report records `simulatedActivation: true`. It tests closing, cleanup, submenu behavior, repeated opening, stale events, focus-return decisions and the cat's non-activating mouse response.

The native mode omits `--menu-events-only` and requires an actual foreground fixture. It could not run here because Windows returned no foreground window in this RDP session, both with the usual launcher and the explicit diagnostic launch. That is a pending live desktop check, not a passed native outside-click test. No computer plugin or input injection was used.

The signed installed UIAccess build was checked separately: all 19 simulated menu checks passed, and the native mode still reported unavailable foreground activation. Installation/signature/preservation checks passed (14). Evidence is in `artifacts/menu-v092-installed-events`, `artifacts/menu-v092-installed-native`, and `artifacts/install-v092-local`. Existing settings/history and the signing identity were preserved. The fix is installed locally, not released to GitHub.

Changed files: `PetMenu.cs`, native activation/capture declarations, the diagnostic app entry point, `QualityChecks.Menus.cs`, and the test fixture's narrowly scoped foreground permission. State schema, cat artwork and notification/app-close targeting are unchanged. The next check is a real cat/tray right-click followed by clicking another app, the desktop, and a submenu on the user's active desktop.
