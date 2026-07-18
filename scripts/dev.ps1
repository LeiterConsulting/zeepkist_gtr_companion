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
$pluginProject = Join-Path $repositoryRoot "src\Zeepkist.GTR.Companion\Zeepkist.GTR.Companion.csproj"
$projectOutput = Join-Path $repositoryRoot "src\Zeepkist.GTR.Companion\bin\$Configuration\net472"
$hubOutput = Join-Path $repositoryRoot "src\Zeepkist.GTR.Companion.Hub\bin\$Configuration\net8.0-windows"
$assemblyName = "com.leiterconsulting.zeepkist.gtrcompanion"
$pluginAssembly = Join-Path $projectOutput "$assemblyName.dll"
$protocolAssemblyName = "LeiterConsulting.Zeepkist.GtrCompanion.Protocol"
$protocolAssembly = Join-Path $projectOutput "$protocolAssemblyName.dll"
$hubExecutable = Join-Path $hubOutput "Zeepkist.GTR.Companion.Hub.exe"

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
    $buildTarget = if ($Install -or $Package) { $pluginProject } else { $solution }
    Invoke-DotNet restore $buildTarget
    Invoke-DotNet build $buildTarget --configuration $Configuration --no-restore

    if (-not (Test-Path -LiteralPath $pluginAssembly)) {
        throw "Expected plugin assembly was not produced: $pluginAssembly"
    }

    if (-not (Test-Path -LiteralPath $protocolAssembly)) {
        throw "Expected protocol assembly was not produced: $protocolAssembly"
    }

    if (-not $Install -and -not $Package) {
        if (-not (Test-Path -LiteralPath $hubExecutable)) {
            throw "Expected companion hub was not produced: $hubExecutable"
        }

        Write-Host "Built $hubExecutable" -ForegroundColor Green
    }

    Write-Host "Built $pluginAssembly" -ForegroundColor Green

    if ($Install) {
        $gameDirectory = Resolve-ZeepkistDirectory $ZeepkistPath
        $destination = Join-Path $gameDirectory "BepInEx\plugins\ZeepkistGTRCompanion"
        New-Item -ItemType Directory -Force -Path $destination | Out-Null
        Copy-Item -LiteralPath $pluginAssembly -Destination $destination -Force
        Copy-Item -LiteralPath $protocolAssembly -Destination $destination -Force

        $pluginSymbols = Join-Path $projectOutput "$assemblyName.pdb"
        if (Test-Path -LiteralPath $pluginSymbols) {
            Copy-Item -LiteralPath $pluginSymbols -Destination $destination -Force
        }

        $protocolSymbols = Join-Path $projectOutput "$protocolAssemblyName.pdb"
        if (Test-Path -LiteralPath $protocolSymbols) {
            Copy-Item -LiteralPath $protocolSymbols -Destination $destination -Force
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
        Copy-Item -LiteralPath $protocolAssembly -Destination $stagingDirectory
        Compress-Archive -Path (Join-Path $stagingDirectory "*") -DestinationPath $packagePath

        Write-Host "Created Modkist sideload package at $packagePath" -ForegroundColor Green
    }
}
finally {
    Pop-Location
}
