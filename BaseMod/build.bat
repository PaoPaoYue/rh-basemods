@echo off
setlocal enabledelayedexpansion

:: Get the current folder name
for %%I in ("%cd%") do set "CURRENT_NAME=%%~nxI"

:: Set the target folder path (relative to parent artifacts folder)
set "TARGET_DIR=..\artifacts\%CURRENT_NAME%\plugins"

:: Set the libs folder path (relative to examples folder)
set "LIBS_DIR=..\examples\libs"

:: Build the project using dotnet CLI
echo Building the project...
dotnet build
if errorlevel 1 (
    echo Build failed. Exiting script.
    exit /b 1
)

:: Create the target folder if it doesn't exist
if not exist "%TARGET_DIR%" (
    echo Creating target folder: %TARGET_DIR%
    mkdir "%TARGET_DIR%"
)

:: Copy all DLLs from the build output to the target folder
echo Copying DLLs to %TARGET_DIR% ...
xcopy "bin\Debug\netstandard2.1\*.dll" "%TARGET_DIR%\" /Y /I

:: Copy all PDBs from the build output to the examples/libs folder
echo Copying DLLs to %LIBS_DIR% ...
xcopy "bin\Debug\netstandard2.1\*.dll" "%LIBS_DIR%\" /Y /I

echo Done.
pause
