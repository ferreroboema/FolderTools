# FolderTools Installation Script
# Run: irm https://raw.githubusercontent.com/ferreroboema/FolderTools/main/install.ps1 | iex
# Or: .\install.ps1 (for local installation)

param(
    [switch]$Force,
    [switch]$Local,
    [string]$InstallPath = "$env:LOCALAPPDATA\FolderTools",
    [string]$Version = "latest"
)

$ErrorActionPreference = "Stop"
$RepoUrl = "https://github.com/ferreroboema/FolderTools"
$ApiUrl = "https://api.github.com/repos/ferreroboema/FolderTools/releases"

function Write-ColorOutput($Message, $Color = "White") {
    Write-Host $Message -ForegroundColor $Color
}

function Get-LatestReleaseUrl {
    Write-ColorOutput "Fetching latest release information..." "Cyan"

    try {
        $releaseInfo = Invoke-RestMethod -Uri "$ApiUrl/latest" -Headers @{
            "User-Agent" = "FolderTools-Installer"
        }
        $version = $releaseInfo.tag_name
        $asset = $releaseInfo.assets | Where-Object { $_.name -eq "FolderTools.exe" } | Select-Object -First 1

        if ($asset) {
            return @{
                Version = $version
                Url = $asset.browser_download_url
            }
        }

        # Fallback: construct URL manually
        return @{
            Version = $version
            Url = "$RepoUrl/releases/download/$version/FolderTools.exe"
        }
    }
    catch {
        Write-ColorOutput "Failed to fetch release info: $_" "Red"
        throw
    }
}

function Download-FolderTools {
    $release = Get-LatestReleaseUrl
    $outputPath = "$PSScriptRoot\FolderTools.exe"

    Write-ColorOutput "Downloading FolderTools $($release.Version)..." "Cyan"
    Write-ColorOutput "  From: $($release.Url)" "Gray"

    try {
        Invoke-WebRequest -Uri $release.Url -OutFile $outputPath -UseBasicParsing
        Write-ColorOutput "  Downloaded successfully!" "Green"
        return $outputPath
    }
    catch {
        Write-ColorOutput "Download failed: $_" "Red"
        throw
    }
}

function Test-Installation($Path) {
    Write-ColorOutput "`nVerifying installation..." "Cyan"

    # Test if FolderTools is accessible
    $testResult = & "$Path\FolderTools.exe" --help 2>&1

    if (($LASTEXITCODE -eq 0) -or ($testResult -match "Usage|help|FolderTools")) {
        Write-ColorOutput "  FolderTools is working correctly!" "Green"
        return $true
    }
    else {
        Write-ColorOutput "  Verification failed: $testResult" "Red"
        return $false
    }
}

