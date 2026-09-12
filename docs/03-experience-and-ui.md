# Experience and UI specification

Status: mixed specification and current implementation, 12 September 2026. The 0.4.0 native UI has Focus, Your cat, Quiet desktop and Settings. Its cat and preview use the same code-drawn geometry. The browser surfaces below remain future design. Current control renders and measured checks are linked from [build status](build-status.md); earlier 0.2/0.3 render paths are historical.

11 September update: implemented surfaces are Today, Your cat, Quiet desktop, and Settings. Use warm paper/forest tones and restrained serif headings with native Segoe controls. The user explicitly rejected a SaaS appearance and requested a completely free app. Today has one focus control and a quiet line of recorded time; it has no metric-card dashboard. Reference renders come from actual WPF controls under `art-previews/app` and are distinct from live screenshots.

## Visual direction

Quiet studio desk: warm paper, deep botanical green, restrained clay accents, a simple rounded code-drawn character, clear typography. Use generous spacing and calm hierarchy. Avoid a dashboard full of charts, constant gradients, rainbow controls, or art behind reading text.

Use Segoe UI Variable where available, with Segoe UI fallback. Body 14–16 DIP, labels 12–13, page title 26–30, timer 48–56 with tabular numerals. Reserve any future display font for short brand text and bundle it only after licensing review. Icons should be one consistent vector set or original paths, not emoji.

Light theme uses paper backgrounds and near-black green text. Dark theme uses deep green-gray surfaces and warm off-white text. The cat must remain readable in both through its own contour and shading. Borders communicate structure; shadows are subtle and never the only boundary.

## Surfaces and layout

| Surface | Initial size / role | Required content |
| --- | --- | --- |
| Desktop cat | 128 DIP tall by proportional width | Art only; no perpetual speech bubble or timer obscuring work |
| Cat quick panel | About 360 × 400 DIP, clamped to work area | Session state, primary action, protection status, Hide cat, Open app |
| Main app | Start at 860 × 640 DIP, resizable; min 720 × 540 before large-text adaptation | Four destinations: Today, Your cat, Distractions, Settings |
| First-run flow | Content within main app, 520–640 DIP comfortable reading width | Meet cat, choose boundaries, browser connection, practice |
| Browser focus screen | Shield fills webpage viewport; centered card max 440 CSS px | Reason, Return to work, Allow 5 minutes, Pause protection, rule/settings link |
| Session complete panel | Small native panel or main-app state | Actual focus duration, brief acknowledgement, Start break / Done |

The main app has an approximately 168 DIP navigation rail and a flexible content column. At narrow widths or enlarged text, collapse navigation to a labeled menu and allow vertical scrolling. Do not make critical actions inaccessible behind fixed-height art.

Preserve ordinary Windows window affordances, minimize/close behavior, keyboard focus, and resizing. Closing the main window leaves the chosen background companion running, with a one-time explanation. Quit is available in the tray and app menu. Do not hide the only quit action behind the cat.

## First run

1. **Meet your cat:** show the approved idle cat and “A little company for your focus time.” Let the user choose a local nickname, optional. Continue and “Just show the cat” are visible.
2. **Choose boundaries:** explain sessions; suggest Shorts, with whole-site presets separate. No blanket list pre-enabled. Show the difference between website shields and app nudges.
3. **Connect browser:** detect supported installed browsers as a convenience. Offer the relevant official extension page; never claim the connection until the native handshake and permission check succeed. Browser prompts remain browser-owned.
4. **Practice safely:** use a local extension demo fixture to show the shield and escape actions. A button can open a real Shorts page only through an explicit user action. Do not automatically browse social media.
5. **Start:** optional task label, duration preset, and one Start session action. Cat settles into quiet focus behavior.

A user who skips browser access can complete onboarding. The screen says “Cat and timer ready. Website protection needs a browser connection.” If permission is declined, retain the user's rules as inactive drafts with a clear repair action.

## Today screen

Before a session: heading “Make room for one thing.”; optional task field; duration chips 25 / 50 / Custom; one primary Start session button; selected boundaries summary; small character vignette with ample whitespace.

During a session: time remaining is the strongest element. Show the task, pause/end actions, and an independently labeled protection control. Keep settings available without changing focus state accidentally. Do not add a live productivity score.

After a session: “You spent 25 minutes with your task.” Show actual counted focus time. The cat performs one short acknowledgement. Offer Start a 5-minute break and Done. No automatic next session, guilt copy, forced rating request, or confetti storm.

