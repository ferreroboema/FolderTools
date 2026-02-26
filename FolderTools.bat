@echo off
REM FolderTools Batch Wrapper
REM This allows running FolderTools without the .exe extension
REM Place this file in the same directory as FolderTools.exe

REM Get the directory where this batch file is located
setlocal
set "SCRIPT_DIR=%~dp0"

REM Check if FolderTools.exe exists
if not exist "%SCRIPT_DIR%FolderTools.exe" (
    echo Error: FolderTools.exe not found in: %SCRIPT_DIR%
    echo Please ensure FolderTools.exe is in the same directory as this batch file.
    exit /b 1
)

REM Forward all arguments to FolderTools.exe
"%SCRIPT_DIR%FolderTools.exe" %*