function Add-ToPath($Path) {
    Write-ColorOutput "`nAdding to PATH..." "Cyan"

    # Get current user PATH
    $currentUserPath = [Environment]::GetEnvironmentVariable("Path", "User")

    # Check if already in PATH
    if ($currentUserPath -split ";" | Where-Object { $_ -eq $InstallPath }) {
        Write-ColorOutput "  Already in PATH" "Yellow"
        return $true
    }

    # Add to PATH
    $newPath = if ($currentUserPath) {
        "$InstallPath;$currentUserPath"
    }
    else {
        $InstallPath
    }

    try {
        [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
        Write-ColorOutput "  Added to user PATH successfully" "Green"
        Write-ColorOutput "  Note: Restart your terminal for PATH changes to take effect" "Yellow"
        return $true
    }
    catch {
        Write-ColorOutput "  Failed to add to PATH: $_" "Red"
        Write-ColorOutput "  You can manually add '$InstallPath' to your PATH" "Yellow"
        return $false
    }
}

function Install-FolderTools($SourcePath) {
    Write-ColorOutput "`n=== FolderTools Installation ===" "Cyan"
    Write-ColorOutput "Install path: $InstallPath" "Gray"

    # Check if already installed
    $alreadyInstalled = Test-Path "$InstallPath\FolderTools.exe"
    if ($alreadyInstalled -and (-not $Force)) {
        Write-ColorOutput "`nFolderTools is already installed at: $InstallPath" "Yellow"
        $response = Read-Host "Reinstall? (y/N)"
        if (($response -ne "y") -and ($response -ne "Y")) {
            return
        }
    }

    # Create installation directory
    if (-not (Test-Path $InstallPath)) {
        New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
        Write-ColorOutput "  Created installation directory" "Green"
    }

    # Copy executable
    Write-ColorOutput "`nCopying files..." "Cyan"
    Copy-Item -Path $SourcePath -Destination "$InstallPath\FolderTools.exe" -Force
    Write-ColorOutput "  Installed FolderTools.exe" "Green"

    # Create batch wrapper for convenience
    $batchContent = "@echo off`n`"$InstallPath\FolderTools.exe`" %*"
    Set-Content -Path "$InstallPath\FolderTools.bat" -Value $batchContent
    Write-ColorOutput "  Created FolderTools.bat wrapper" "Green"

    # Add to PATH
    $pathAdded = Add-ToPath -Path $InstallPath

    # Create uninstall script
    $uninstallScript = @"
# FolderTools Uninstallation Script
`$InstallPath = "$InstallPath"

Write-Host "Removing FolderTools from `" + `$InstallPath + "..." -ForegroundColor Cyan

# Remove from PATH
`$currentUserPath = [Environment]::GetEnvironmentVariable("Path", "User")
`$newPath = (`$currentUserPath -split ";" | Where-Object { `$`_ -ne `$InstallPath }) -join ";"
[Environment]::SetEnvironmentVariable("Path", `$newPath, "User")

# Remove installation directory
if (Test-Path `$InstallPath) {
    Remove-Item -Path `$InstallPath -Recurse -Force
    Write-Host "Removed installation directory" -ForegroundColor Green
}

Write-Host "`nFolderTools has been uninstalled." -ForegroundColor Green
Write-Host "Please restart your terminal for PATH changes to take effect." -ForegroundColor Yellow
"@
    Set-Content -Path "$InstallPath\uninstall.ps1" -Value $uninstallScript
    Write-ColorOutput "  Created uninstall.ps1" "Green"

    # Test installation
    $success = Test-Installation -Path $InstallPath

    # Print summary
    Write-ColorOutput "`n=== Installation Complete ===" "Green"
    Write-ColorOutput "Location: $InstallPath" "Gray"
    Write-ColorOutput "Executable: $InstallPath\FolderTools.exe" "Gray"

    if ($pathAdded) {
        Write-ColorOutput "`nPATH updated!" "Yellow"
        Write-ColorOutput "Restart your terminal and run:" "Cyan"
        Write-ColorOutput "  FolderTools --help" "White"
        Write-ColorOutput "`nOr without restarting (current session only):" "Yellow"
        Write-ColorOutput '  $env:Path = [Environment]::GetEnvironmentVariable("Path", "User")' "Gray"
    }
    else {
        Write-ColorOutput "`nTo use FolderTools from anywhere, add this to your PATH:" "Yellow"
        Write-ColorOutput "  $InstallPath" "White"
    }

    Write-ColorOutput "`nTo uninstall later, run:" "Yellow"
    Write-ColorOutput "  & `"$InstallPath\uninstall.ps1`"" "Gray"
}

function Install-Local {
    # Check for local executable
    $localExe = "$PSScriptRoot\FolderTools\bin\Release\FolderTools.exe"
    $altExe = "$PSScriptRoot\FolderTools.exe"

    $sourcePath = if (Test-Path $localExe) { $localExe }
    elseif (Test-Path $altExe) { $altExe }
    else {
        Write-ColorOutput "Could not find FolderTools.exe in current directory." "Red"
        Write-ColorOutput "Build it first with: dotnet build -c Release" "Yellow"
        exit 1
    }

    Install-FolderTools -SourcePath $sourcePath
}

function Install-Remote {
    # Download to temp and install
    $tempExe = Download-FolderTools
    try {
        Install-FolderTools -SourcePath $tempExe
    }
    finally {
        # Clean up download
        if (Test-Path $tempExe) {
            Remove-Item $tempExe -Force
        }
    }
}

# Main execution
try {
    if ($Local) {
        Install-Local
    }
    else {
        Install-Remote
    }
}
catch {
    Write-ColorOutput "`nInstallation failed: $_" "Red"
    exit 1
}
