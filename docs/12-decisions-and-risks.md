# Decisions and risks

## D30 — requested 1.0.0 GitHub release

The user explicitly requested pushing all changes and publishing version 1.0.0, then explaining how to reach a production public release. Publish a normal GitHub release/Latest, with source and binary versions aligned; retain the current self-signed-development/UIAccess disclosures. This does not authorize inventing publisher credentials, silently importing new trust, choosing a source license or claiming assistive-technology eligibility. Stable update checks now skip draft/pre-release entries. Primary-source production guidance is in [22-public-release-guide.md](22-public-release-guide.md).

## D29 — desktop menu ownership and dismissal

**0.9.3 correction:** the user reported that 0.9.2 never showed the menu. Closing on refused activation was incorrect and is removed. Use an activatable WPF Window hosting MenuBase items, not activation of the non-activating cat or a focus-dependent popup. Opening/native visibility is tested with actual failures; foreground transitions remain separately simulated when RDP has no active desktop.

Use temporary foreground ownership only for an explicit cat/tray menu request, with cleanup tied to that popup's native messages and WPF events. Preserve `WS_EX_NOACTIVATE` for ordinary petting. Do not implement global click hooks or polling to work around incorrect menu ownership. Simulated activation tests and live native tests are separate; RDP returned no foreground desktop during this fix. See [menu contract](21-menu-dismissal.md).

## D28 — local wardrobe and theme revision (13 September 2026)

**Publication follow-up, 13 September:** after 0.9.1 was installed and checked locally, the user explicitly requested the source push and installer release. The hold below is now satisfied; publish the tested 0.9.1 preview to `kaustabhws/cute-cat`. This does not expand monitoring, signing trust or browser scope.

The user requested coat colour repair, independent hats/neckwear/collars with tabbed UI, theme-aware native title-bar colours, and a replacement for the green UI palette. Use opaque RGB coat/accessory colours, a cached procedural palette, native DWM caption styling, and charcoal/periwinkle controls. Schema 4 preserves schema 3 before migration. No new observation or permission scope. **No GitHub push or release until the user approves PC testing**, superseding D27's publication direction. Contract: [20-appearance-and-theme.md](20-appearance-and-theme.md).

## D27 — 0.8 profile and reliability scope (12 September 2026)

The user authorized profiles, per-app exceptions/allowances, desktop awareness, personality and release hardening, excluding browser integration. They explicitly permitted deferred live testing under RDP, requested GitHub source/release publication, and selected protected preview signing. Contract: [19-profiles-and-reliability.md](19-profiles-and-reliability.md).

Schema 3 uses bounded JSON aggregates, no browsing timeline. Manual profile selection pauses scheduling. Focused-control avoidance is optional and geometry-only. Signing uses a reusable nonexportable CNG key, not an exported PFX or publicly verified publisher. Updater accepts only same-publisher signed installers and offers installer-based repair/rollback. 0.8 establishes the recovery baseline; 0.7's unsigned installer is excluded. Future-version rollback, real editor/multi-monitor behavior and standard-user UIAccess remain separate desktop gates.

