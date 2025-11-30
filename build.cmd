@echo off
setlocal enabledelayedexpansion

REM Build configuration
set _runtime=win-x64
set _self-contained=false
set _publish_single_file=false
set _copy_config=true
set _suffix=
set _launch=false
set _zip=false
set _installer=false

REM Tool paths - modify these if tools are installed in different locations
set _7z_path=C:\Program Files\7-Zip\7z.exe
set _inno_compiler=C:\Users\Kalulas\AppData\Local\Programs\Inno Setup 6\ISCC.exe

REM Get version from git tag
for /f "tokens=*" %%i in ('git describe --tags --abbrev^=0') do set _version=%%i
if "%_version%"=="" (
  echo Warning: Could not get version from git tag, using fallback v0.9.9
  set _version=v0.9.9
)

REM Create version without 'v' prefix for file naming
set _version_clean=%_version%
if "%_version:~0,1%"=="v" (
  set _version_clean=%_version:~1%
)

REM Parse command line arguments
:parse_args
if "%1"=="--no-copy-config" (
  set _copy_config=false
  shift
  goto parse_args
)

if "%1"=="--launch" (
  set _launch=true
  shift
  goto parse_args
)

if "%1"=="--zip" (
  set _zip=true
  shift
  goto parse_args
)
if "%1"=="--installer" (
  set _installer=true
  shift
  goto parse_args
)

REM Check for suffix parameter --suffix=value
if "%1"=="--suffix" (
  REM Handle --suffix value format
  if "%2" NEQ "" (
    set "_suffix=%2"
    shift
    shift
    goto parse_args
  )
)

if "%1"=="/?" goto show_help
if "%1"=="-h" goto show_help
if "%1"=="--help" goto show_help

if not "%1"=="" (
  echo Unknown argument: %1
  goto show_help
)

goto start_build

:show_help
echo Usage: build.cmd [options]
echo.
echo Options:
echo  --no-copy-config  Do not copy test configuration files from Example directory
echo  --suffix=STRING   Add suffix to output directory name (e.g., --suffix=preview)
echo  --launch          Launch TableCraft.Editor.exe after successful build
echo  --zip             Create a zip archive of the output directory
echo  --installer       Create an installer using Inno Setup
echo  -h, --help, /?    Show this help message
echo.
echo Default behavior: Copy configuration files from Example directory to output
echo Output location: publish\VERSION\OUTPUT_DIR[_SUFFIX]
exit /b 0

:start_build

REM Check tool availability
if "%_zip%"=="true" (
  if not exist "%_7z_path%" (
    echo Error: 7-Zip not found at: %_7z_path%
    echo Please install 7-Zip or update the _7z_path variable in this script.
    pause
    exit /b 1
  )
)

if "%_installer%"=="true" (
  if not exist "%_inno_compiler%" (
    echo Error: Inno Setup compiler not found at: %_inno_compiler%
    echo Please install Inno Setup or update the _inno_compiler variable in this script.
    pause
    exit /b 1
  )
)

REM Set up output directory structure
set _base_output_name=TableCraft.Editor-%_version_clean%-%_runtime%
if not "%_suffix%"=="" (
    set _base_output_name=!_base_output_name!_%_suffix%
)
set _release_dir=publish\%_version%
set _output_dir=!_release_dir!\!_base_output_name!

echo ===============================================
echo TableCraft Editor Build Script
echo Version: %_version%
echo Runtime: %_runtime%
echo Self-contained: %_self-contained%
echo Single file: %_publish_single_file%
echo Output directory: !_output_dir!
echo Copy config files: %_copy_config%
echo Launch after build: %_launch%
echo Create zip archive: %_zip%
echo Create installer: %_installer%
if not "%_suffix%"=="" echo Suffix: %_suffix%
echo ===============================================

REM Create release directory structure
if not exist "%_release_dir%" (
  echo Creating release directory: %_release_dir%
  mkdir "%_release_dir%"
)

echo Checking if output directory exists: !_output_dir!
if exist "!_output_dir!" (
  echo Output directory exists, cleaning contents...
  rmdir /s /q "!_output_dir!"
  echo Directory cleaned.
  ) else (
  echo Output directory does not exist, will be created.
)

echo Starting build process...
@echo on

dotnet publish .\TableCraft.Editor\TableCraft.Editor.csproj ^
--configuration Release ^
--runtime %_runtime% ^
--self-contained %_self-contained% ^
-p:PublishSingleFile=%_publish_single_file% ^
-p:DebugType=None ^
-p:DebugSymbols=false ^
--output "!_output_dir!"

