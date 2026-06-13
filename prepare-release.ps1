<#
.SYNOPSIS
    Performs the steps of `nbgv prepare-release` manually, with GPG-signed commits and tags.

.DESCRIPTION
    NerdBank.GitVersioning does not sign the commits it creates during `prepare-release`.
    This script replicates the same workflow transparently, signing every commit (-S)
    and tag (-s) it creates.

    Workflow (run from the repository root, on the source branch — `develop` by default):

      0. Safety checks (clean tree, correct branch, not behind the remote, etc.)
      1. Determine the release version from .\version.json:
           - release version  = current version with any prerelease suffix stripped
           - release branch   = version.json::release.branchName  (default 'release/v{version}')
           - next dev version = release version incremented per
                                version.json::release.versionIncrement (default 'patch'),
                                with '-<PrereleaseLabel>' appended
      2. Create the release branch from the current HEAD
      3. Update version.json on that branch (strip the prerelease suffix)
      4. Commit (signed) — skipped when stripping the suffix is a no-op
      5. Optionally create a signed annotated tag 'v{version}' on the release branch (-Tag)
      6. Push the release branch (and tag) to the remote
      7. Update version.json on the source branch to the next prerelease version
      8. Commit (signed)
      9. Push the source branch

    Idempotency: every step checks whether its outcome already exists and skips if so.
    If a previous run died after committing the develop bump but before pushing it, the
    script recognizes its own bump commit (by its exact message) and resumes by pushing
    the remaining refs. If it finds unpushed commits it did NOT create, it stops and
    tells you — it never guesses.

.PARAMETER Tag
    When set, creates (and pushes) a signed annotated tag 'v{releaseVersion}' pointing
    at the release branch tip. When omitted, no tag is created.

.PARAMETER Remote
    The remote to push to. Default: origin.

.PARAMETER SourceBranch
    The branch releases are cut from and that receives the next prerelease version.
    Default: develop. The script must be run with this branch checked out.

.PARAMETER PrereleaseLabel
    Prerelease label appended to the incremented version on the source branch
    (e.g. 'beta' -> 0.0.4-beta). Pass an empty string for no suffix. Default: beta.

.EXAMPLE
    .\prepare-release.ps1 -Tag
    Cuts release/v0.0.3, tags v0.0.3, pushes both, and moves develop to 0.0.4-beta.

.EXAMPLE
    .\prepare-release.ps1 -WhatIf
    Shows every git command and file edit that would run, without changing anything.
#>
#requires -Version 7.0
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch] $Tag,
    [string] $Remote = 'origin',
    [string] $SourceBranch = 'develop',
    [string] $PrereleaseLabel = 'beta'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$VersionJsonPath = Join-Path (Get-Location) 'version.json'
$PrereleaseLabel = $PrereleaseLabel.TrimStart('-')

# The exact develop-bump commit subject; also used to detect and resume a partial run.
$BumpCommitFormat = 'chore(release): bump version to {0} (release branch: {1})'
$BumpCommitPattern = '^chore\(release\): bump version to (?<version>\S+) \(release branch: (?<branch>\S+)\)$'

############################################################################
# Helpers

function Write-Step([string] $Message) { Write-Host "`n==> $Message" -ForegroundColor Cyan }
function Write-Skip([string] $Message) { Write-Host "    (skip) $Message" -ForegroundColor DarkGray }

