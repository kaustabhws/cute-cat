# Data, privacy, and trust boundaries

**1.1.1 update:** the user requested Windows startup enabled by default. The first normal launch records this choice; a subsequent explicit opt-out persists. Writes are coalesced on one background worker and flushed on exit. Policy cancellation is immediate and monitoring scope is unchanged. See [contract](24-ui-responsiveness.md).

**1.1.0 current inventory:** state schema 5 includes bounded saved outfits, profile links, optional reminder/purr preferences and a paused focus return snapshot. User-exported configuration includes executable paths, but excludes session/history/usage/exceptions/placement. A separate support-report allowlist omits personal names and paths. Import leaves monitoring off and preserves Windows startup. See [the contract](23-companion-extras.md).

**0.8 current inventory:** profile rules/schedules and normalized monitor spots are local configuration. Per-app exceptions store chosen executable plus UTC expiry. Daily allowance aggregates store only chosen executable, local day and seconds, retained for 14 days. Optional focused-control avoidance observes transient geometry only and starts off. Optional GitHub update checks/downloads send ordinary HTTP metadata to GitHub; no settings, usage or browsing data is uploaded. Startup checking starts off. See [the current contract](19-profiles-and-reliability.md). Browser data sections below remain future specifications.

Local-first is a product behavior, not a marketing substitute for data accounting. The desktop app has no need for a login, image-generation key, screen capture, page-content upload, or typing history.

## Data inventory

| Data | Purpose | Storage / retention |
| --- | --- | --- |
| Settings, selected rules, cat size/position | Restore explicit preferences | Local user profile until reset/uninstall choice |
| Current session ID, duration, elapsed time, optional task label | Session/recovery | Local; checkpointed; label can be omitted by user |
| Session summary | Today/week display | Bounded local records; planned 30-day detail and 365-day daily totals |
| Rule ID and confirmed intervention count | Feedback and summary | Aggregate only; no raw browsing history |
| Active route/tab/document identity | Exact browser-local targeting | Ephemeral in extension; not sent to the desktop app |
| Temporary allowances | Avoid unwanted repeat interruption | Session-scoped extension storage, expired/cleared automatically |
| Browser connection status | Accurate coverage UI | Ephemeral; no persistent profile inventory |
| Crash/error code | Troubleshooting | Local bounded sanitized records, no automatic upload |
| Azure image key | Build-time artwork generation | Environment only; never bundled or persisted by the app |

Initial atomic JSON storage is acceptable for the first local build; use write-to-temp then replace and keep a recoverable backup. Cap record count and text lengths. Malformed data loads safe defaults with a visible notice, not an unhandled exception or immediate deletion of the original. The proposed SQLite migration uses explicit schema versions and transactional updates.

Current 0.4.0 state uses explicit `Schema: 1`, preferences, a session snapshot and at most 500 completed-session records within 30 days. It preserves an atomic `.bak`. The prior flat `Version: 1` format migrates nickname, size tier, monitor/position, visibility, quiet/motion preferences, prior notification permission and paused session progress, with a separate `.legacy-v1.json` backup. Unknown/future shapes are read-only. The broader 365-day aggregate store above is future scope. Diagnostic reports contain scenario names, timing/resource metrics and capability codes; no notification content or user nickname/task text is emitted.

Store under `%LOCALAPPDATA%\CuteCat`. A `--data-dir` override is allowed for isolated development/testing. Never use a shared system directory for per-user behavioral data. Ordinary user ACLs do not protect against malware already running as that user; no such guarantee is made.

0.6.0 uses Windows UI Automation only in verified shell toast windows. It reads structural IDs, bounds, state and the Windows close icon's accessibility label when a known ID is unavailable. It does not read toast message text or log icon labels/window titles. Real test notifications register only Cute Cat's own app identity/activation in the current user's registry through the Windows Community Toolkit helper. There is no notification-listener capability or history access to other apps. Explicit diagnostic runs can export timing/geometry and known status codes; normal operation keeps these ephemeral.

## Permissions

The extension needs native messaging, storage, scripting, and optional host access for chosen websites. Avoid broad `tabs` permission if selected-host access is enough. `activeTab` cannot silently monitor future visits; do not advertise it as persistent protection. Use navigation events only if needed and explain the added permission.

Request optional host access during explicit rule setup. The browser owns its permission prompt. No access to incognito, file URLs, every site, downloads, history, microphone, clipboard, camera, or cookies by default. The content script must never read form values, private messages, account identity, or page text to classify content.

Register the native host with exact extension IDs for each browser catalog/development build. A wildcard allowed origin is forbidden. Use a current-user-only pipe and bounded data messages. The bridge has no general file, network, command-execution, or process-control capability.

## Threat and failure model

- Malicious/changed websites may remove their own page shield. This app is voluntary friction, not a security boundary.
- A compromised same-user process is out of scope for strong isolation, but must not receive an intentionally exposed command-execution API.
- Browser/app crashes must release protection rather than strand the user.
- Do not rely on an extension-supplied URL to open anything through the OS. Route matching remains local; only known actions cross the bridge.
- Extension scripts run in an isolated world; sanitize visible labels with textContent and never inject remote HTML/code.
- Avoid raw exception dumps containing user paths, page URLs, or task labels in shared diagnostics.
- An API key supplied for asset generation does not authorize a runtime cloud monitoring feature.

## User controls

Pause protection, remove a rule, disconnect a browser, disable startup, hide the cat, quit, and delete local history must be available. Data deletion requires an explicit user action in the product with an understandable confirmation. Uninstall must explain whether local preferences/history will be retained or removed.

Before public release, verify on a clean profile that no application network calls occur during normal companion/timer use. Browser-extension activity is limited to the local native bridge and the sites the user already visits. A future updater or optional cloud feature needs its own visible disclosure and decision record.
