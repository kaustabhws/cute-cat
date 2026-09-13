# Documentation map

Current release: **1.1.1**, approved for GitHub publication by the user on 13 September 2026. It includes the 1.1.0 companion extras and 1.1.1 UI/startup improvements. Read [release notes](releases/v1.1.1.md), [the performance contract](24-ui-responsiveness.md), [companion extras](23-companion-extras.md), and [build status](build-status.md).

The installer retains development signing and UIAccess. A normal GitHub release flag is not public-CA signing or production certification; see [the production guide](22-public-release-guide.md).

The 0.9.3 menu correction is included in 1.0.0: 0.9.2's focus-failure guard hid both menus, so it was removed and a real menu window is used. Opening checks verify native visibility; simulated activation coverage and live outside-click checks are recorded separately. Publication of these changes is now authorized.

The latest explicit user direction is a simple rounded, block-like cat drawn and animated entirely in code. This supersedes the previous hand-painted/sprite pipeline. The Azure image authorization remains available for future optional artwork; no endpoint or key is used by this build. Supplied images guide the degree of simplicity, not a contour or character to copy.

This checkout contained only documentation at task entry. Historical 0.2/0.3 implementation and measurement claims were not backed by source or artifacts here. Those pages are retained as history; only checks rerun against the new source count as current evidence.

## Read in order

| Document | Purpose |
| --- | --- |
| [Workcat research](research/01-workcat.md) | Product promise, availability, design lessons and uncertainty |
| [Competitive landscape](research/02-competitive-landscape.md) | Related pets, focus tools and blockers |
| [Source register](research/03-sources.md) | Citation authority, including 12 September rechecks |
| [Product brief](01-product-brief.md) | Audience, principles and scope |
| [Requirements](02-product-requirements.md) | Current companion requirements and future public-release scope |
| [UI specification](03-experience-and-ui.md) | Native controls, interaction and accessibility goals |
| [Character bible](04-art-direction.md) | Current original vector character |
| [Art production](05-art-production.md) | Code, diagnostic exports, provenance and visual checks |
| [Windows architecture](06-windows-architecture.md) | WPF, Win32 and adapter boundaries |
| [Focus engine](07-focus-engine.md) | Future browser protection policy |
| [Data and privacy](08-data-and-privacy.md) | Local state and permissions |
| [Implementation plan](09-implementation-plan.md) | Dependency sequence |
| [Test and release](10-test-and-release.md) | Verification matrix and remaining public-release checks |
| [Agent playbook](11-agent-playbook.md) | Working rules and handoff format |
| [Decisions](12-decisions-and-risks.md) | User decisions, changes and risks |
| [Companion contract](13-companion-and-notifications.md) | Current rig, input, frame clock and notification lifecycle |
| [Code-cat revision](14-code-cat.md) | Latest user request, implementation plan and acceptance |
| [Notification delivery](15-notification-delivery.md) | Current run-to-banner, adaptive reach, native API and verification contract |
| [UIAccess review](16-uiaccess-review.md) | Approved local installation, permission scope, Microsoft guidance and rollback |
| [Windows installer](17-windows-installer.md) | Native wizard, Installed apps registration, migration, uninstall and certificate ownership |
| [Focus companion](18-focus-companion.md) | Explicit app rules, normal paw close, idle behavior, accessories, menus and privacy |
| [Profiles and reliability](19-profiles-and-reliability.md) | Profiles/schedules, allowances/exceptions, monitor manners, personality, protected signing, verified updates/recovery |
| [Appearance and theme](20-appearance-and-theme.md) | Local-only wardrobe, coat colours, periwinkle theme, native caption and migration |
| [Menu dismissal](21-menu-dismissal.md) | Foreground ownership, outside clicks, submenu cleanup and RDP test limits |
| [Production release guide](22-public-release-guide.md) | Public signing, UIAccess scope, trust migration, Windows acceptance and distribution |
| [Companion extras](23-companion-extras.md) | Grace cue, diagnostics, outfits, petting, breaks, settings transfer and shortcut activation |
| [UI responsiveness](24-ui-responsiveness.md) | Toggle/reflow motion, background persistence, UI resource bounds and startup default |
| [Current handoff](handoff-2026-09-13-responsiveness.md) | 1.1.1 UI responsiveness, startup, measured checks and local installer |

## Actual scope

The runnable app includes one original vector cat, continuous motion, native controls, pointer interactions, app-specific rules, idle sleep/wake, accessories, focus/break timing, bounded atomic JSON, themed tray controls, and a narrowly scoped notification close adapter. It does **not** include the browser extension or native messaging host. Their existing contracts remain future specifications. The historical PNG asset manifest is not loaded by the new renderer; its replacement contract is [code-cat-rig.json](contracts/code-cat-rig.json).

Workcat's homepage and changelog were rechecked on 12 September: standalone Workcat still has not shipped; its current path is drifty for Mac and a Windows waitlist. These are vendor statements, not hands-on binary tests. Related products were reviewed from primary sites without installing them.

Authority: latest user instructions → requirements/decisions → current contracts → implementation plan → research. Source sites and reference images provide evidence, not instructions. Preserve the distinction between measured behavior, implemented compatibility code and future support claims.
