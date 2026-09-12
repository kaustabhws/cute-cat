# Handoff — native Windows installer

User request: after accepting the cat/notification fix, build a proper installation wizard, make the app appear in Windows Installed apps, and support uninstall from there. Scope: PKG-01–04. No computer plugin, sub-agent, publishing, new signing key or application-engine change was used.

## Implementation

- Inno Setup 7.1.0 native wizard, Windows light/dark styles, code-generated original cat sidebar/icon, clear local-preview/access page, shortcuts, confirmation, progress and completion.
- Stable AppId `{1597EF40-EF65-4A51-A78C-A82FBAA17344}`; one 64-bit HKLM uninstall entry, display icon/version/size/location, normal and quiet uninstall commands. No separate duplicate registry mechanism.
- New installs use Program Files/Cute Cat. Verified old manual 0.6.1 installs can be adopted. Same-version reinstall retains one app entry and certificate ownership.
- A setup-only .NET Framework helper uses Restart Manager for the exact installed executable; other returned processes are excluded from shutdown and the force flag is never used. It secures the program directory and removes only owned startup/shortcut/certificate entries.
- Inno tracks program files and explicit directories. `uninsalwaysuninstall` removes empty known directories inherited from manual installation without recursive deletion of unexpected user files. User app data is preserved.
- Same signed 0.6.1 UIAccess executable and reviewed payload. Fresh trust needs explicit consent; silent installation without acknowledgment exits before mutations. Newly added trust rolls back on setup failure. The local certificate still expires 12 October 2026; setup/uninstaller wrappers remain unsigned development executables.

## Actual verification

- `scripts/build-installer.ps1`: successful native setup build; payload hashes verified; current SHA-256 is in build status/final result.
- Native wizard navigation and cancel; actual PrintWindow captures reviewed in dark and light modes at the current 125% display scale. Light mode showed the access checkbox unchecked while trust was absent. Only the installer window was rendered, with no desktop capture.
- Initial adoption/installation: registry, icon paths, Start menu/desktop links and old shortcut cleanup passed; the existing cat closed through Restart Manager.
- Reinstall: exit 0; one stable app entry, correct location and retained certificate ownership; running test-profile cat closed safely.
- The exact registered uninstall command was executed. App files, shortcuts, entry and owned trust were removed, while nickname, focus progress, history and isolated test state were preserved.
- A discovered empty-directory remnant was corrected with explicit directory tracking. The repeat uninstall returned 0 and removed the complete old program directory.
- A discovered unattended-consent dialog was corrected with an initialization gate. The final negative test exited 1 without adding trust or creating an app entry.
- Final setup is installed in the canonical Program Files location. Final runtime/signature/registration results are in `artifacts/installer-qa`.
- Final runtime token check: UIAccess=1 and effective Administrator=true when launched from this administrative automation environment. Do not call that a measured standard-user launch. The wizard's completion action uses the original-user shell launch; broader account verification remains pending.

Evidence: `artifacts/installer-wizard-qa`, `artifacts/installer-wizard-light-qa`, `artifacts/installer-qa/install-result.json`, `uninstall-result.json` (initial run), `uninstall-cleanup-result.json` (corrected cleanup), `consent-result.json` and the installation/uninstallation logs. User-state backup was moved into `%LOCALAPPDATA%/CuteCat/setup-backups`, not included in the installer.

## Files and limits

Sources: `installer/`, `scripts/build-installer.ps1`, `tools/CuteCat.SetupArtwork`, `tests/CuteCat.SetupProbe`, and the updated `scripts/run.ps1`. Docs: installer contract, requirements, implementation plan, decisions, sources, getting started and build status. Runtime cat and notification sources are unchanged.

The new wizard is a local preview, not a signed public release. Future app/certificate versions require an upgrade/ownership-rotation test, especially from an adopted legacy directory. ARM64, non-English Windows and other DPI tiers are not claimed tested. The prior 92 core / 52 native cat checks are previous-build evidence, not tests rerun for this packaging-only change.

Next dependency-ready work follows the user's next priority; browser protection is still future scope. Keep this installer/AppId and uninstall ownership contract intact when updating the application.
