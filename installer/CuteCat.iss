#include "generated\Build.iss"
#ifndef WizardTheme
  #define WizardTheme "dynamic"
#endif

[Setup]
AppId={{1597EF40-EF65-4A51-A78C-A82FBAA17344}
AppName=Cute Cat
AppVersion={#AppVersion}
AppPublisher=Cute Cat project
AppComments=A little company for your desktop.
DefaultDirName={code:GetInstallDirectory}
DefaultGroupName=Cute Cat
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableWelcomePage=no
UsePreviousAppDir=yes
UsePreviousTasks=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.22000
WizardStyle=modern {#WizardTheme} windows11
WizardSizePercent=110
WizardImageFile={#ArtDir}\wizard-light.bmp
WizardImageFileDynamicDark={#ArtDir}\wizard-dark.bmp
WizardSmallImageFile={#ArtDir}\small-light.bmp
WizardSmallImageFileDynamicDark={#ArtDir}\small-dark.bmp
WizardImageStretch=yes
SetupIconFile=..\assets\app.ico
UninstallDisplayIcon={app}\CuteCat.exe
UninstallDisplayName=Cute Cat
CreateUninstallRegKey=yes
Uninstallable=yes
UninstallLogMode=append
CloseApplications=no
RestartApplications=no
SetupMutex=CuteCat.Setup.1597EF40
OutputDir=..\dist\installer
OutputBaseFilename=CuteCat-{#AppVersion}-Setup
VersionInfoVersion={#SetupVersion}
VersionInfoProductVersion={#SetupVersion}
VersionInfoProductName=Cute Cat
SignTool=CuteCatSign
SignedUninstaller=yes
Compression=lzma2/normal
SolidCompression=yes
SetupLogging=yes
InfoBeforeFile=generated\Preview.txt

[Messages]
SetupWindowTitle=Cute Cat Setup
WelcomeLabel1=Welcome to Cute Cat
WelcomeLabel2=A little company for focused days.%n%nSetup will install Cute Cat on this PC and add it to Windows Installed apps. You can remove it there whenever you like.%n%nYour cat settings and focus history stay on this PC.
FinishedHeadingLabel=Your new desk companion is ready
FinishedLabel=Cute Cat is installed.%n%nOpen it from the Start menu or the desktop shortcut. On first open, your cat is set to start quietly with Windows. You can turn this off in Settings.%n%nTo remove it later, open Settings > Apps > Installed apps > Cute Cat. Uninstall keeps your settings and focus history.
ConfirmUninstall=Remove Cute Cat from this PC?%n%nThe app, its shortcuts, and certificate trust owned by Cute Cat will be removed. Your settings and focus history will be kept.

[Tasks]
Name: desktopicon; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: checkedonce

[Dirs]
Name: "{app}"; Flags: uninsalwaysuninstall
Name: "{app}\licenses"; Flags: uninsalwaysuninstall
Name: "{app}\Recovery"; Flags: uninsalwaysuninstall
#include "generated\Directories.iss"

[Files]
Source: "{#SupportExe}"; Flags: dontcopy
Source: "{#CertificateFile}"; Flags: dontcopy
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "GETTING-STARTED.md"
Source: "{#SupportExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "generated\Getting-started.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\third-party-notices.md"; DestDir: "{app}"; DestName: "CUTECAT-THIRD-PARTY-NOTICES.md"; Flags: ignoreversion
Source: "licenses\*.txt"; DestDir: "{app}\licenses"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Cute Cat"; Filename: "{app}\CuteCat.exe"; WorkingDir: "{app}"; Comment: "A little company for your desktop"
Name: "{autodesktop}\Cute Cat"; Filename: "{app}\CuteCat.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\CuteCat.exe"; Description: "Open Cute Cat"; Flags: shellexec nowait postinstall skipifsilent runasoriginaluser

[UninstallDelete]
Type: files; Name: "{app}\setup-state.ini"

[Code]
var
  AccessPage: TInputOptionWizardPage;
  AlreadyTrusted, AddedTrustThisRun, AddedCacheThisRun, InstallationAttempted, InstallationComplete: Boolean;

function Q(const Value: String): String;
begin
  Result := '"' + Value + '"';
end;

function GetInstallDirectory(Param: String): String;
var Legacy: String;
begin
  Result := ExpandConstant('{autopf}\Cute Cat');
  Legacy := ExpandConstant('{autopf}\{#LegacyDirectory}');
  if FileExists(Legacy + '\cute-cat-install.json') and FileExists(Legacy + '\CuteCat.exe') then
    if CompareText(GetSHA256OfFile(Legacy + '\CuteCat.exe'), '{#ExecutableSha256}') = 0 then
      Result := Legacy;
end;

function RunSupport(const Parameters: String; var ExitCode: Integer): Boolean;
begin
  Result := Exec(ExpandConstant('{tmp}\CuteCat.SetupSupport.exe'), Parameters,
    '', SW_HIDE, ewWaitUntilTerminated, ExitCode);
end;

function InitializeSetup: Boolean;
var Code: Integer;
begin
  ExtractTemporaryFile('CuteCat.SetupSupport.exe');
  ExtractTemporaryFile('local-preview.cer');
  Result := RunSupport('status ' + Q(ExpandConstant('{tmp}\local-preview.cer')), Code);
  AlreadyTrusted := Result and (Code = 10);
  Result := Result and ((Code = 0) or (Code = 10));
  if not Result and not WizardSilent then
    MsgBox('This local preview certificate has expired or could not be verified. Please use a newer Cute Cat setup.', mbError, MB_OK);
  if Result and WizardSilent and not AlreadyTrusted and (ExpandConstant('{param:ACCEPTUIACCESS|0}') <> '1') then begin
    Log('Unattended setup requires explicit /ACCEPTUIACCESS=1 when certificate trust has not been approved.');
    Result := False;
  end;
end;

procedure InitializeWizard;
var Detail: TNewStaticText;
begin
  AccessPage := CreateInputOptionPage(wpInfoBefore, 'Windows notification access',
    'Keep the cat visible when it taps a notification.',
    'Windows needs to grant this build access to other apps'' controls so the cat can appear above notification banners.', False, False);
  AccessPage.Add('Allow Cute Cat to appear above Windows notifications');
  AccessPage.Values[0] := AlreadyTrusted or (ExpandConstant('{param:ACCEPTUIACCESS|0}') = '1');
  Detail := TNewStaticText.Create(AccessPage);
  Detail.Parent := AccessPage.Surface;
  Detail.Left := 0;
  Detail.Top := ScaleY(112);
  Detail.Width := AccessPage.SurfaceWidth;
  Detail.Height := ScaleY(145);
  Detail.AutoSize := False;
  Detail.WordWrap := True;
  if AlreadyTrusted then
    Detail.Caption := 'This certificate is already trusted on this PC. Setup will reuse the access you previously approved.' + #13#10#13#10
  else
    Detail.Caption := 'For this local preview, Setup adds Cute Cat''s signing certificate to this PC''s trusted certificates. This is a machine-wide trust change.' + #13#10#13#10;
  Detail.Caption := Detail.Caption + 'Uninstall removes certificate trust added or adopted by Cute Cat. Your personal settings and focus history are kept.' + #13#10#13#10 +
    'Local preview certificate expires {#CertificateExpiry}. No subscription or automatic renewal is installed.';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if (CurPageID = AccessPage.ID) and not AccessPage.Values[0] then begin
    MsgBox('This preview needs the notification access described here. Select the option to continue, or cancel Setup.', mbInformation, MB_OK);
    Result := False;
  end;
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo,
  MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
begin
  Result := 'Install location:' + NewLine + Space + ExpandConstant('{app}') + NewLine + NewLine + MemoTasksInfo + NewLine + NewLine +
    'Windows integration:' + NewLine + Space + 'Start menu and Installed apps entry' + NewLine +
    Space + 'Uninstall keeps settings and focus history' + NewLine +
    Space + 'Notification access approved for this local preview';
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer;
begin
  Result := '';
  if not AccessPage.Values[0] then begin
    Result := 'Notification access has not been approved. Run the wizard to review it. For an explicitly approved unattended installation, pass /ACCEPTUIACCESS=1.';
    Exit;
  end;
  if not RunSupport('close ' + Q(ExpandConstant('{app}')), Code) or (Code <> 0) then begin
    Result := 'Cute Cat could not be closed safely. Choose Quit from its tray menu, then retry Setup.';
    Exit;
  end;
  if not RunSupport('trust ' + Q(ExpandConstant('{tmp}\local-preview.cer')), Code) or ((Code <> 0) and (Code <> 10)) then begin
    Result := 'Windows could not verify the certificate or grant the approved notification access.';
    Exit;
  end;
  AddedTrustThisRun := AddedTrustThisRun or (Code = 10);
  if not RunSupport('cache ' + Q(ExpandConstant('{app}')) + ' ' + Q(ExpandConstant('{srcexe}')), Code) or ((Code <> 0) and (Code <> 10)) then begin
    Result := 'Setup could not verify or save its recovery installer. Installed program files have not been replaced.';
    Exit;
  end;
  AddedCacheThisRun := AddedCacheThisRun or (Code = 10);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var Code: Integer; Owned: String;
begin
  if CurStep = ssInstall then InstallationAttempted := True;
  if CurStep = ssPostInstall then begin
    if AddedTrustThisRun then Owned := '1' else Owned := '0';
    if not RunSupport('complete ' + Q(ExpandConstant('{app}')) + ' ' + Owned + ' ' + Q(ExpandConstant('{srcexe}')), Code) or (Code <> 0) then
      RaiseException('Cute Cat could not finish installation safely. Your personal data has not been removed.');
    InstallationComplete := True;
  end;
end;

procedure DeinitializeSetup;
var Code: Integer;
begin
  if AddedCacheThisRun and not InstallationComplete then
    RunSupport('undo-cache ' + Q(ExpandConstant('{app}')), Code);
  if AddedTrustThisRun and not InstallationComplete then
    RunSupport('undo-trust', Code);
end;

function GetCustomSetupExitCode: Integer;
begin
  Result := 0;
  if InstallationAttempted and not InstallationComplete then Result := 20;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var Code: Integer;
begin
  if CurUninstallStep = usUninstall then begin
    if not Exec(ExpandConstant('{app}\CuteCat.SetupSupport.exe'),
      'uninstall ' + Q(ExpandConstant('{app}')), '', SW_HIDE, ewWaitUntilTerminated, Code) or (Code <> 0) then
      RaiseException('Cute Cat could not be closed or its setup state could not be verified. Choose Quit from the tray menu, then retry Uninstall. Your settings and focus history are preserved.');
  end;
end;
