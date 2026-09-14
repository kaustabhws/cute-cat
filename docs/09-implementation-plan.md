# Implementation plan

**Current task — publish 1.1.2:** the user approved pushing the source and tested installer on 14 September 2026. Publish the unchanged installer with matching checksum/update metadata, verify Windows CI and downloaded release assets. The earlier local-only directions below describe prior milestones.

**Current task — 1.1.2 App guard repair:** reproduce the reported failure through the actual app-rule UI and real foreground windows; recognize supported custom and dialog-based main windows; stop notification discovery from blocking app rules; preserve save prompts and install the repair locally. See [contract](25-app-guard-repair.md).

**Current task — publish 1.1.1:** the user approved pushing all completed changes and the new installer release. Preserve the tested installer, update release documentation, verify the source/CI and uploaded assets. The earlier publication holds below are historical.

**Current task — 1.1.1:** optimize the app UI and toggles, animate maximize/restore reflow, preserve desktop-cat animation/protection, enable Windows startup by default, and install locally. Do not push or release until the user approves. See [contract](24-ui-responsiveness.md).

**Current local task — 1.1.0:** implement EXTRA-01–07 from [the extras contract](23-companion-extras.md), preserve the working notification/menu/app-guard flows, verify local regression/artwork/installation, and keep the candidate local. Browser URLs remain deferred. Earlier task entries below are historical.

**Current task — 1.0.0:** publish all current code and the installer under version 1.0.0 as requested, verify CI/artifacts, and explain the remaining production-release steps. Keep development-trust/UIAccess limitations visible. Do not treat the version number or GitHub's normal-release flag as public-CA signing or completed desktop certification. The former local-only restriction on the menu corrections is lifted by this request. See [release guide](22-public-release-guide.md).

**Current 0.9.3 task:** repair the menu-not-opening regression from 0.9.2 for both cat and hidden-tray entry points. A denied focus request must never suppress the menu. Use a real activatable menu window, verify native visibility without faking activation success, and keep dismissal/submenu checks separate. Install locally for user testing; 0.9.1 remains published.

**Current 0.9.2 task:** fix the desktop cat menu remaining open after outside clicks. Use normal foreground/menu capture semantics and scoped cleanup; preserve non-activation for normal petting. Verify event paths locally, install the fix, and record any live-desktop verification blocked by RDP. See [menu contract](21-menu-dismissal.md). This request does not add a new release publication step.

**Current 0.9.1 task:** publish the tested appearance/control changes and installer to the authorized GitHub repository. The user explicitly approved this on 13 September after the local title-bar, coat/wardrobe, palette, navigation and input refinements. That approval supersedes the earlier local-only hold. Preserve the tested installer and its checksum, exclude private artifacts/keys, and verify CI and release assets. See [appearance contract](20-appearance-and-theme.md).

**Current 0.8 priority:** profiles/schedules, app exceptions and allowances, monitor-aware resting, personality, durable preview signing, verified updates/recovery and automated Windows checks. Browser integration is explicitly excluded by the user. Complete packaging and publish source/release to the authorized GitHub repository. Desktop checks unavailable under RDP may be documented for later; do not block delivery on them. See [contract](19-profiles-and-reliability.md).

**Current user priority — focus companion 0.7:** implement explicit desktop-app rules with normal close/reminder behavior, angry paw contact, idle sleep/wake, accessories and modern controls/menus; then update the installer and publish source plus the installer release to the user-named GitHub repository. App closing is authorized only for user-selected apps; no process kill or save-prompt automation. Browser URL detection is deferred. See [focus companion contract](18-focus-companion.md).

**0.6.1 notification follow-up:** actual Windows 11 discovery/template/geometry fixes and a live dismissal are verified. The user approved the signed UIAccess experiment and accepted the result; its runtime window is above the Windows notification band. The standard portable build retains ordinary permissions/layering.

**0.5.0 follow-up:** CAT-10 takes precedence over browser expansion: replace immediate mirroring at both boundaries with an interruptible, planted turn, and refine the original cat toward a plumper, cheekier toy-like silhouette. Prove intermediate native poses/cadence, both boundaries, cancellation, unchanged contact targeting and desktop-size artwork before packaging. No monitoring or permission expansion.