| ID | Decision | Status / reason |
| --- | --- | --- |
| D01 | Soft hand-painted 2D | Historical; superseded by explicit code-only direction D21 on 12 September |
| D02 | Native Windows C#/.NET 10, WPF + Win32 cat | Selected implementation direction; mature native UI and specialized small overlay |
| D03 | Reversible page shield | Default; preserves tabs and unsaved state; no automatic close/kill |
| D04 | Browser-authorized context | Selected; executable names cannot reliably identify Shorts |
| D05 | Local deterministic rules | Selected; no runtime AI/model download or screen capture |
| D06 | One cat before many cosmetics | Selected; animation cohesion is the quality priority |
| D07 | Windows 11 x64 first | Initial support target; ARM64/older Windows require separate validation |
| D08 | Atomic JSON for local alpha; SQLite deferred | Bounded data makes a small dependency-free start viable; migrate only when needed |
| D09 | User-provided Azure endpoint / gpt-image-2 | Authorized for build-time artwork; environment key only; see Azure setup |
| D10 | Local working alpha before public-release scope | User asked to build; signing/store distribution remain separate and must be reported honestly |
| D11 | Rebuild from documentation-only checkout | 11 September: no source, assets, scripts, tests or distribution existed despite the old status file; previous completion/test claims cannot be attributed to this checkout |
| D12 | Completely free, native companion appearance | Explicit user decision, 11 September; supersedes all paid-feature/cosmetic/subscription exploration. No SaaS dashboard |
| D13 | Narrow notification close authorization | User specifically requested Windows notification paw dismissal. Explicit app opt-in; scoped shell toast UIA controls only, identity/bounds rechecked; no broad window closing, message reading or input injection |
| D14 | Companion milestone precedes browser reconstruction | Current code focuses on requested motion/notifications plus a local focus timer. Existing browser design/protocol pages describe future work, not an implemented extension |
| D15 | New immutable original reference v002 | v001 files missing; generated original cream/taupe cat via authorized deployment. Actual patch is anatomical right. No borrowed art or mirrored asymmetric coats |
| D16 | Explicit cat motion controls | Full cat animation by default, with Reduce motion and optional Follow Windows animation preference. The automation session reports Windows animations disabled; explicit controls avoid silently disabling all requested cat actions |
| D17 | Asset manifest v2 | 384px canvas, timing/loop entry, ground pivot, measured paw anchor, SHA-256 files, bounded caches; generated keys plus reviewed offline in-betweens, not claimed as hand-authored animation |
| D18 | Short, explicit interaction recovery | 12 September user bug report: touches/drops must not enter parking or a long idle chain. Tap resumes in 1.1 s; drop in 0.7 s, unless quiet/reduced/focus mode intentionally parks the cat. Monitor/settings refresh must be idempotent |
| D19 | High-resolution active frame clock and predecoded sprites | DispatcherTimer still measured ~27 ms after image work was moved off-thread. Windows waitable-timer deadlines measured ~16.67 ms with a one-frame queue; retain slower idle cadence and asynchronous saves |
| D20 | Revised motion pack, same v002 character | Four new 12-key gait sheets and one friendly surprised drag pose generated through the existing Azure authorization. Local exports add registered in-betweens, remove flow islands, and preserve the earlier pack in `assets/cat` for comparison |
| D21 | Original vector cat and continuous procedural rig | Latest explicit user request: simple rounded/block-like cat, entirely in code, smooth walk/run/meow/groom/sleep. Supersedes D01/D15/D17/D20's raster character and frame cache. No competitor contour/source copied |
| D22 | Rebuild evidence from actual checkout | Only docs existed at entry; historical build/art/timing claims cannot establish the current implementation. New 0.4.0 source, checks and artifacts are authoritative |
| D23 | Reusable direct-draw surface | GDI+ cubic paths into a persistent premultiplied DIB, one native DC, waitable frame clock; shared painter for WPF. No frame decoding or runtime image service |
| D24 | Reduced motion rests paw helper | No automatic travel/dismissal in reduced mode; clearly labeled in UI. No silent broad-notification fallback |
| D25 | Preserve discovered legacy local data | Flat Version 1 migrates through a separate original backup; retain prior notification permission and restore sessions paused. Unknown/future schemas remain untouched |
| D26 | Shell test is an unresolved compatibility gate | Current real-banner test reported NoBanner and no invocation. Practice success is independent evidence; do not reuse the earlier user's report as proof for this build |
| D27 | Continuous projected turns | User rejected instant direction mirroring. 0.5.0 uses an 860 ms planted turn with head/body/tail timing, full-width front view and paw shuffle. Interrupted turns preserve displayed orientation and cancel future movement |
| D28 | Softer original character, revision 2 | User requested greater cuteness without realism. Retain cream/three-oat identity while shortening/plumping the body, enlarging round cheeks, lowering facial features and shortening the plush tail. All art remains code |
| D29 | Continuous urgent notification journey | Current position is the start; no parking/teleport. Cubic eased straight travel takes 0.35–1.55 s; urgent turns use the existing rig over 0.42 s. Gait remains continuous and capped at six cycles/s |
| D30 | Adaptive paw endpoint and entrance stabilization | Solve a valid body position plus an endpoint inside the vector canvas. Wait for 120 ms of stable close geometry before approaching. Preserve the original eight-second candidate lifetime |
| D31 | Narrow shell control discovery | Verified ShellExperienceHost / ShellHost image paths and process start time, known shell classes, flat UIA root fallback, exact runtime identities and corner geometry. Known close IDs first; a bounded English OS close-icon label fallback reads only that icon label, never notification message text. Exclude Notification Center ancestors |
| D32 | Real WinRT test notification | CommunityToolkit.WinUI.Notifications 7.1.2 (MIT), .NET 10 Windows SDK target, own per-user app notification identity. No notification-listener permission or access to other notification payloads |
| D33 | Standard-user runtime | Microsoft documents notifications as unsupported for elevated apps. Relaunch with a limited token when launched elevated, preserving only the current user's access to its own process objects. No account/UAC/OS security setting is changed; ordinary standard-user launches require no relaunch |
| D34 | Plugin-free verification and remaining shell gate | User requested no computer plugin. 0.6 native tests use compiled app/Windows APIs directly. The real test was accepted but no popup was exposed while Windows reported busy; keep this distinct from passing motion/UIA fixture tests |

