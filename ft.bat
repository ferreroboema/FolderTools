@echo off
REM FolderTools Short Alias (ft)
REM Convenient shorthand for FolderTools command
REM Usage: ft "search" "replace" "C:\path" [options]

setlocal
set "SCRIPT_DIR=%~dp0"

REM Forward all arguments to FolderTools.exe
"%SCRIPT_DIR%FolderTools.exe" %*