**Current override, 12 September 2026:** [the code-cat plan](14-code-cat.md) replaces image production M03 with original cubic paths and continuous rig animation. This checkout again contained only documentation at task entry; 0.4.0 is the new source-backed implementation. Finish and verify the code-cat/notification milestone, preserve the discovered legacy user state, then proceed to M05 browser protection. Current evidence belongs in build status, not historical 0.2/0.3 handoffs.

The user authorized actual implementation and Azure image generation on 10 September 2026, after research. Build a working local alpha first; the complete P0 requirements are the subsequent public-release standard. Track actual completion in [build status](build-status.md), not by rewriting aspirations as accomplished facts.

On 11 September the checkout contained documentation only. The active dependency-ready work therefore rebuilt M01–M04 and the companion parts of M06–M09. The user additionally requested running, playing, grooming, sleeping/waking, and notification paw dismissal, then explicitly asked for repeated build/test/refinement. Finish this companion milestone before reconstructing browser protection. The app must be completely free and must look like a native companion, not a SaaS product.

Current proof sequence: deterministic motion/session/identity/recovery checks → alpha/registration review → real native layered-window and WPF event checks → paw-contact geometry and cancellation → actual interactive Windows banner smoke → packaging. A passing practice-card test does not complete the real-banner gate. See [companion and notification contract](13-companion-and-notifications.md).

12 September follow-up: CAT-07/08 repairs touch/drop recovery and measured animation smoothness. The native motion smoke must exercise the production pointer handlers, displayed drag/landing/greeting frames, resumed walking and actual frame cadence. A timer configured to 60 FPS is not evidence that it delivers 60 FPS; reject recurring frame cache misses or mean/p95 intervals above the local gate.

| Task | Depends on | Deliverable and proof |
| --- | --- | --- |
| M01 Native skeleton | None | .NET solution, WPF window, deterministic session core, isolated state directory, build instructions |
| M02 Cat surface | M01 | Native transparent window, alpha art, no-focus click/drag, parking, show/hide, taskbar/tray behavior |
| M03 Character production | Latest user medium | Original code contours, continuous rig, vector icon, visible groom/sleep/paw behavior, real-size native preview and cadence/resource evidence |
| M04 Focus session | M01 | Presets/custom duration, pause/resume/end, break, recovery, local summary; deterministic core tests |
| M05 Browser vertical slice | M01, M04 | MV3 extension, native messaging host, per-user pipe, Shorts matching, reversible shield, timeout cleanup |
| M06 UI refinement | M02–M05 | Four coherent native screens, honest connection state, no clipping, keyboard controls, art integration |
| M07 Rule management | M05 | Explicit whole-host rules, permissions, allow five minutes, protection pause, rule revision tests |
| M08 Desktop hardening | M02, M04 | Monitor/DPI/lock/sleep behavior, tray recovery, startup opt-in, reduced motion, performance evidence |
| M09 Local alpha delivery | M03–M08 | Runnable executable/distribution, tests, setup scripts, documented remaining gaps, actual UI inspection |
| M10 Public release | Alpha feedback | Complete P0 QA, full animation set, app nudges, signed installer, extension store distribution, accessibility/performance audit |
| M11 Expanded coverage | M10 | Verified Reels/TikTok feed routes, Firefox/ARM64 if chosen, additional art |

## Proof gates

1. Cat is visible and draggable without a rectangular background or stealing typing focus. If the windowing prototype fails, fix it before adding features.
2. Rule tests distinguish Shorts from ordinary video and lookalike domains. The page shield always has an exit and releases after disconnected state.
3. The same approved cat appears across poses and UI. Do not add more coats until identity and alpha QA pass.
4. Main application can be launched from its published folder on this Windows machine. Report whether it needs the installed .NET runtime or is self-contained.
5. Public-release claims require real browser installation and fixture/live tests, code signing, accessibility and hardware coverage; a local build is not a store release.

## Effort estimate

A capable Windows developer plus art support might need 3–5 engineering weeks for a polished beta and another 1–3 weeks for hardening/distribution; art can take 1–3 weeks with revisions. A narrower local alpha can be produced earlier. These are planning ranges, not delivery guarantees, and depend strongly on browser coverage and animation quality.

## Release sequence

Deliver a one-cat local alpha with the complete focus loop. Collect a week of feedback. Add app nudges and complete the motion set. Verify installer/extension onboarding on a clean machine. Expand feed coverage only after adapter fixtures establish correctness. Do not build billing, accounts, or a cosmetic store before the core loop is trusted.
