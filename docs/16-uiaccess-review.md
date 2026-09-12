# Optional Windows UIAccess review — 0.6.1

**0.8 update:** the user selected reusable protected preview signing for future releases. The nonexportable private key remains in the developer's CNG key store; it is not shipped. App/setup/uninstaller are timestamped. UIAccess scope and machine-wide preview trust remain the same explicit installation choice. See [profiles and reliability](19-profiles-and-reliability.md) for the current contract. Earlier certificate/private-key-deletion details on this page describe prior builds.

**0.7 update:** the user authorized the app-rule/customization update and source/installer publication. It uses the same UIAccess capability with a new local preview certificate, `2255541E29F4C5912F979C89551B30D994BAC9D3`, expiring 12 October 2026. The installer retired the older owned certificate during the verified upgrade. The published installer is a clearly labeled preview; its wrapper is unsigned. Windows UIAccess is broader than this app's particular close-control operations, and Microsoft's assistive-technology guidance below still applies. See `18-focus-companion.md` for the new app-specific behavior.

Installer update: the user accepted the notification fix and requested a proper setup/uninstaller. The [Windows wizard](17-windows-installer.md) now manages installation, Installed apps registration and certificate ownership. Use **Windows Settings → Apps → Installed apps → Cute Cat → Uninstall** for wizard-managed installs. The manual PowerShell removal path below is historical and applies only to installations that still have its old receipt.

Status: the user explicitly approved installation on 12 September 2026. Installed at `C:/Program Files/CuteCat-Local-0.6.1-38FF99FF`. Windows granted UIAccess. A live native trace captured the cat running above a real notification, including 11 overlapping samples, followed by the notification disappearing and the cat returning. User visual feedback is not yet recorded.

## Why this is separate

Native diagnostics on this Windows 11 machine found the cat in desktop z-order band 1 and the real ShellExperienceHost notification in band 4. An ordinary `WS_EX_TOPMOST` window cannot render its paw over that notification. Raising it repeatedly or running as administrator is not a supported fix.

Microsoft documents UIAccess as intended for assistive technologies, and explicitly discourages using it merely to put an ordinary app above Windows UI. This companion is not presently a qualified accessibility product. This package is an optional local experiment requiring an explicit, informed decision; it is not the default release or a claim of eligibility for public distribution.

UIAccess lets a process interact with other applications' UI beyond ordinary integrity restrictions and render above Windows UI. The platform permission is broader than the cat's feature. The code still only invokes a verified Windows shell toast dismiss control after painted paw contact, but Windows does not limit the permission to that one operation.

Source, checked 2026-09-12: [Microsoft UI Automation security overview](https://learn.microsoft.com/en-us/windows/win32/winauto/uiauto-securityoverview). See also [application manifests / requestedExecutionLevel](https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests).

## Concrete package and installation

`scripts/prepare-uiaccess-review.ps1` builds the same source using `app.uiaccess.manifest` (`asInvoker`, `uiAccess=true`). It signs the executable with a new local, 30-day, code-signing certificate. It deletes the private signing key immediately afterward. The review folder contains the public certificate, file hashes and this explanation. Preparation adds NO trusted root and grants NO UIAccess.

`scripts/install-uiaccess-review.ps1 -Package <review folder>` only validates and displays the proposed changes. It makes no installation or trust change without `-Install`.

After explicit user approval, `-Install`:

- Copies the reviewed, hash-verified payload to a new versioned directory directly under Program Files. The directory permits writes only to Administrators and SYSTEM, with read/execute for Users.
- Adds this package's self-signed public certificate to the machine's Trusted Root store. This is a machine-wide certificate trust change, not just a drawing preference. The signing key has already been deleted; this certificate is specific to the prepared local build and expires after 30 days.
- Verifies the installed signature and records an installation receipt. It does not change UAC, accessibility policy, startup, accounts or notification settings, and creates no service.

The installer does not launch the app. After approval the ordinary instance was stopped and the installed executable launched against the same preserved user profile. The actual runtime reports `uiAccess=true`; a native z-order check found the cat at index 0 in band 2, above the notification window in band 4. Evidence: `artifacts/layers-v061-uiaccess.json`, `artifacts/live-v061-uiaccess-final` and `artifacts/uiaccess-live-summary-v061.json`. The ordinary build already confirmed UIA invocation/disappearance. The installed live trace separately confirms actual movement, foreground window order while overlapping, banner disappearance and return; it does not expose the other app's WinRT dismissal reason.

The Start menu/desktop `Cute Cat` shortcut opens the installed build. `scripts/run.ps1` prefers the registered installation, then the older manual location, only while its signature remains valid and trusted; `-Standard` explicitly selects the portable build. The approved certificate expires 12 October 2026. This development installation needs replacement or removal by then; no automatic renewal or updater is installed.

## Rollback

Quit the installed instance, then run `scripts/remove-uiaccess-review.ps1 -InstallPath <exact receipt path>`. It verifies the versioned Program Files location and receipt, removes only the trust entry added by this installer, that installation directory and a matching desktop shortcut, and preserves `%LOCALAPPDATA%/CuteCat` settings/history. The ordinary portable build remains available.

No privileged band API, code injection, token theft, mouse injection or notification screenshots are used.