function Invoke-Git {
    <#
      Runs git, echoing the full command so every step is visible.
      -Mutating commands are suppressed (and printed) under -WhatIf.
      -AllowFailure returns $null on a nonzero exit instead of throwing.
    #>
    param(
        [Parameter(Mandatory)] [string[]] $GitArgs,
        [switch] $Mutating,
        [switch] $AllowFailure
    )
    $display = "git $($GitArgs -join ' ')"
    if ($Mutating -and $WhatIfPreference) {
        Write-Host "    WhatIf: $display" -ForegroundColor Yellow
        return $null
    }
    Write-Host "    > $display" -ForegroundColor DarkGray
    $output = & git @GitArgs 2>&1 | ForEach-Object { "$_" }
    if ($LASTEXITCODE -ne 0) {
        if ($AllowFailure) { return $null }
        throw "Command failed (exit ${LASTEXITCODE}): $display`n$($output -join "`n")"
    }
    return $output
}

function Test-GitRef([string] $Ref) {
    return $null -ne (Invoke-Git @('rev-parse', '--verify', '--quiet', $Ref) -AllowFailure)
}

function Get-CommitSha([string] $Ref) {
    return (Invoke-Git @('rev-parse', "$Ref^{commit}")) | Select-Object -First 1
}

function Split-Version {
    # Splits '1.2.3-beta' into the numeric part and the prerelease suffix (without '-').
    param([Parameter(Mandatory)] [string] $Version)
    if ($Version -notmatch '^(?<numeric>\d+\.\d+(?:\.\d+)?(?:\.\d+)?)(?:-(?<prerelease>.+))?$') {
        throw "Unsupported version format in version.json: '$Version'"
    }
    return [pscustomobject]@{
        Numeric    = $Matches['numeric']
        Prerelease = if ($Matches.ContainsKey('prerelease')) { $Matches['prerelease'] } else { $null }
    }
}

function Get-IncrementedVersion {
    # NBGV semantics: bump the named component, reset everything below it to zero.
    param(
        [Parameter(Mandatory)] [string] $NumericVersion,
        [Parameter(Mandatory)] [string] $Increment
    )
    $parts = [System.Collections.Generic.List[int]]($NumericVersion.Split('.') | ForEach-Object { [int]$_ })
    switch ($Increment.ToLowerInvariant()) {
        'major' {
            $parts[0]++
            for ($i = 1; $i -lt $parts.Count; $i++) { $parts[$i] = 0 }
        }
        'minor' {
            $parts[1]++
            for ($i = 2; $i -lt $parts.Count; $i++) { $parts[$i] = 0 }
        }
        { $_ -in 'build', 'patch' } {
            while ($parts.Count -lt 3) { $parts.Add(0) }
            $parts[2]++
            for ($i = 3; $i -lt $parts.Count; $i++) { $parts[$i] = 0 }
        }
        default { throw "Unsupported release.versionIncrement '$Increment' (expected major, minor, build, or patch)." }
    }
    return $parts -join '.'
}

function Set-VersionJsonVersion {
    # Rewrites only the top-level "version" value, preserving all other formatting.
    param(
        [Parameter(Mandatory)] [string] $Path,
        [Parameter(Mandatory)] [string] $NewVersion
    )
    if ($WhatIfPreference) {
        Write-Host "    WhatIf: set ""version"": ""$NewVersion"" in $Path" -ForegroundColor Yellow
        return
    }
    $content = Get-Content -Path $Path -Raw
    $pattern = '(?m)^(\s*)"version"\s*:\s*"[^"]*"'
    $found = [regex]::Matches($content, $pattern)
    if ($found.Count -ne 1) {
        throw "Expected exactly one ""version"" property in $Path but found $($found.Count). Refusing to edit."
    }
    Write-Host "    edit: ""version"" -> ""$NewVersion"" in version.json" -ForegroundColor DarkGray
    $updated = [regex]::Replace($content, $pattern, ('${1}"version": "' + $NewVersion + '"'))
    Set-Content -Path $Path -Value $updated -NoNewline
}

function Get-BranchVersionRegex {
    # Turns the branchName template (e.g. 'release/v{version}') into a regex that
    # extracts the version back out of a concrete branch name.
    param([Parameter(Mandatory)] [string] $Template)
    return '^' + [regex]::Escape($Template).Replace('\{version}', '(?<version>.+)') + '$'
}

############################################################################
# Step 0 — Safety checks

Write-Step 'Step 0: Safety checks'

if (-not (Test-Path $VersionJsonPath)) {
    throw "No version.json found in the current directory ($(Get-Location)). Run this from the repository root."
}

$null = Invoke-Git @('rev-parse', '--show-toplevel')

$currentBranch = (Invoke-Git @('branch', '--show-current')) | Select-Object -First 1
if ($currentBranch -ne $SourceBranch) {
    throw "Current branch is '$currentBranch' but releases are prepared from '$SourceBranch'. Run: git switch $SourceBranch"
}

$dirty = Invoke-Git @('status', '--porcelain')
if ($dirty) {
    throw "Working tree is not clean. Commit, stash, or discard these changes first:`n$($dirty -join "`n")"
}

$signingKey = (Invoke-Git @('config', '--get', 'user.signingkey') -AllowFailure)
if (-not $signingKey) {
    Write-Warning 'git config user.signingkey is not set. Signing will rely on your default gpg/ssh key and may fail.'
}

$null = Invoke-Git @('fetch', $Remote, '--tags', '--prune')

$remoteSourceRef = "$Remote/$SourceBranch"
if (-not (Test-GitRef "refs/remotes/$remoteSourceRef")) {
    throw "Remote branch '$remoteSourceRef' not found after fetch."
}

# Local must contain everything the remote has (being ahead is handled below; behind/diverged is not).
$null = Invoke-Git @('merge-base', '--is-ancestor', $remoteSourceRef, 'HEAD') -AllowFailure
if ($LASTEXITCODE -ne 0) {
    throw "'$SourceBranch' is behind (or has diverged from) '$remoteSourceRef'. Pull/rebase first."
}

############################################################################
# Resume detection — a previous run may have committed the develop bump but failed
# to push. In that state version.json already shows the NEXT cycle's version, so
# recomputing from it would start a brand-new release. Detect our own bump commit
# and finish the pushes instead.

$aheadCount = [int]((Invoke-Git @('rev-list', '--count', "$remoteSourceRef..HEAD")) | Select-Object -First 1)
if ($aheadCount -gt 0) {
    $headSubject = (Invoke-Git @('log', '-1', '--format=%s', 'HEAD')) | Select-Object -First 1
    if ($headSubject -notmatch $BumpCommitPattern) {
        throw ("'$SourceBranch' has $aheadCount unpushed commit(s) that this script did not create " +
               "(HEAD: '$headSubject'). Push or reconcile them first, then re-run.")
    }

    $resumeBranch = $Matches['branch']
    Write-Step "Resuming a partial run: HEAD is this script's bump commit for '$resumeBranch'"
    if (-not (Test-GitRef "refs/heads/$resumeBranch")) {
        throw "Bump commit references release branch '$resumeBranch', but that branch does not exist locally. Reconcile manually."
    }

    $versionJson = Get-Content -Path $VersionJsonPath -Raw | ConvertFrom-Json
    $branchTemplate = if ($versionJson.PSObject.Properties['release'] -and $versionJson.release.PSObject.Properties['branchName']) {
        $versionJson.release.branchName
    } else { 'release/v{version}' }
    if ($resumeBranch -notmatch (Get-BranchVersionRegex $branchTemplate)) {
        throw "Release branch '$resumeBranch' does not match the branchName template '$branchTemplate'. Reconcile manually."
    }
    $resumeTag = "v$($Matches['version'])"

    Write-Step "Pushing release branch '$resumeBranch'"
    $null = Invoke-Git @('push', $Remote, $resumeBranch) -Mutating

    if (Test-GitRef "refs/tags/$resumeTag") {
        Write-Step "Pushing tag '$resumeTag'"
        $null = Invoke-Git @('push', $Remote, $resumeTag) -Mutating
    }

    Write-Step "Pushing '$SourceBranch'"
    $null = Invoke-Git @('push', $Remote, $SourceBranch) -Mutating

    Write-Host "`nResume complete. All refs pushed to '$Remote'." -ForegroundColor Green
    return
}

############################################################################
# Step 1 — Determine versions

Write-Step 'Step 1: Determine release and next-prerelease versions from version.json'

$versionJson = Get-Content -Path $VersionJsonPath -Raw | ConvertFrom-Json
$currentVersion = $versionJson.version

$increment = 'patch'
$branchTemplate = 'release/v{version}'
if ($versionJson.PSObject.Properties['release']) {
    if ($versionJson.release.PSObject.Properties['versionIncrement']) { $increment = $versionJson.release.versionIncrement }
    if ($versionJson.release.PSObject.Properties['branchName'])       { $branchTemplate = $versionJson.release.branchName }
}

$split = Split-Version $currentVersion
$releaseVersion = $split.Numeric
$releaseBranch = $branchTemplate.Replace('{version}', $releaseVersion)
$tagName = "v$releaseVersion"

$nextNumeric = Get-IncrementedVersion -NumericVersion $releaseVersion -Increment $increment
$nextDevVersion = if ($PrereleaseLabel) { "$nextNumeric-$PrereleaseLabel" } else { $nextNumeric }

Write-Host @"

    Current version        : $currentVersion
    Release version        : $releaseVersion
    Release branch         : $releaseBranch
    Tag                    : $(if ($Tag) { $tagName } else { '(none — pass -Tag to create one)' })
    Next $SourceBranch version : $nextDevVersion  (increment: $increment, label: $(if ($PrereleaseLabel) { $PrereleaseLabel } else { 'none' }))
    Remote                 : $Remote
"@

$releaseBranchExistsLocally = Test-GitRef "refs/heads/$releaseBranch"
$releaseBranchExistsRemotely = Test-GitRef "refs/remotes/$Remote/$releaseBranch"
$tagExists = Test-GitRef "refs/tags/$tagName"

if ($tagExists -and -not ($releaseBranchExistsLocally -or $releaseBranchExistsRemotely)) {
    throw "Tag '$tagName' already exists but release branch '$releaseBranch' does not. Version $releaseVersion appears to be already released — bump version.json on '$SourceBranch' first."
}

############################################################################
# Steps 2–4 — Create the release branch, update version.json on it, commit (signed)

Write-Step "Steps 2-4: Release branch '$releaseBranch'"

if ($releaseBranchExistsLocally -or $releaseBranchExistsRemotely) {
    # Idempotency: the branch already exists — verify it carries the expected version, then skip.
    $existingRef = if ($releaseBranchExistsLocally) { $releaseBranch } else { "$Remote/$releaseBranch" }
    $branchVersionJson = (Invoke-Git @('show', "${existingRef}:version.json")) -join "`n" | ConvertFrom-Json
    if ($branchVersionJson.version -ne $releaseVersion) {
        throw ("Release branch '$existingRef' already exists but its version.json says " +
               "'$($branchVersionJson.version)' instead of '$releaseVersion'. " +
               "If it is a leftover from a failed run, delete it (git branch -D $releaseBranch) and re-run.")
    }
    Write-Skip "release branch '$existingRef' already exists with version $releaseVersion"
    if ($releaseBranchExistsRemotely -and -not $releaseBranchExistsLocally) {
        $null = Invoke-Git @('branch', '--track', $releaseBranch, "$Remote/$releaseBranch") -Mutating
    }
}
else {
    $null = Invoke-Git @('branch', $releaseBranch, 'HEAD') -Mutating

    if ($currentVersion -eq $releaseVersion) {
        Write-Skip "version.json already reads '$releaseVersion' (no prerelease suffix to strip) — no commit needed on the release branch"
    }
    else {
        # Edit + commit on the release branch, always returning to the source branch.
        $null = Invoke-Git @('switch', $releaseBranch) -Mutating
        try {
            Set-VersionJsonVersion -Path $VersionJsonPath -NewVersion $releaseVersion
            $null = Invoke-Git @('add', 'version.json') -Mutating
            $null = Invoke-Git @('commit', '-S', '-m', "chore(release): set version to $releaseVersion") -Mutating
        }
        finally {
            $null = Invoke-Git @('switch', $SourceBranch) -Mutating
        }
    }
}

############################################################################
# Step 5 — Optionally tag the release (signed)

if ($Tag) {
    Write-Step "Step 5: Signed tag '$tagName'"
    if (-not $WhatIfPreference -or (Test-GitRef "refs/heads/$releaseBranch")) {
        $releaseTip = Get-CommitSha $releaseBranch
        if ($tagExists) {
            $tagTarget = Get-CommitSha $tagName
            if ($tagTarget -ne $releaseTip) {
                throw "Tag '$tagName' already exists but points at $tagTarget, not the release branch tip $releaseTip. Resolve manually."
            }
            Write-Skip "tag '$tagName' already exists at the release branch tip"
        }
        else {
            $null = Invoke-Git @('tag', '-s', '-m', "Release $tagName", $tagName, $releaseBranch) -Mutating
        }
    }
    else {
        Write-Host "    WhatIf: git tag -s -m ""Release $tagName"" $tagName $releaseBranch" -ForegroundColor Yellow
    }
}
else {
    Write-Step 'Step 5: Tagging skipped (-Tag not specified)'
}

############################################################################
# Step 6 — Push the release branch (and tag)

Write-Step "Step 6: Push '$releaseBranch'$(if ($Tag) { " and '$tagName'" }) to '$Remote'"
$null = Invoke-Git @('push', $Remote, $releaseBranch) -Mutating
if ($Tag) {
    $null = Invoke-Git @('push', $Remote, $tagName) -Mutating
}

############################################################################
# Steps 7–8 — Move the source branch to the next prerelease version, commit (signed)

Write-Step "Steps 7-8: Move '$SourceBranch' to $nextDevVersion"

if ($currentVersion -eq $nextDevVersion) {
    Write-Skip "version.json on '$SourceBranch' already reads '$nextDevVersion'"
}
else {
    Set-VersionJsonVersion -Path $VersionJsonPath -NewVersion $nextDevVersion
    $null = Invoke-Git @('add', 'version.json') -Mutating
    $null = Invoke-Git @('commit', '-S', '-m', ($BumpCommitFormat -f $nextDevVersion, $releaseBranch)) -Mutating
}

############################################################################
# Step 9 — Push the source branch

Write-Step "Step 9: Push '$SourceBranch' to '$Remote'"
$null = Invoke-Git @('push', $Remote, $SourceBranch) -Mutating

Write-Host "`nDone." -ForegroundColor Green
Write-Host "  Release branch : $releaseBranch  (version $releaseVersion)"
if ($Tag) { Write-Host "  Tag            : $tagName" }
Write-Host "  $SourceBranch        : $nextDevVersion"
