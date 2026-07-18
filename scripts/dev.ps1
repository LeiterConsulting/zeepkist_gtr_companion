[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [switch]$Install,

    [switch]$Package,

    [string]$ZeepkistPath = $env:ZEEPKIST_PATH
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $repositoryRoot "Zeepkist.GTR.Companion.sln"
$projectOutput = Join-Path $repositoryRoot "src\Zeepkist.GTR.Companion\bin\$Configuration\net472"
$assemblyName = "com.leiterconsulting.zeepkist.gtrcompanion"
$pluginAssembly = Join-Path $projectOutput "$assemblyName.dll"

function Invoke-DotNet {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet failed with exit code $LASTEXITCODE."
    }
}

function Resolve-ZeepkistDirectory {
    param([string]$RequestedPath)

    $candidates = @(
        $RequestedPath,
        "${env:ProgramFiles(x86)}\Steam\steamapps\common\Zeepkist",
        "$env:ProgramFiles\Steam\steamapps\common\Zeepkist"
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($candidate in $candidates) {
        $resolved = [Environment]::ExpandEnvironmentVariables($candidate)
        if (Test-Path -LiteralPath (Join-Path $resolved "BepInEx")) {
            return (Resolve-Path -LiteralPath $resolved).Path
        }
    }

    throw "Could not find a Zeepkist installation with BepInEx. Pass -ZeepkistPath or set ZEEPKIST_PATH."
}

Push-Location $repositoryRoot
try {
    Invoke-DotNet restore $solution
    Invoke-DotNet build $solution --configuration $Configuration --no-restore

    if (-not (Test-Path -LiteralPath $pluginAssembly)) {
        throw "Expected plugin assembly was not produced: $pluginAssembly"
    }

    Write-Host "Built $pluginAssembly" -ForegroundColor Green

    if ($Install) {
        $gameDirectory = Resolve-ZeepkistDirectory $ZeepkistPath
        $destination = Join-Path $gameDirectory "BepInEx\plugins\Sideloaded\Plugins\ZeepkistGTRCompanion"
        New-Item -ItemType Directory -Force -Path $destination | Out-Null
        Copy-Item -LiteralPath $pluginAssembly -Destination $destination -Force

        $pluginSymbols = Join-Path $projectOutput "$assemblyName.pdb"
        if (Test-Path -LiteralPath $pluginSymbols) {
            Copy-Item -LiteralPath $pluginSymbols -Destination $destination -Force
        }

        Write-Host "Installed development build to $destination" -ForegroundColor Green
    }

    if ($Package) {
        $artifactsDirectory = Join-Path $repositoryRoot "artifacts"
        $stagingDirectory = Join-Path $artifactsDirectory "ZeepkistGTRCompanion"
        $packagePath = Join-Path $artifactsDirectory "$assemblyName-$Configuration.zip"

        Remove-Item -LiteralPath $stagingDirectory -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $packagePath -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $stagingDirectory | Out-Null
        Copy-Item -LiteralPath $pluginAssembly -Destination $stagingDirectory
        Compress-Archive -Path (Join-Path $stagingDirectory "*") -DestinationPath $packagePath

        Write-Host "Created Modkist sideload package at $packagePath" -ForegroundColor Green
    }
}
finally {
    Pop-Location
}
