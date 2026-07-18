[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "This command must be run inside the repository."
}

Push-Location $repositoryRoot
try {
    $paths = & git ls-files --cached --others --exclude-standard
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to enumerate repository files."
    }

    $blockedPathPattern = '(^|/)(ios|android|(private-)?ios-app|(private-)?android-app|\.private|\.local|\.scratch|dev-tests|local-tests|notes|research|scratch|conversations|transcripts|prompts|secrets|captures|dumps)(/|$)'
    $blockedFilePattern = '(\.xcodeproj(/|$)|\.xcworkspace(/|$)|\.xcuserstate$|\.mobileprovision$|\.keystore$|\.jks$|(^|/)(key|local)\.properties$|(^|/)google-services\.json$|(^|/)google-service-info\.plist$|(^|/)\.env($|\.)|\.p12$|\.pfx$|\.pem$|\.key$|secrets[^/]*\.json$)'
    $blockedPaths = @()

    foreach ($path in $paths) {
        $normalizedPath = $path.Replace('\', '/').ToLowerInvariant()
        if ($normalizedPath -match $blockedPathPattern -or $normalizedPath -match $blockedFilePattern) {
            $blockedPaths += $path
        }
    }

    if ($blockedPaths.Count -gt 0) {
        $blockedPaths | ForEach-Object { Write-Error "Blocked public repository path: $_" }
        throw "Public repository boundary check failed."
    }

    Write-Host "Public repository boundary check passed." -ForegroundColor Green
}
finally {
    Pop-Location
}
