# Product requirements

**1.0.0 publication direction:** the user requested all current changes on GitHub and a normal release numbered 1.0.0, plus production-release guidance. Preserve existing functionality and truthfully disclose development signing, UIAccess scope and uncompleted desktop checks. The stable updater excludes draft/pre-release entries. Browser integration remains deferred; the public-release guide defines the current companion-only production work.

**0.9.3 correction:** menu visibility is independent of foreground/capture success. Both cat and tray right-click paths must open a native visible menu, including when the main panel or cat is hidden and Windows refuses focus. Activation failures and opening transitions must not dismiss it. Preserve context-style submenus and existing outside-click/Escape/selection cleanup. The prior 0.9.2 failure guard violated this requirement.

**0.9.2 menu follow-up:** an explicitly opened pet/tray menu must dismiss on outside clicks, activation of another window, Escape or leaf selection, release its mouse capture, and resume companion autonomy. A first Escape may close the current submenu; clicks inside a submenu must work. Old menu events must not close a replacement menu. Ordinary petting/dragging remain non-activating. No global input hooks/polling. Evidence and native test limits: [menu dismissal](21-menu-dismissal.md).

**0.9.1 user update (13 September):** STYLE-02 requires real coat/custom RGB colours, separate hat/neckwear/collar slots with their own colours, a live tabbed wardrobe, pose-aligned accessories, and migration of old selections. UI-CHROME requires native caption colour matching the app or accent, with Windows default available. UI-PALETTE replaces green with charcoal/periwinkle and readable text in both themes. The final refinement removes the top-navigation click border and rounds text inputs. After local testing, the user explicitly approved GitHub source and installer publication. Acceptance and evidence: [appearance/theme](20-appearance-and-theme.md).

**0.8 user-authorized additions:** the current requirements below supplement the 0.7 companion milestone; browser work remains deferred.

| ID | Requirement | Acceptance |
| --- | --- | --- |
| PROF-01 | Work/Study/Break profiles | Independent app rules/activity; explicit manual override; weekly intervals/overnight/priority; schedule changes cancel pending paws |
| APP-03 | Exceptions, grace and allowances | Exact-path exceptions; 0–60 second grace; 0–720 minute daily allowance; local bounded aggregates; revalidate/cancel before normal close |
| DESK-01 | Desktop manners | Remember deliberate resting position per monitor; restore on selection; settle during input; optional transient focused-control avoidance |
| CAT-11 | Procedural personality | Ear twitch, notice before run, yawn, focus completion celebration; continuous transitions and reduced-motion support; accessories stay anchored |
| REL-02 | Signed preview/update/recovery | Reusable protected key; timestamped app/setup/uninstaller; same-publisher/hash verified downloads; user-initiated installation; retain verified installers for repair/rollback |
| QA-08 | Automated Windows coverage | Core and controlled native-policy tests on declared CI OS matrix; report live desktop checks separately |

See [profiles and reliability](19-profiles-and-reliability.md) for precedence, privacy, data and release limitations. Publicly verified signing remains outside the preview signing claim.

Status: proposed specification. These IDs define scope and acceptance. P0 is the first public release; an internal alpha can implement a narrower vertical slice. P1 items are explicitly deferred and must not appear as supported in release copy until verified.

**12 September 2026 user update:** the immediate deliverable is a free native companion with a simple original cat drawn and animated entirely in code: walking, running, meowing, playing, grooming, sleeping/waking, and a paw tap that dismisses supported Windows notification banners. The former sprite implementation was absent at task entry and is superseded by the new source. The browser extension remains future scope. Notification dismissal authorizes the close controls of supported banners, not arbitrary window or tab closing. See [the code-cat revision](14-code-cat.md).

