;; TableCraft Editor - Inno Setup Script
;; This script creates an installer for TableCraft.Editor

#define MyAppName "TableCraft Editor"
#define MyAppPublisher "kalulas"
#define MyAppURL "https://github.com/kalulas/TableCraft"
#define MyAppExeName "TableCraft.Editor.exe"

;; Get version from environment variable (set by build script, already cleaned)
#define MyAppVersion GetEnv("TABLECRAFT_VERSION")
#if MyAppVersion == ""
  #define MyAppVersion "1.0.0"
#endif

;; Get runtime from environment variable (set by build script)
#define MyAppRuntime GetEnv("TABLECRAFT_RUNTIME")

;; Get suffix from environment variable (set by build script)
#define MyAppSuffix GetEnv("TABLECRAFT_SUFFIX")

[Setup]
; Basic information
AppId={{F27235BC-9643-455F-9CA9-B6784B6EE86E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation directories
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}

; Output settings
OutputDir={#GetEnv("TABLECRAFT_RELEASE_DIR")}
;; Build the base filename
#if MyAppRuntime != ""
  #define BaseFileName "TableCraft-Editor-" + MyAppVersion + "-" + MyAppRuntime + "-setup"
#else
  #define BaseFileName "TableCraft-Editor-" + MyAppVersion + "-setup"
#endif

;; Add suffix if present
#if MyAppSuffix != ""
  #define FinalFileName BaseFileName + "_" + MyAppSuffix
#else
  #define FinalFileName BaseFileName
#endif

OutputBaseFilename={#FinalFileName}

; Compression and appearance
Compression=lzma
SolidCompression=yes
WizardStyle=modern

; Requirements
MinVersion=6.1sp1
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Privileges
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; Icons and license
SetupIconFile=TableCraft.Editor\Assets\CraftingTable_256.ico
LicenseFile=LICENSE

; Uninstall
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main application files
Source: "{#GetEnv("TABLECRAFT_RELEASE_DIR")}\{#GetEnv("TABLECRAFT_OUTPUT_DIR")}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Start Menu shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Desktop shortcut (optional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Option to launch the application after installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Clean up user data (optional - be careful with this)
Type: filesandordirs; Name: "{userappdata}\TableCraft"

[Code]
// Custom Pascal code for installation logic (if needed)
procedure InitializeWizard();
begin
  // Custom initialization code can go here
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  // Add custom pre-installation checks here if needed
end;