## Risk register

- **D38 — Explicit selected-app closing:** the user authorized app-specific normal closing with an angry paw. Exact executable paths, foreground window identity, supported caption controls and painted contact are required. Scope can be always or focus-only; global and per-rule controls start safe. No process kill, save-prompt handling or URL/title classification.
- **D39 — Local companion brain and accessories:** elapsed idle time drives naps and waking; manual/quiet/reduced-motion priorities remain explicit. Continuous code adds a nose bubble/z symbols, wake stretch and mood. Bandana/collar/bow tie are anchored behind the chin, with a side shoulder fold; flower belongs to the head. No runtime images or cloud inference.
- **D40 — Source/release publication:** the user explicitly named `kaustabhws/cute-cat` and asked for source commits and the installer in Releases. Publish the functional preview with signing limitations disclosed. Exclude private config, user data, diagnostic captures, build caches and credentials. This authorization supersedes the earlier local-only delivery boundary for that repository.

- **D37 — Native installer:** user requested a wizard and Windows Installed apps/uninstall integration after accepting 0.6.1. Use Inno Setup 7.1.0 with a stable AppId, the unchanged signed app payload, generated code-cat wizard art, per-PC Program Files installation, explicit UIAccess trust acknowledgment, and Windows Restart Manager for the exact Cute Cat executable. Preserve user settings/history; remove only owned program artifacts and certificate trust. Same-version reinstall and legacy adoption are tested; future version/certificate upgrades need their own verification. See `17-windows-installer.md`.

12 September follow-up decisions:

- **D35 — Actual Windows 11 template:** direct FindWindowEx discovery; NormalToastView/FlexibleToastView support; known small right-aligned close controls below hero images; live scroll wrappers allowed, NotificationCenterGrid excluded. Production live UIA dismissal now confirmed, superseding D26/D34's earlier unconfirmed shell result.
- **D36 — Protected notification layer:** desktop TOPMOST cannot paint above shell notifications. The user explicitly approved the reviewed UIAccess test installation and its machine trust change. Its runtime UIAccess token and native window order above the toast are verified. Microsoft discourages this permission for ordinary non-accessibility apps; it remains an expiring local experiment, not a public-release recommendation. Do not claim foreground paw overlap from off-screen buffer contact alone. See `16-uiaccess-review.md`.