| ID | Current milestone requirement | Acceptance |
| --- | --- | --- |
| CAT-05 | Walk/run in both directions | One symmetric original vector character; continuous rig, acceleration/braking, distance-linked stance and work-area bounds |
| CAT-06 | Play/groom/sleep/wake/meow | Distinct continuous poses, visible paw washing, same tail curling for sleep, calm scheduling, user interruption and reduced-motion alternative |
| NOT-01 | Notification paw dismissal | Explicit local opt-in; recognize shell toast close control, approach it, reach measured paw contact, revalidate runtime identity/geometry/age, invoke only that control |
| NOT-02 | Cancellation | Hide, drag, unavailable/moving/replaced banner, lock/suspend, disable, and expiry cancel pending attempts; no input injection or queued stale close |
| NOT-03 | Reviewable practice | Clearly labeled app-owned card uses the same motion/contact pipeline; must never be reported as proof of real Windows banner compatibility |
| NOT-04 | Run from the current position to the banner | No initial position reset; direct continuous run, smooth urgent turns and braking; on-screen arrival plus paw contact within 3.2 seconds in the far-corner native fixture |
| NOT-05 | Reach bottom-edge banners | Solve the body position and paw endpoint together at all cat sizes / tested DPI tiers; tip aligns within 1.5 physical pixels before invocation |
| NOT-06 | Real Windows test and honest outcome | Use WinRT app notifications under standard user privileges; report submission, actual target detection and confirmed dismissal separately; never treat a tray balloon or practice card as real-shell proof |
| APP-01 | Explicit app rules | Select a desktop executable; exact-path matching; per-rule reminder/close, enabled switch and always/focus-session scope; global guard and temporary pause; no default app list |
| APP-02 | Angry normal-close gesture | Real foreground window identity and caption close geometry, angry expression, continuous run and painted contact before normal close; save prompts/vetoes remain untouched; never force-kill |
| BRAIN-01 | Idle-aware behavior | Elapsed inactivity only; automatic nap threshold; breathing bubble and floating z symbols; wake stretch and varied activity; manual/quiet/reduced-motion priorities |
| STYLE-01 | Pet customization | One original cat with optional bandana, bow tie, bell collar or ear flower and five colours; neckwear anchored behind the chin; positions checked in both directions, front, groom and sleep |
| UI-NEW | Modern controls and menus | Themed animated switches, rounded scrollbars and one WPF menu shared by pet and tray icon; keyboard support and explicit guard controls |
| PKG-01 | Native Windows setup wizard | Welcome, access explanation, shortcut choice, confirmation, progress and completion; original cat artwork; inspect actual light/dark native pages |
| PKG-02 | Windows Installed apps integration | One stable app registration with name/version/icon/size and an executable uninstall command; Start menu shortcut; desktop shortcut choice |
| PKG-03 | Safe reinstall and removal | Reinstall preserves one entry and user state; uninstall closes only Cute Cat, removes owned program files/shortcuts/registration and empty program directories, preserves settings/history |
| PKG-04 | Explicit local-preview trust lifecycle | Reuse approved UIAccess access; new certificate trust needs an explicit acknowledgment; cancel/failure cannot leave new trust; uninstall removes only trust owned by Cute Cat; disclose preview expiry |
| FREE-01 | Completely free | No accounts, subscriptions, paid features, upgrades, or payment code |
| CAT-07 | Responsive touch and drag | A tap retains its greeting and resumes autonomous motion within two seconds; a drop settles and resumes within one second. At an edge this may start a planted 0.86-second turn before travel. Captured drag uses a friendly surprised pose; lost capture cannot strand the cat |
| CAT-08 | Smooth active playback | No sprite frames; directly paint code geometry to a reusable native buffer with a monotonic high-resolution clock; actual cadence mean ≤20 ms / p95 ≤25 ms; hidden rendering stops |
| CAT-09 | Code-only production cat | Original cubic paths, shared rig across native surface and UI; no image generation, optical-flow frames, runtime PNG decoding or frame cache; visual review at 96/128/160 sizes on both themes |
| CAT-10 | Animated direction changes | Reach the edge before reversing; planted paw shuffle with head lead and tail follow-through; continuous front/three-quarter views, no instantaneous mirror or zero-width squash. Interruption preserves the displayed orientation; notification contact waits for its final facing direction |

## Release scope

| ID | Priority | Requirement | Acceptance |
| --- | --- | --- | --- |
| CAT-01 | P0 | One original code-drawn desktop cat | Complete declared motion set, transparent edges, same geometry across desktop and UI |
| CAT-02 | P0 | Polite desktop behavior | Small nonactivating overlay; transparent padding passes clicks to other apps; no keyboard-focus theft |
| CAT-03 | P0 | Pet, move, park, hide | Mouse and accessible menu alternatives; position restored on a valid monitor; hide does not stop the session |
| CAT-04 | P0 | Quiet/normal and reduced-motion behavior | Quiet mode parks the cat; reduced motion removes roaming and flourish while preserving all actions |
| FOC-01 | P0 | Manual focus sessions | Presets 25/50 minutes, custom 5–180 minutes, optional task label; start/pause/resume/end all work |
| FOC-02 | P0 | Breaks | Suggested 5-minute break after a completed session; starts only after user action; protection inactive during break |
| FOC-03 | P0 | Protection lifecycle | Protection only runs in an active focus session; pause/end/lock/sleep/disconnect cancels pending actions |
| WEB-01 | P0 | Chrome and Edge stable | Separate installation/status per browser/profile; connection and supported-site permission verified |
| WEB-02 | P0 | YouTube Shorts rule | Protect verified `/shorts/…` routes on explicitly supported YouTube hosts; ordinary `/watch`, search, and account pages pass |
| WEB-03 | P0 | User-selected whole-site rules | Explicitly chosen hosts including optional Instagram/TikTok whole-site presets; accurate label that the entire selected site is covered |
| WEB-04 | P0 | Reversible feed shield | Selected active page pauses supported media and shows a dismissible focus screen; tab, form state, and history stay intact |
| WEB-05 | P0 | Allow/pause/correct | Allow this site for 5 minutes, pause protection, and edit/remove rule are available through visible controls |
| WEB-06 | P1 | Reels/TikTok feed-only rules | Implement only after logged-in/out, SPA, and regional route fixtures pass; do not relabel a whole-site rule as feed-only |
| APP-01 | P0 | Selected app nudges | User chooses installed/running app identity; foreground dwell produces a quiet nudge; no closing, blocking input, or process kill |
| SUM-01 | P0 | Small local summary | Today/week completed focus minutes, session count, and confirmed interventions; paused/break time excluded |
| SET-01 | P0 | Essential controls | Appearance, cat size, monitor/parking, reduced motion, sounds, startup, rules, browser status, local data deletion |
| SYS-01 | P0 | Windows integration | Supported Windows 11 x64 builds; mixed DPI, multiple monitors, taskbar work area, lock/sleep, Explorer restart |
| SYS-02 | P0 | Install/update/uninstall | Signed per-user release installer; no admin runtime; explicit startup choice; installed file/registration cleanup |
| A11Y-01 | P0 | Accessible control UI | Keyboard-only core workflows, Narrator names, visible focus, high contrast, scaling; cat art is not sole status indicator |
| PRIV-01 | P0 | Local and minimal | No screen/key capture, account, analytics upload, cloud model, or raw browsing log; selected-site permissions only |
| REL-01 | P0 | Recover safely | Crash/restart restores a paused session, never an unannounced active shield; idempotent completion and cleanup |
| COS-01 | P1 | More coats/cosmetics | Additional assets pass the same visual QA; accessories fit every approved animation |
| FOC-04 | P1 | Scheduled focus | Time zones, DST, missed schedules, pause semantics specified before implementation |
| SYS-03 | P1 | Windows ARM64 / Firefox | Separate builds and browser adapter QA; no support implied by framework portability |