@echo off
if %ERRORLEVEL% EQU 0 (
  echo ===============================================
  echo Build completed successfully!
  echo Output location: !_output_dir!
  echo ===============================================
  
  REM move configuration files from \Example to output directory
  if "%_copy_config%"=="true" (
    echo Copying test configuration files from Example...
    if exist "Example" (
      xcopy "Example\*" "!_output_dir!\" /E /Y /Q
      echo Configuration files copied successfully.
      
      echo Configuring appsettings.json with working paths...
      echo Replacing placeholder paths in appsettings.json...
      powershell -Command "& {" ^
      "$content = Get-Content '!_output_dir!\appsettings.json' -Raw;" ^
      "$outputDir = '!_output_dir!'.Replace('\','/');" ^
      "$configPath = (Resolve-Path $outputDir).Path + '\Config';" ^
      "$jsonPath = (Resolve-Path $outputDir).Path + '\ConfigJson';" ^
      "$exportPath = (Resolve-Path $outputDir).Path + '\GeneratedCode';" ^
      "$content = $content -replace '\[your-config-home-path\]', $configPath.Replace('\','\\');" ^
      "$content = $content -replace '\[your-json-home-path\]', $jsonPath.Replace('\','\\');" ^
      "$content = $content -replace '\[your-csharp-export-path\]', $exportPath.Replace('\','\\');" ^
      "Set-Content '!_output_dir!\appsettings.json' -Value $content" ^
      "}"
      echo appsettings.json configured successfully.
    ) else (
      echo Warning: Example directory not found, skipping configuration copy.
    )
  ) else (
    echo Skipping configuration files copy --no-copy-config specified.
  )
  
  REM Create zip archive if requested
  if "%_zip%"=="true" (
    echo.
    echo ===============================================
    echo Creating zip archive...
    echo ===============================================
    set "_zip_name=!_base_output_name!.zip"
    set "_zip_path=!_release_dir!\!_zip_name!"
    echo Archive name: !_zip_name!
    echo Archive path: !_zip_path!
    
    REM Remove existing zip file if it exists
    if exist "!_zip_path!" (
      echo Removing existing archive: !_zip_path!
      del "!_zip_path!"
    )
    
    REM Use 7z to create zip archive
    echo Creating zip archive using 7z...
    pushd "!_release_dir!"
    "%_7z_path%" a -tzip "!_zip_name!" "!_base_output_name!" -mx=9
    set "_7z_exitcode=!errorlevel!"
    popd
    
    if !_7z_exitcode! equ 0 (
      if exist "!_zip_path!" (
        echo Zip archive created successfully: !_zip_path!
      ) else (
        echo Error: Zip file not found after 7z command
      )
    ) else (
      echo Error: 7z command failed with exit code !_7z_exitcode!
    )
  )
  
  REM Create installer if requested
  if "%_installer%"=="true" (
    echo.
    echo ===============================================
    echo Creating installer using Inno Setup...
    echo ===============================================
    
    REM Set environment variables for Inno Setup script
    set "TABLECRAFT_VERSION=%_version_clean%"
    set "TABLECRAFT_OUTPUT_DIR=!_base_output_name!"
    set "TABLECRAFT_RELEASE_DIR=!_release_dir!"
    set "TABLECRAFT_RUNTIME=%_runtime%"
    set "TABLECRAFT_SUFFIX=%_suffix%"

    REM Use Inno Setup command line compiler
    set "_iss_script=TableCraft.Editor.iss"

    echo Compiling installer script: !_iss_script!
    "%_inno_compiler%" "!_iss_script!"
    set "_inno_exitcode=!errorlevel!"

    if !_inno_exitcode! equ 0 (
      echo Installer created successfully in: !_release_dir!
    ) else (
      echo Error: Installer compilation failed with exit code !_inno_exitcode!
    )
  )

  REM Launch application if requested
  if "%_launch%"=="true" (
    echo ===============================================
    echo Launching TableCraft.Editor.exe...
    echo ===============================================
    echo Changing directory to: !_output_dir!
    cd /d "!_output_dir!"
    if exist "TableCraft.Editor.exe" (
      echo Starting TableCraft.Editor.exe...
      start "" "TableCraft.Editor.exe"
      echo Application launched successfully.
    ) else (
      echo Error: TableCraft.Editor.exe not found in output directory.
    )
  )
  
) else (
  echo ===============================================
  echo Build failed with error code: %ERRORLEVEL%
  echo ===============================================
)

pause
