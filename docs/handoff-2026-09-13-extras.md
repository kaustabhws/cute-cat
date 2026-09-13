# Handoff — companion extras and shortcut activation

**Publication update, 13 September 2026:** the user subsequently authorized the source push and 1.1.1 release. Earlier local-only statements below are historical; see [release notes](releases/v1.1.1.md).

## Delivered locally

Version 1.1.0 implements EXTRA-01–07 from [the contract](23-companion-extras.md). Desktop/Start shortcuts open the running panel instead of producing an already-running dialog, including hidden, minimized and maximized cases. The six additions are grace countdowns, troubleshooting/support reports, saved outfits/procedural coats, expressive petting/optional purr, optional break cues with focus return, and reviewed settings transfer. Browser URL detection remains deferred.

The installer is at `dist/installer/CuteCat-1.1.0-Setup.exe` and is installed in `C:\Program Files\Cute Cat`. Windows Installed apps reports 1.1.0. No source push, tag or GitHub release was performed; v1.0.0 remains the published version. The installer is 58,867,824 bytes; SHA-256 `BFB88FE6FB333EC7032B6AB052C1C3D4C10D195764327E027CF9BD20A3A2BEBC`.

Signing reuses the previously approved protected preview certificate `93ECC442E6EC72D1238D6BCA2E8F80D9B01D88EE`; the private CNG key was not exported. App/helper/setup/uninstaller signatures and timestamps validate. This is not a publicly verified publisher identity.

## Source and contracts

- Core: `CompanionExtras.cs`, `SettingsTransfer.cs`, schema-5 `StateStore`, `PetAppearance`, `FocusProfiles`, focus return in `FocusSession`, procedural actions in `CatRig`/`Companion`.
- Windows/UI: `CompanionHint`, `CompanionHost.Extras`, `SingleInstanceActivation`, `SupportDiagnostics`, `MainWindow.Tools`, existing host/window/profile/wardrobe entry points and procedural purr audio.
- Art: `CatPainter.Patterns` plus existing painter clipping/expressions. Existing bandana/chin layers and paw geometry are preserved.
- Tests: `ExtrasChecks`, `QualityChecks.Extras`, the schema and new pet-action expectations in existing checks. CI now has an extras/shortcut step, not yet executed remotely for this local candidate.
- Docs: requirements, implementation plan, data/privacy, wardrobe, decisions D48–D52, rig contract version 7, new extras contract, getting started, README and build status. `docs/images/procedural-coats.png` and `saved-outfits.png` are diagnostic documentation art, never runtime assets.

## Checks actually run

| Run | Result | Evidence |
| --- | --- | --- |
| `scripts/build.ps1 -Publish` | Zero warnings/errors; 210 core checks passed | `artifacts/v110-build.log` |
| Standard self-contained extras suite | 25 passed | `artifacts/v110-extras-final` |
| Signed installed extras suite | 25 passed | `artifacts/v110-installed-extras` |
| Signed installed full native regression | 82 passed | `artifacts/v110-installed-full` |
| Signed installed tray/cat menu opening | 10 passed | `artifacts/v110-installed-opening` |
| Signed installed menu lifecycle, simulated activation | 19 passed | `artifacts/v110-installed-lifecycle` |
| Signed installed appearance/native captions | 12 passed | `artifacts/v110-installed-appearance` |
| `test-installer.ps1 -Install` | 14 passed; registration/signatures/recovery/tamper rejection and nickname/history preservation | `artifacts/install-v110-local` |

The signed payload and installer were built with `prepare-uiaccess-review.ps1` and `build-installer.ps1`. All native runs used isolated state. Existing full/menu/appearance suites also passed before packaging. New shortcut tests launch actual second processes against the first instance. Native cue tests verify visibility, the MA_NOACTIVATE message response and event wiring; app foreground identity is explicitly injected in the controlled fixture. No computer-control plugin or system input injection was used.

UI exports were rendered via `scripts/render-previews.py` and inspected in light/dark themes and at the minimum width. Inspected pattern sheets and pet/stretch/drink sequences; all tested patterns/actions/yaws stayed inside the canvas. Maximum sampled head-tilt step for the new reactions was under 0.8 degrees at 50 Hz. These animation exports are diagnostic outputs, not input sprite frames.

## Remaining acceptance

Physical outside-click/menu/submenu operation, cue clicking without stealing focus, live Windows toast overlap/dismissal, standard-account integrity transitions, mixed-DPI monitor placement, screen readers and purr volume need an active user desktop. RDP fixture results do not claim that hardware matrix. Current scopes remain exact executable normal-close, untouched save prompts, optional helper permissions and no browser monitoring.

Before any public release, obtain user approval of the installed changes, then publish the reviewed source and this matching installer/checksum. Production signing and UIAccess eligibility remain governed by `22-public-release-guide.md`. A downgrade to 1.0.0 preserves schema-5 data read-only; `.schema4.json` retains the pre-upgrade configuration.