## Defaults

- First launch shows the cat and a short welcome. No protection begins automatically.
- Default character size: 128 logical pixels tall; small 96, large 160. Aspect ratio is preserved.
- Cat parks near the bottom-right of the current work area with a margin, above the taskbar. A monitor can be chosen explicitly.
- Normal activity uses occasional short edge walks; quiet mode is suggested during focus. Reduced motion follows OS preference initially and can be changed.
- Sounds off. Startup off until chosen. Theme follows Windows; explicit light/dark options exist.
- Shorts protection is a suggested toggle, not an enabled hidden permission. Whole-site presets start off.
- Application rules are nudges only and start empty. Labels say “Nudge me in this app,” not “Block app.”
- Default focus preset 25 minutes; suggested break 5 minutes; no automatic repeat.
- Protection pause lasts until explicitly resumed or the session ends. It is separate from pausing the timer.
- Detailed event retention 30 days; daily aggregates 365 days. See data specification for exact fields.

## Protection status

| Visible state | Meaning | UI copy |
| --- | --- | --- |
| Companion only | No focus session or browser not configured | “Your cat is here. Start a session to protect selected sites.” |
| Protection active | Session running and at least one configured adapter is healthy | “Shorts protected in Edge · Chrome not connected” or equivalent scoped wording |
| Protection paused | User paused protection but timer may continue | “Protection paused” + Resume |
| Session paused | Timer and protection paused | “Session paused” + Resume session |
| Browser disconnected | Connection/permission missing for that profile | “Edge protection unavailable” + Repair connection |
| Break | Break timer running; no protection | “On a break” |

Never show a global green shield implying all browsers, incognito windows, apps, or sites are covered. App nudges and website shields have separate labels.

## Critical scenarios

### Start without an extension

The user can see the cat and run a timer. The interface explains that website protection needs the optional extension. It does not repeatedly reopen installation prompts or request permissions unrelated to the chosen rule.

### Shorts beside a useful video

In a supported connected browser, a Shorts page qualifies after the configured dwell. Another tab playing a normal tutorial does not. If the user changes tabs before the intervention commits, the old candidate is cancelled. No browser-wide keyboard shortcut is issued.

### Allow five minutes

The current shield disappears, supported media remains paused until the user plays it, and a visible timed exception is created for the selected rule/host scope. Switching between tabs does not reset its duration. Expiry during active browsing starts a fresh dwell; it does not instantly obscure content with a stale action.

### Work includes a selected site

The user can remove that rule or use the temporary exception. The app does not argue about their productivity. A whole-site rule explicitly includes all content on that selected host; the preview warns that login/compose/work views may also be covered, while still preserving state and offering an escape.

### Cat hidden, user presenting, full-screen app

Hide cat keeps the timer/protection state visible in the tray. Quiet screen/presentation mode is a separate explicit action that also suspends protection and nudges. Automatic full-screen suppression applies to the companion animation/nudges; browser protection remains governed by its selected rules unless Quiet screen is enabled. Thus full-screen Shorts do not silently bypass the rule.

### Lock, sleep, disconnect, or app crash

Pending commands expire, feed shields release through the adapter's cleanup/lease logic, and no stale action runs on resume. Lock/sleep pauses the session. Restart offers a paused recovery with the last saved elapsed time. The user must resume it.

## Non-goals for P0

No force-quit, automatic tab close, DNS/hosts modification, firewall/VPN, driver/service, anti-uninstall, global input hook, accessibility scraping of arbitrary window contents, screenshot/OCR classifier, LLM, cloud sync, account, social feed, pet hunger, paywall, or room simulation.

The app provides voluntary friction. A user can disable an extension, stop the app, use an unsupported browser, or remove a shield with developer tools. This limitation is part of the product definition, not a bug to “fix” by expanding permissions.
