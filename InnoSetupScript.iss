; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Soup"
#define MyAppVersion "1.0.4.1"
#define MyAppPublisher "Andrew Mitchell"
#define MyAppURL "https://andrewmitchell4.typeform.com/to/EPQpiB"
#define MyAppExeName "Soup.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{5F312DB2-6584-4A53-AF40-6C7737897A4A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=SoupSetup
SetupIconFile=Soup Archives\bin\MakeSetup\Icons\01-MixedLogo.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
UninstallDisplayName=Soup
UninstallDisplayIcon={app}\Soup.exe
DisableStartupPrompt=False
DisableWelcomePage=False

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "Soup Archives\bin\MakeSetup\Soup.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Soup Archives\bin\MakeSetup\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "Soup Archives\bin\MakeSetup\Helpers\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Flags: nowait postinstall skipifsilent; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"
Filename: "{app}\Helpers\PostSetup.bat"; WorkingDir: "{app}\Helpers"; Flags: runasoriginaluser; Description: "Make Soup Default"

[Registry]
Root: "HKCR"; Subkey: ".7zip"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".7z"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".zlib"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".gz"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".tgz"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".7-zip"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".z"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".rar"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".tar"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: ".zip"; ValueType: string; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: "HKCR"; Subkey: "{#MyAppName}"; ValueType: string; ValueData: "Program {#MyAppName}"; Flags: uninsdeletekey
Root: "HKCR"; Subkey: "{#MyAppName}\DefaultIcon"; ValueType: string; ValueData: "{app}\{#MyAppExeName},1"
Root: "HKCR"; Subkey: "{#MyAppName}\shell\open\command"; ValueType: string; ValueData: """{app}\{#MyAppExeName}"" ""tryOpen"" ""%1"""

[Dirs]
Name: "{app}\Helpers"
