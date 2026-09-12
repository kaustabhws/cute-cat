# Test and release plan

## Automated checks

Core tests must cover countdown/pause/resume/end, focus vs break accounting, gap/suspend behavior, crash recovery, duplicate completion, invalid durations, bounded persistence, and malformed state recovery. Extension tests cover exact-host/path matching, lookalike hosts, normal YouTube pages, exceptions, revision/session changes, and safe state defaults. Protocol tests reject invalid/oversized lengths and unknown requests.

Build both desktop and native-host release configurations. Validate JSON examples/manifests and relative documentation links. Secrets are never test fixtures. Tests use a disposable explicit data directory and synthetic pages; do not reset the user's real browser/profile data.

## Manual acceptance matrix

| Area | Cases |
| --- | --- |
| Companion | Show/hide, drag, pet, quiet/reduced motion, theme contrast, focus remains in underlying app |
| Transparent window | Click outside painted silhouette reaches another process; no invisible rectangle |
| Displays | 100/125/150/200/250% scaling; negative monitor origins; taskbar edges; unplug/replug; fullscreen |
| Sessions | Start, pause, resume, end early, completion, break, restart recovery, sleep and lock |
| Browser | Chrome and Edge stable; separate profiles/windows; selected host permissions; missing/disabled extension |
| Targeting | Shorts vs watch/search; SPA transition; rapid tab switch; stale reply; inactive window; incognito unsupported |
| Recovery | Quit desktop while shield visible; host/worker crash; permission revoke; reconnect; allowance expiry |
| UI | Small window, large text, long labels, keyboard-only, Narrator, high contrast, both themes |
| Packaging | Clean profile launch; native-host setup/removal; startup choice; update/uninstall; missing runtime |
| Performance | 5-minute idle and active capture; decoded sprite memory; hidden-window timer behavior |

For each run, record build hash, OS/build, architecture, browser version, display configuration, test data path, expected/actual results, and screenshots or reproduction steps where useful.

## Public-release checklist

- All P0 requirements implemented or explicitly respecified with user agreement.
- Zero known wrong-target destructive behavior; current scope contains no automatic destructive action.
- Shield escape and disconnect cleanup tested in real installed browser extension.
- Local privacy/network review complete; logs sanitized; history deletion verified.
- Code/asset/font licenses and generated-asset provenance recorded.
- Signed application/installer, checksum, version, release notes, extension store IDs, update strategy.
- Clean-install and uninstall tests; per-user host registry entries correct and cleaned.
- Support scope and limitations visible, including voluntary bypass and unsupported browsers.

Signing and store publishing need real credentials/ownership. Do all local build and QA work first; never imply that an unsigned local binary is signed, notarized, or publicly released.