| Risk | Mitigation / evidence needed |
| --- | --- |
| Cat becomes a distraction | Quiet default during focus; long held poses; one-week user feedback |
| Inconsistent generated anatomy | Reference-conditioned edits, registered poses, native playback review |
| Chroma-key halos | Inspect alpha on multiple backgrounds; despill and edge cleanup; no model switch without authorization |
| Wrong page intervention | Document-local matching, continuous dwell, revision/session check, reversible action |
| Browser changes break adapters | Narrow supported routes, fixtures, fail-open behavior, honest coverage |
| Extension setup friction | Explicit connection screen and test; store distribution before public release |
| Sprite memory/CPU | Runtime-size exports, bounded cache, suspend idle/hidden animation |
| Monitor/DPI issues | Signed coordinates, work-area clamping, actual multi-monitor QA |
| Native bridge misuse | Current-user pipe, exact extension origins, bounded typed messages, no command execution |
| Underestimated release work | Distinguish runnable alpha from signed installer/store release; record unchecked cases |
| Branding/IP overlap | Original character and name check before publishing; track every asset's provenance |

## Open decisions

Public brand/name, store distribution ownership, signing provider, updater, final release support matrix, additional free art variants, scheduling, Firefox, ARM64, and route-specific Reels/TikTok expansion. The app's cost is settled: completely free. These remaining decisions do not block the local companion milestone.

## 13 September — companion extras

- **D48 — Local 1.1.0 milestone:** all six authorized suggestions plus existing-instance shortcut activation. Preserve browser deferral and current normal-close, notification and menu semantics.
- **D49 — Presentation follows policy:** the grace bubble observes GuardGate and binds an exception button to exact identity/path/epoch. It never drives a close timer. Nonactivating WPF UI, no global hook.
- **D50 — Portable settings with review:** a bounded versioned backup contains configuration; imported rules start unchecked and global monitoring stays off. Keep startup, placement and earned session/history on this PC. Support reports use a separate fixed allowlist.
- **D51 — Saved looks and breaks:** named outfits are immutable until explicitly replaced; edits detach profile links. Optional attended-focus cues preserve a paused focus snapshot across a five-minute break and restart.
- **D52 — Shortcut signal:** use a payload-free registered message addressed to a per-instance message-only HWND; explicitly allow only that message across UIAccess integrity levels. Restore the existing panel instead of reporting a duplicate instance. No general IPC commands or new monitoring.

## 1.1.1 UI responsiveness

- **D53 — Input before background work:** keep cat animation rates/geometry; yield rendering callbacks to input, coalesce UI refresh and atomic background writes, cache bounded UI thumbnails and render previews at display resolution. Runtime policy updates remain synchronous.
- **D54 — Brief elapsed-time UI transitions:** animate toggles and content reflow, including maximize/restore, with one temporary dispatcher timer. Use current displayed values for reversal; respect reduced motion and stop after completion.
- **D55 — Startup default authorized:** the user explicitly requested startup on. Apply once at first normal 1.1.1 launch; record an additive StartupInitialized marker so later explicit opt-outs persist. Register only the app-owned current-user Run value. QA/custom data and read-only states are excluded. Keep the installer local until publication is approved.

- **D56 — Publish the accepted 1.1.1 build:** the user explicitly authorized pushing the complete 1.1.0/1.1.1 work and a new GitHub release after local testing. Keep the installed installer unchanged; publish its matching SHA-256 and update metadata. Preserve the development-signing/UIAccess disclosure and existing regular-release updater channel.

- **D57 — Verified native caption fallback:** use bounded native HTCLOSE queries when legacy rectangles are empty. The system close command must be available, the measured region must pass fresh checks, and the final close remains a normal request after paw contact. No executable-name allowlist or coordinate-only assumption.
- **D58 — Independent shell scan slot:** notification discovery cannot hold the app-guard loop. Keep one pending shell task and discard stale-epoch observations. Regression coverage includes both helpers together, the actual Add app UI and real foreground state.
- **D59 — Dialog-based application windows:** replace the blanket dialog-class exclusion with structural main-window checks: unowned, minimizable and no competing visible main window in the same process. Continue rejecting owned and message/save-style dialogs; verify normal-close refusal separately.

- **D60 — Publish 1.1.2:** the user authorized publishing the App guard source and tested installer on 14 September 2026. Use the unchanged installed artifact and matching checksum/update metadata; retain development-signing and desktop-validation disclosures.
