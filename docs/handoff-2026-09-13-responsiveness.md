# Handoff — 1.1.1 UI responsiveness and startup

**Publication update, 13 September 2026:** the user subsequently authorized the source push and 1.1.1 release. Earlier local-only statements below are historical; see [release notes](releases/v1.1.1.md).

## User request and delivered scope

The user clarified twice that the lag was in the app UI, not the desktop cat, and specifically requested smooth maximization/restoration. They also requested startup enabled by default, a new local installer and no GitHub publication until they approve.

Implemented: brief elapsed-time toggle and layout transitions; rapid reversal without a starting jump; accessibility toggle-to-preference wiring; one background latest-state writer with shutdown flush and I/O recovery; bounded/lazy wardrobe thumbnails; display-resolution UI preview buffers and fewer redraws; input priority ahead of cat-frame work; combined UI refresh and disabled-helper polling suppression. Desktop-cat action parameters, paw contact rules and configured frame rates are unchanged. Startup defaults on at first normal 1.1.1 launch, records an additive schema-5 initialization marker, and preserves later explicit opt-outs. QA/custom data/read-only future states do not register startup.

## Main files

`ControlMotion.cs`, `MainWindow.Motion.cs`, `App.xaml`, main/wardrobe views, `CatPreview.cs`, `FrameClock.cs`, `CompanionHost.cs`, `CoalescedStateWriter.cs`, `StartupRegistration.cs`, startup fields/import preservation, installer completion/help text, `QualityChecks.Performance.cs` and `PersistenceChecks.cs`. The small PetMenu additions are diagnostic open/close counters; its successful activation/capture/dismissal strategy is retained. Current requirements, defaults, privacy, decisions D53–D55, getting started, build status and the new [contract](24-ui-responsiveness.md) are updated.

## Executed validation

- `scripts/build.ps1 -Publish`: zero warnings/errors and **217 core checks**, including coalescing, in-flight writes, final flush, failure recovery and startup opt-out preservation.
- App-owned before/after responsiveness fixture: baseline `artifacts/v111-ui-before`; optimized `artifacts/v111-responsiveness-final`. Software rendering/RDP, 8 profiles × 50 rules and 24 outfit records. Toggle-handler p95 9.062 → 0.764 ms; wardrobe-navigation mean 94.774 → 33.090 ms; toggle-burst allocation 5.794 → 1.725 MiB/s. These are controlled measurements, not hardware input latency or guaranteed CPU savings.
- Installed UIAccess build: **13** responsiveness/resize checks, **85** native regression checks, **25** extras, **10** menu opening, **19** explicitly simulated-activation menu lifecycle, **12** appearance/caption. All passed under `artifacts/v111-installed-*`.
- `test-installer.ps1 -Install`: **14** passed, including Installed apps registration, app/helper/setup/uninstaller signatures and timestamps, recovery identity, update tamper rejection, nickname/history preservation.
- Light/dark control exports, mid-resize/settled layouts and actual-size previews were rendered and inspected. The UI animation timer is checked to stop after completion. Existing active-cat cadence remains covered by the full suite.

An initial combined native run reported AutonomyPaused=true during pointer tests. An instrumented rerun verified matching menu opens/closes and passed; the pointer fixture now explicitly dismisses the menu and asserts that precondition. The final installed full suite passed. No recurring menu failure was identified. Installed UIAccess launches must use Windows shell activation; attempting stdout/stderr-redirection launch from the shell lacks that broker and returns error 740. The corrected shell launches all succeeded.

## Installer and local state

Installed in `C:\Program Files\Cute Cat` as 1.1.1. Installer: `dist/installer/CuteCat-1.1.1-Setup.exe`, 58,877,048 bytes, SHA-256 `3B7F53F7AD93F537DF5D7407C621CEFC02EF72270DD282A4FA65963CE597024A`. Same approved protected preview signing certificate `93ECC442E6EC72D1238D6BCA2E8F80D9B01D88EE`; no private key export or new trust identity. No push/tag/release was performed. The previous published release is still v1.0.0.

## Remaining acceptance

The user should assess perceived smoothness on their active desktop and mixed-DPI/maximize/restore configurations. RDP/compositor delivery can differ from measured handler timings. Physical mouse and live-notification overlap checks are not replaced by synthetic input callbacks. Windows startup configuration is verified without logging off or rebooting the user's PC. Public-trust signing and UIAccess qualification remain separate from this local development-signed installer.
