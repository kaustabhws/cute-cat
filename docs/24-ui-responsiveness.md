# UI responsiveness and startup — 1.1.1

**Publication update, 13 September 2026:** the user subsequently authorized the source push and 1.1.1 release. Earlier local-only statements below are historical; see [release notes](releases/v1.1.1.md).

The user reported lag in the app window and toggles, explicitly clarified that desktop-cat animation was already fine, and requested smooth maximize/restore reflow, lower resource use, startup enabled by default, and local installation without publication.

## Implemented behavior

- `ControlMotion` interpolates toggle position and colour opacity over 160 ms and content width over 180 ms. A single dispatcher timer at background priority exists only during a transition. Time starts at the actual interaction, avoiding a jump caused by a stale WPF presentation clock under RDP. Rapid reversal starts from the displayed value; initialization/unload and reduced motion settle immediately. Compiled-template transforms are cloned if frozen.
- Checkbox preference handlers observe Checked/Unchecked, including accessibility toggle requests. Visual feedback does not wait for persistence. The main app content smoothly reflows when the viewport changes, including maximize and restore; normal Windows caption behavior remains.
- One background state writer retains only the latest pending snapshot. A 120 ms quiet period combines quick edits; a long burst checkpoints at least once per second. Shutdown flushes the last snapshot. Saves remain atomic, policy cancellation still happens immediately, and transient I/O failure is visible and recoverable.
- The frame callback yields to mouse/keyboard input. Desktop-cat action geometry, gait, paw contact, and configured frame rates are unchanged.
- UI previews render at their displayed physical resolution using reused buffers. Hidden/minimized/off-screen previews stop updating, unchanged images are not rebuilt, and static wardrobe thumbnails use a bounded cache. Hidden saved-outfit cards are created when their tab is opened.
- Disabled notification/app helpers stop their 100 ms scan timer. Enabled helpers retain that interval and existing identity/epoch validation. Status refreshes are combined; unchanged primary-button content and unchanged theme resources are left intact.

## Startup default

`Preferences.Startup` defaults true. Additive schema-5 field `StartupInitialized` records that the new default has been applied. The first normal 1.1.1 launch applies the user-requested startup-on default, including migration from the old unrecorded false default. An explicit subsequent off choice sets `StartupInitialized=true` and is retained across restarts, imports and upgrades.

The normal app reconciles only its `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\CuteCat` entry to the installed executable plus `--tray`. Startup displays the pet while keeping the main panel closed. Tests/custom data directories and read-only future settings do not register startup. Import preserves the current machine's startup preference and initialization marker. The installer explains this first-open behavior; owned startup cleanup is unchanged. This user request supersedes the older startup-off specification. Automatic update checking and sounds remain optional/off by default.

## Validation contract

The reproducible `--responsiveness-checks` fixture uses 8 profiles, 50 rules per profile and 24 saved outfit records. It measures app-owned toggle handler time, Input-priority dispatcher probes, allocation rate, navigation and process CPU in this session. These are synthetic local measurements, not physical input-to-photon latency or hardware guarantees.

Baseline: `artifacts/v111-ui-before` (1.1.0 with measurement entry point only). Post-change: `artifacts/v111-responsiveness-final`. Both use software rendering under RDP and the same fixture. Measured toggle-handler p95 fell from 9.062 ms to 0.764 ms; mean repeated wardrobe navigation from 94.774 ms to 33.090 ms; allocation during the toggle burst from 5.794 to 1.725 MiB/s. CPU varies across short runs and no universal CPU reduction is claimed. Native-desktop frame quality must still pass its existing tests.

Checks cover initial/intermediate/final toggle positions, reversal, accessibility-to-preference wiring, resize widths, real window maximize/restore, reduced motion, a stopped animation timer after completion, latest-state persistence, failure recovery and shutdown flush. Run the existing full native, menu, appearance, extras and installer suites before delivery. Physical feel over an active desktop, mixed-DPI resizing and RDP presentation quality remain user acceptance checks.

Publication remains on hold. Current executed counts and installer identity belong in build status and the handoff.
