; Inno Setup script for the unpackaged WinGet build.
; Command Palette currently discovers MSIX packages. This installer also
; registers the COM local server so a future unpackaged host can find it.

#define MyAppName "CmdPalPomodoro"
#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif
#define MyAppPublisher "HamidRezaAshkiyan"
#define MyAppURL "https://github.com/HamidRezaAshkiyan/CmdPalPomodoro"
#ifndef ArchName
  #define ArchName "x64"
#endif
#ifndef ArchitecturesAllowed
  #define ArchitecturesAllowed "x64compatible"
#endif
#ifndef ArchitecturesInstallIn64BitMode
  #define ArchitecturesInstallIn64BitMode "x64compatible"
#endif

[Setup]
AppId={{38533641-1FFE-4633-8EF1-298AA24FF630}
AppName=Pomodoro
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={localappdata}\CmdPalPomodoro
OutputBaseFilename=CmdPalPomodoro-{#MyAppVersion}-{#ArchName}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
OutputDir=..\dist
MinVersion=10.0.19041
ArchitecturesAllowed={#ArchitecturesAllowed}
ArchitecturesInstallIn64BitMode={#ArchitecturesInstallIn64BitMode}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Pomodoro"; Filename: "{app}\CmdPalPomodoro.exe"

[Registry]
Root: HKCU; Subkey: "Software\Classes\CLSID\{{38533641-1ffe-4633-8ef1-298aa24ff630}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\CLSID\{{38533641-1ffe-4633-8ef1-298aa24ff630}\LocalServer32"; ValueType: string; ValueName: ""; ValueData: """{app}\CmdPalPomodoro.exe"" -RegisterProcessAsComServer"; Flags: uninsdeletekey

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
