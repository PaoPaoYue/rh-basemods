@echo off
setlocal enabledelayedexpansion

:: Set target directory
set "TARGET_DIR=F:\Steam Games\steamapps\common\RouletteHero\Mod\ModDebug"
set "SOURCE_DIR=%cd%"

echo SOURCE_DIR=%SOURCE_DIR%
echo TARGET_DIR=%TARGET_DIR%

:: Create target directory if it doesn't exist
if not exist "%TARGET_DIR%" (
    mkdir "%TARGET_DIR%"
)

:: Loop through all folders in current directory
for /d %%F in (*) do (
    set "FOLDER=%%F"
    set "SUFFIX=!FOLDER:~-3!"
    if /I "!SUFFIX!"=="Mod" (
        echo Copying folder: %%F
        xcopy "%SOURCE_DIR%\%%F" "%TARGET_DIR%\%%F" /E /I /Y
    )
)

echo Copy complete.
pause