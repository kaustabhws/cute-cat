# Handoff — actual Windows 11 toast layout, 12 September 2026

Task: NOT-04–06 follow-up. User reported a visible Snipping Tool notification being ignored and the cat rendering behind it. Continue without the computer plugin. No sub-agent work was used.

## Implemented and measured

Captured control structure (no notification content) identifies a native ShellExperienceHost CoreWindow containing ToastCenterScrollViewer → NormalToastView / FlexibleToastView → DismissButton. The example toast rectangle was (1445,582,455,422), with close rectangle (1841,811,50,50), below the image preview. Old discovery omitted immersive windows; the old template and top-corner-only check also rejected this view.

Updated ShellNotifications/Native discovery and resolver, retained narrow process/control identity validation, and added exact NotificationCenterGrid exclusion. Added native regression checks and a bounded opt-in structural observation command. CatSurface raises among permitted topmost peers on approach; this alone does not cross Windows' protected notification layer.

The running standard 0.6.1 build captured a real shell target, started moving in 64.3 ms, reached painted contact after 1.865 s with 0.157 px error, and confirmed control disappearance after invocation at 3.957 s. See `artifacts/live-v061/notification-observation.jsonl`. No WinRT dismissal event is claimed for another app's notification.

## Checks run

- `dotnet build CuteCat.slnx -c Release`: passed.
- Core checks: 92 passed, 0 failed.
- Native executable `--qa artifacts/qa-v061`: 52 passed, 0 failed, including root-as-toast, live scroll wrapper, actual flexible template, below-image X, options/action/outside geometry rejection and history exclusion.
- Self-contained x64 standard publish succeeded. Running `dist/CuteCat-0.6.1/CuteCat.exe` with the existing user profile; state backed up before launch.
- UIAccess alternate publish/sign preparation succeeded. PE resource checks confirm standard manifest uiAccess=false, alternate uiAccess=true, both asInvoker and version 0.6.1.0. Hashes: `artifacts/manifest-verification-v061.json`.
- Installer validation-only mode verified all hashes. After explicit approval, installation succeeded and signature validation is Valid. Runtime UIAccess=true and native window order above shell notifications are verified. The signing key was removed; the user-approved public certificate is in LocalMachine/Root. Install and removal scripts parse successfully; removal has not been run.

No cat art/rig change was necessary. Native UI/renderer outputs exist in the QA directory. The protected-layer issue remains in the standard portable build; the approved installed build now has the required Windows layer.

## Approved installation and remaining feedback

The user explicitly answered **Install the UIAccess test build**. That approval persists; do not request it again. Installed path: `C:/Program Files/CuteCat-Local-0.6.1-38FF99FF`. Package: `dist/CuteCat-0.6.1-uiaccess-review`; public certificate thumbprint `38FF99FFF0ECA4AB46CDE5440CFD9ACDF37FD8D1`, expires 12 October 2026. The signing key was deleted. The reviewed machine root entry and secure Program Files directory were created under that authorization. No UAC policy or account change was made.

Microsoft says UIAccess is for assistive technology and discourages using it merely for ordinary app topmost behavior. The user was informed before approval. `docs/16-uiaccess-review.md` records scope and rollback. The installed process reports UIAccess=true; `artifacts/layers-v061-uiaccess.json` shows the cat at z-order index 0 in band 2 and the shell toast below in band 4. `artifacts/layers-v061-uiaccess-live.jsonl` is a bounded, content-free native layer/geometry observer. Final user visual confirmation was requested; do not invent their answer or conflate the earlier standard-build dismissal with a new installed-run measurement.

The desktop Cute Cat shortcut targets the installed build. `scripts/run.ps1` prefers that matching installed/trusted signature; `-Standard` selects the preserved ordinary build. The user state was preserved. The certificate's 30-day expiry remains a material limit, and the local installation must be replaced or removed then. No automatic updater/renewal was added.

Final native live evidence: `artifacts/uiaccess-live-summary-v061.json` and the underlying layer JSONL captured a real toast at 487.31 seconds of observer time, cat travel from x=244 to x=1611, above the notification in all 28 recorded visible-toast samples including 11 overlaps, toast disappearance at 491.98 seconds, and return travel. This proves the installed foreground layer during the interaction, while the earlier application observation proves exact UIA invocation/disappearance. It is not a screenshot, user testimonial or a new WinRT dismissal-reason event. The temporary layer observer was stopped; the installed app remains running.

Changed files: ShellNotifications.cs, Native.cs, CatSurface.cs, CompanionHost.cs, App.xaml.cs, QualityChecks.cs, NotificationControlFixture.cs, RuntimeAccess.cs, csproj/manifests; dev-only ShellProbe observers; UIAccess preparation/install/removal scripts; current docs/contracts/status/source register. See D35–D36.

Next dependency-ready work is collecting final installed interaction feedback, then addressing any real failure it exposes before browser protection. No arbitrary input injection, notification content reads, global UI history collection or security-policy bypass is authorized.
