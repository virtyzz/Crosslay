#define MyAppName "Crosslay"
#define MyAppPublisher "Crosslay"
#define MyAppExeName "Crosslay.exe"
#define MyAppVersion GetEnv("CROSSLAY_VERSION")

#if MyAppVersion == ""
  #define MyAppVersion "0.1.0"
#endif

[Setup]
AppId={{7E8F9DF7-7C0A-4D93-9DA8-4B4C6D40F23F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
OutputDir=..\artifacts\installer
OutputBaseFilename=Crosslay-Setup-{#MyAppVersion}
SetupIconFile=..\assets\crosslay.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
CloseApplicationsFilter={#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[InstallDelete]
Type: files; Name: "{app}\{#MyAppExeName}"

[Code]
procedure StopRunningApp();
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{cmd}'), '/c taskkill /IM "{#MyAppExeName}" /T >nul 2>nul', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(1500);
  Exec(ExpandConstant('{cmd}'), '/c taskkill /IM "{#MyAppExeName}" /T /F >nul 2>nul', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  StopRunningApp();
  Result := '';
end;

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