Below the main card, keep a compact Today summary. A simple weekly bar chart can compare session minutes, with accessible text values; this is recorded session time, not an estimate of work quality. No empty chart should dominate the first run.

## Your cat screen

Use a large but bounded preview, nickname, small/medium/large size, normal/quiet activity, monitor/parking choice, and Show/Hide. Future coats use named swatches and selected-state outlines. For P0, do not show a store or locked silhouettes for unavailable art.

The preview has a replay button for a few approved interactions. Replaying preview art must not trigger focus actions. Moving the cat across monitors has an accessible alternative: choose a monitor and corner from menus.

## Distractions screen

Separate “Websites” and “App nudges.” Each row shows rule name, exact scope, enabled state, and coverage status. Examples:

- “YouTube Shorts — Shorts pages only.”
- “TikTok website — all pages on the selected host.”
- “Discord — gentle desktop nudge.”

Adding a website parses and previews the normalized host, whether subdomains are included, and the selected action. Start with exact-host scope; offer subdomain coverage as an explicit option. Host permissions are requested from a user gesture. Do not expose arbitrary regular expressions in P0.

Each browser/profile has its own connection row. Provide a Test connection button, last successful handshake indicator, and useful repair steps. Do not display an unrelated browser as covered because it shares Chromium internals.

## Intervention experience

After a qualifying dwell, the cat looks toward the page and makes a small paw gesture near its current edge position. It does not travel across the whole working screen or imitate a click on browser chrome. The browser shield appears in the targeted page, with supported video paused. A short “paper settling” animation connects the gesture and the shield.

Suggested copy:

> A small pause for your focus.
>
> You chose to pause YouTube Shorts during this session.

Actions: **Return to work**, **Allow 5 minutes**, **Pause protection**. Include “Change this rule.” Return to work is user-initiated and can offer the previously chosen work app; if Windows cannot activate it, present an ordinary task-switch cue rather than forcing focus.

The shield uses a mostly opaque calm surface so the feed is not visually tempting. Keep all actions keyboard reachable, announce the reason once, and focus the card only when the verified visible page is being shielded. On removal, restore prior in-page focus if it still exists. Do not automatically resume media.

Escape releases the current shield and creates the same temporary allowance as Allow 5 minutes, avoiding an immediate re-block loop. The card explains the shortcut. App-level Pause protection is also available through a configurable global hotkey; register it gracefully and show if another program owns it.

## Cat interaction rules

- Single click: brief pet response; do not take keyboard focus.
- Right-click: accessible quick panel/menu opens intentionally.
- Drag past a small movement threshold: move cat, cancel pending flourish, preserve work-area boundaries on drop.
- Hover: optional cursor hint; no endless pet animation loop.
- Hide cat: removes artwork only, without misrepresenting protection state.
- Quiet screen: explicit combined suspension of companion/nudges/protection; timer remains as the user chose.
- Alt+Tab: ordinary app appears when its window is open; decorative cat does not.

All essential cat operations also exist in the app/tray so the nonactivating art is not an accessibility barrier.

## Motion and sound

Controls use restrained visual state changes. The cat's continuous rig and 320 ms pose transitions are independent of UI motion. Future browser interventions have bounded flourishes and must not depend on animation completion.

Reduced motion replaces roaming with still poses and removes large transforms. No flashing or more than three flashes per second. Sounds default off; later licensed purr/tap sounds must be quiet, rate-limited, and separately mutable. No lofi streaming service in P0.

## Accessibility and visual acceptance

- Standard text contrast at least 4.5:1; large text and essential nontext controls at least 3:1. Verify actual rendered state, not just token pairs.
- Visible focus outline in both themes; error/connection state communicated by text and icon, not color alone.
- Narrator supports onboarding, sessions, rule changes, pause, quit, and data deletion. Decorative animation is not repeatedly announced.
- Cat size and UI text scaling are independent. Test Windows scaling 100/125/150/200/250% and enlarged text.
- High-contrast mode uses system colors and removes decorative effects from controls.
- Keyboard routes are documented and tested; drag is never required for a core operation.
- Empty, loading, disconnected, no-permission, error, long-label, and recovered-session states are designed before feature completion.

Visual QA artifacts should show light/dark Today states, onboarding, rule editing, disconnected browser, focus screen, and the cat at real desktop scale. A marketing-sized portrait does not prove desktop legibility.
