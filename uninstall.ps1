# FolderTools Uninstallation Script
# Usage: .\uninstall.ps1

param(
    [switch]$Force,
    [string]$InstallPath = "$env:LOCALAPPDATA\FolderTools"
)

$ErrorActionPreference = "Stop"

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Remove-FromPath {
    param([string]$Path)

    Write-ColorOutput "Removing from PATH..." "Cyan"

    # Get current user PATH
    $currentUserPath = [Environment]::GetEnvironmentVariable("Path", "User")

    if (-not $currentUserPath) {
        Write-ColorOutput "  PATH is empty, nothing to remove" "Yellow"
        return $true
    }

    # Check if in PATH
    $pathEntries = $currentUserPath -split ";"
    $inPath = $pathEntries | Where-Object { $_ -eq $Path }

    if (-not $inPath) {
        Write-ColorOutput "  Not found in PATH" "Yellow"
        return $true
    }

    # Remove from PATH
    $newPath = ($pathEntries | Where-Object { $_ -ne $Path }) -join ";"

    try {
        [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
        Write-ColorOutput "  ✓ Removed from user PATH" "Green"
        Write-ColorOutput "  Note: Restart your terminal for PATH changes to take effect" "Yellow"
        return $true
    }
    catch {
        Write-ColorOutput "  ✗ Failed to update PATH: $_" "Red"
        return $false
    }
}

function Remove-InstallationDirectory {
    param([string]$Path)

    Write-ColorOutput "Removing installation directory..." "Cyan"

    if (-not (Test-Path $Path)) {
        Write-ColorOutput "  Directory not found: $Path" "Yellow"
        return $true
    }

    # Show what will be deleted
    $items = Get-ChildItem -Path $Path -Recurse
    $size = ($items | Measure-Object -Property Length -Sum).Sum / 1KB
    $count = $items.Count

    Write-ColorOutput "  Found $count items ($([math]::Round($size, 2)) KB)" "Gray"

    if (-not $Force) {
        $response = Read-Host "Delete directory '$Path'? (y/N)"
        if ($response -ne "y" -and $response -ne "Y") {
            Write-ColorOutput "  Skipped directory removal" "Yellow"
            return $false
        }
    }

    try {
        Remove-Item -Path $Path -Recurse -Force
        Write-ColorOutput "  ✓ Removed installation directory" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "  ✗ Failed to remove directory: $_" "Red"
        Write-ColorOutput "  You may need to run as Administrator or close FolderTools first" "Yellow"
        return $false
    }
}

function Show-UninstallSummary {
    Write-ColorOutput "`n=== Uninstallation Complete ===" "Green"
    Write-ColorOutput "`nFolderTools has been removed from your system." "Cyan"
    Write-ColorOutput "`nTo finish cleanup:" "Yellow"
    Write-ColorOutput "  1. Restart your terminal for PATH changes to take effect" "White"
    Write-ColorOutput "  2. If you added any aliases manually, remove them from your profile" "White"
    Write-ColorOutput "`nThank you for using FolderTools!" "Cyan"
}

# Main execution
try {
    Write-ColorOutput "`n=== FolderTools Uninstallation ===" "Cyan"
    Write-ColorOutput "Install path: $InstallPath" "Gray"

    # Verify installation exists
    if (-not (Test-Path "$InstallPath\FolderTools.exe")) {
        Write-ColorOutput "`nFolderTools does not appear to be installed at: $InstallPath" "Yellow"

        if (-not $Force) {
            $response = Read-Host "Continue anyway? (y/N)"
            if ($response -ne "y" -and $response -ne "Y") {
                exit 0
            }
        }
    }

    # Confirm uninstallation
    if (-not $Force) {
        Write-ColorOutput "`nThis will:" "Yellow"
        Write-ColorOutput "  - Remove FolderTools from your PATH" "White"
        Write-ColorOutput "  - Delete the installation directory at $InstallPath" "White"
        $response = Read-Host "`nProceed with uninstallation? (y/N)"
        if ($response -ne "y" -and $response -ne "Y") {
            Write-ColorOutput "Uninstallation cancelled." "Yellow"
            exit 0
        }
    }

    # Remove from PATH
    $pathRemoved = Remove-FromPath -Path $InstallPath

    # Remove installation directory
    $dirRemoved = Remove-InstallationDirectory -Path $InstallPath

    # Show summary
    if ($pathRemoved -or $dirRemoved) {
        Show-UninstallSummary
    }
    else {
        Write-ColorOutput "`nUninstallation completed with issues. Check messages above." "Yellow"
        exit 1
    }
}
catch {
    Write-ColorOutput "`nUninstallation failed: $_" "Red"
    exit 1
}
