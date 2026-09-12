# Focus companion and customization — 0.7.0

The user explicitly requested app-specific distraction behavior, normal window closing with an angry paw gesture, idle sleep and wake, better sleep artwork, accessories, modern switches/menus, and an installer update. This supersedes the earlier nudge-only app policy for apps the user selects. Browser URLs and tab-specific detection remain future work.

## User controls

- **App guard:** add an open desktop app or browse to its executable. Choose a normal window close or a reminder, always while the guard is enabled or only during a running focus session. Each rule has an enabled switch. The global guard starts off, with no apps selected. Pause/resume is available in the page and the pet/tray menu.
- **Your cat:** choose calm, curious/balanced or playful activity; enable idle naps and choose a delay. Returning activity wakes an automatic nap with a stretch. Manual Sleep remains until Wake or an explicit interaction. Quiet/focus settings still constrain wandering.
- **Accessories:** none, side-fold bandana, bow tie, bell collar or ear flower; sage, rose, blue, lavender or honey colours. Neckwear is drawn between body and head so the chin occludes it. The bandana's small fold rests on the shoulder; it is not drawn on the face. The flower uses a head anchor.
- **Modern UI:** themed animated switches, scrollbars and a shared WPF context menu for the desktop pet and notification-area icon. No WinForms ContextMenuStrip is used for the pet menu.

## Detection and closing contract

`AppRulePolicy` receives rules, an executable path, focus state, monotonic time and a temporary pause deadline. It performs exact path matching with ordinal case-insensitivity. It does not inspect titles, URLs, text, pixels or typing.

`DesktopApps` queries the foreground window's PID, executable path and process start time. It excludes the current process, system executables under Windows, owned/dialog windows and ineligible windows. The app picker reads executable metadata, not document/window titles. Rules are local user-selected configuration.

Close geometry comes from `WM_GETTITLEBARINFOEX`. A fallback considers only a small enabled close control under an accessible title-bar container. Unknown/custom layouts are not guessed. The button must be reachable on the cat's selected monitor. Top-edge caption buttons are supported by allowing the transparent top of the cat canvas above the work area while keeping the character visible.

The app uses the existing current-position approach, continuous turns and painted paw-contact gate. App-window and notification targets have distinct purpose flags. Immediately before requesting a close, it revalidates the foreground window, process/start identity, selected rule, geometry and cancellation epoch. A standard caption close posts `SC_CLOSE`; an accessible caption button uses InvokePattern. There is no process kill, Alt-F4 injection or automatic answer to a save prompt.

A window that refuses closing is remembered, so the helper does not repeatedly press its close button or its save dialog. Hidden/destroyed/minimized windows are pruned so reopening an app can be handled again. User cancellation, guard/rule changes, focus-scope changes, pause, hide and drag invalidate pending actions. Animation is not the authorization source.

## Idle and artwork contract

Windows `GetLastInputInfo` contributes elapsed inactivity only, using wrap-safe tick subtraction. No key/mouse event contents are collected. `CompanionBrain` decides automatic sleep/wake independently of rendering and Windows APIs. Explicit interaction and active paw work take precedence. Sleep is held, not automatically ended after an arbitrary 45 seconds.

The original procedural rig now includes wake stretching, anger, a breathing nose bubble, and staggered floating z symbols. Accessories and expression parameters are part of the displayed pose. All runtime animation remains code-driven; exported images are diagnostics only. Active drawing remains 60 Hz, with 15 Hz idle/sleep cadence and stopped hidden painting.

## Persistence and upgrade

State schema 2 adds app rules, accessory/colour, activity level, idle settings and app-guard enablement. Schema 1 upgrades with an original `.schema1.json` backup. Nickname, notification permission, focus progress/history and other preferences are preserved. App closing is not enabled by migration. Invalid rules are dropped individually; future schemas remain read-only.

The signed 0.7.0 app uses the previously authorized UIAccess capability. The installer retains the stable AppId and current install directory. Certificate rotation can remove an older owned preview certificate only when its identity is explicitly listed in `installer/CertificateHistory.json`; arbitrary trust entries are not removed. No signing private key is retained.

## Evidence and limits

Core checks cover path matching, scope, pause/disable, idle/manual sleep, wake, pose propagation, top-edge reach and schema migration. Native checks use separate test-owned windows to verify caption contact, close requests, angry expression and refusal/save-prompt behavior. The live foreground test recorded a real foreground window (no injected foreground identity), about 2.0 seconds to contact and 0.405 px contact error; the refusal fixture stayed open with one close request.

When Windows had no foreground window, a separate controlled fixture run injected only the foreground-window identity into the adapter. That run is distinct from the subsequent live foreground test. No user applications were selected or closed for QA.

Artwork placement was reviewed in both directions, front view, grooming and sleep on light/dark backgrounds. Native control XPS exports were rendered for visual inspection; no desktop screenshots or computer plugin were used.

Known scope: supported desktop windows on the selected monitor; normal close can be vetoed or show a save prompt; system/unknown/custom windows are left alone; no URL detection or unbreakable blocking. UIAccess public-release eligibility, durable code signing and broader account/DPI/platform coverage remain separate release work. See build status for exact executed results and the current installer artifact.
