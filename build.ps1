param(
    [switch] $NoTests,
    [switch] $Release,
    [switch] $DryRun,
    [switch] $Publish,
    [switch] $ToolingOnly,
    [switch] $PreserveArtifacts,
    [string] $ArtifactsBasePath = ".\artifacts",
    [string] $LocalPackagesPath = ".\LocalPackages",
    [string] $NuGetSource = "https://git.dkw.io/api/packages/tworiversit/nuget/index.json",
    [string] $NuGetApiKeyName = "GITEA_PERSONAL_ACCESS_TOKEN"
)

$BuildLogPath = Join-Path $PSScriptRoot "build.log"
$script:TranscriptStarted = $false

Start-Transcript -Path $BuildLogPath -Force | Out-Null
$script:TranscriptStarted = $true

############################################################################
# Begin Utility Functions
function Exit-BuildScript {
    param(
        [int] $Code = 0,
        [string] $Message = "Unknown error occurred."
    )

    Write-Host $Message -ForegroundColor Red

    if ($script:TranscriptStarted) {
        Stop-Transcript | Out-Null
        $script:TranscriptStarted = $false
    }

    exit $Code
}

#-----------------------------
# Get version metadata from NerdBank.GitVersioning (nbgv)
#-----------------------------
function Get-Version {
    $VersionJson = nbgv get-version --format json
    
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($VersionJson)) {
        Exit-BuildScript -Code 1 -Message "Failed to retrieve version metadata from: 'nbgv get-version --format json'"
    }
    return $VersionJson | ConvertFrom-Json
}

function Remove-BuildArtifacts {
    param([string] $Path)   
    Get-ChildItem -Path $Path -Directory -Recurse |
    Where-Object {
        ($_.Name -eq 'bin' -or $_.Name -eq 'obj') -and
        ($_.FullName -notmatch '\\node_bundles\\')
    } |
    Remove-Item -Recurse -Force
}

function Remove-LocalPackages {
    param (
        [Parameter(Mandatory = $true)]
        [string] $Path
    )
    # Delete and recreate the LocalPackages directory to ensure a clean slate for packing.
    if (Test-Path $Path) {
        Remove-Item -Force -Recurse -Path $Path -ErrorAction Stop
    }
    New-Item -ItemType Directory -Path $Path | Out-Null
}

function Move-Artifacts {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [String]
        $SourcePath,
        [Parameter(Mandatory = $true)]
        [String]
        $TargetPath
    )
    try {
        # Check if source exists
        if (-Not (Test-Path -Path $SourcePath -PathType Container)) {
            Exit-BuildScript -Code 1 -Message "Source directory does not exist: $SourcePath"
        }

        # Move and rename
        Move-Item -Path $SourcePath -Destination $TargetPath -Force

        Write-Host "Moved $SourcePath to $TargetPath successfully." -ForegroundColor Green
    }   
    catch {
        Exit-BuildScript -Code 1 -Message "Error: $($_.Exception.Message)"
    }    
}

function Get-NormalizedGitPathFromStatusLine {
    param(
        [Parameter(Mandatory = $true)]
        [string] $StatusLine
    )

    if ([string]::IsNullOrWhiteSpace($StatusLine) -or $StatusLine.Length -lt 4) {
        return $null
    }

    $path = $StatusLine.Substring(3).Trim()

    # For renames, git emits "old -> new"; use the current path for policy checks.
    if ($path -like '* -> *') {
        $path = ($path -split ' -> ')[-1].Trim()
    }

    if ($path.StartsWith('"') -and $path.EndsWith('"') -and $path.Length -ge 2) {
        $path = $path.Substring(1, $path.Length - 2)
    }

    $path = $path.Replace('\\', '/')

    if ($path.StartsWith('./')) {
        $path = $path.Substring(2)
    }

    return $path
}
# END Utility Functions
############################################################################

#-----------------------------
# Variables
#-----------------------------
$Version = Get-Version
$InformationalVersion = $Version.AssemblyInformationalVersion
$PackageVersion = $Version.NuGetPackageVersion
$Configuration = if ($Release -or $Publish -or $DryRun) { "Release" } else { "Debug" }
$PublicRelease = if ($Publish -or $DryRun) { "/p:PublicRelease=true" } else { $null }
$TreatWarningsAsErrors = if ($Configuration -eq "Release") { '/p:TreatWarningsAsErrors=true' } else { $null }
$ArtifactsPath = if ($DryRun) { Join-Path -Path $ArtifactsBasePath -ChildPath "$($Version.NuGetPackageVersion)-dryrun" }
    else { Join-Path $ArtifactsBasePath $Version.NuGetPackageVersion }

#-----------------------------
# Validate Build Preconditions
#-----------------------------
if ([string]::IsNullOrWhiteSpace($InformationalVersion) -or [string]::IsNullOrWhiteSpace($PackageVersion)) {
    Exit-BuildScript -Code 1 -Message "nbgv output is missing AssemblyInformationalVersion or NuGetPackageVersion."
}

if ($Publish -or $PreserveArtifacts) {

    if (Test-Path -Path $ArtifactsBasePath) {
        Write-Host "`nThe artifacts base directory already exists: $ArtifactsBasePath" -ForegroundColor Green
    } else {
        Write-Host "`nCreating artifacts base directory: $ArtifactsBasePath" -ForegroundColor Green
        New-Item -ItemType Directory -Path $ArtifactsBasePath | Out-Null
    }

    # If the destination artifacts directory already exists...
    if (Test-Path -Path $ArtifactsPath) {
        Write-Host "`nThe destination artifacts directory already exists: $ArtifactsPath" -ForegroundColor Red
        Exit-BuildScript -Code 1 -Message "You MUST NOT overwrite existing artifacts!`nEither increment the version, or remove the existing directory."
    }
}

if ($Publish -or $DryRun) {
    
    if ($NoTests) {
        Exit-BuildScript -Code 1 -Message "-Publish or -DryRun with -NoTests is prohibited."
    }
    
    $statusLines = @(git status --porcelain)
    if ($LASTEXITCODE -ne 0) {
        Exit-BuildScript -Code $LASTEXITCODE -Message "Failed to determine git working tree status. Aborting."
    }
    
    $dirtyFiles = @(
        $statusLines |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object { Get-NormalizedGitPathFromStatusLine -StatusLine $_ } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Sort-Object -Unique
    )
        
    if ($dirtyFiles.Count -gt 0) {
        $buildScriptIsDirty = ($dirtyFiles.Count -eq 1) -and ($dirtyFiles[0] -ieq 'build.ps1')
        
        Write-Host "`nUncommitted Changes: $($dirtyFiles.Count)" -ForegroundColor Yellow
        $dirtyFiles | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        
        if ($Publish -or ($DryRun -and -not $buildScriptIsDirty)) {
            Write-Host "Dirty files:" -ForegroundColor Red
            Exit-BuildScript -Code 1 -Message "-Publish and -DryRun require a clean working tree. Go commit your changes and try again."
        }
        
        if ($DryRun -and $buildScriptIsDirty) {
            Write-Host "'build.ps1' is dirty! Allowing -DryRun to continue anyway." -ForegroundColor Yellow
        }
    }
}

#-----------------------------
# Real Work Begins Here
#-----------------------------
Write-Host "`nAll right people, get your asses in gear and get to work! v$($Version.NuGetPackageVersion) FTW" -ForegroundColor Green

#-----------------------------
# Clean up
#-----------------------------
Write-Host "`nCleaning Up:" -ForegroundColor Cyan
Remove-BinObjFolders -Path "."
Write-Host "`nCleaning Up: LocalPackages" -ForegroundColor Cyan
Remove-BuildArtifacts -Path "."
Write-Host "`nCleaning Up: Projects" -ForegroundColor Cyan
Remove-LocalPackages -Path $LocalPackagesPath

#-----------------------------
# Build
#-----------------------------
Write-Host "`nBuilding v$($Version.NuGetPackageVersion) in $Configuration..." -ForegroundColor Cyan

dotnet build -c $Configuration `
    $(if ($TreatWarningsAsErrors) { $TreatWarningsAsErrors }) `
    $(if ($PublicRelease) { $PublicRelease }) `
        /p:ContinuousIntegrationBuild=true

if ($LASTEXITCODE -ne 0) {
    Exit-BuildScript -Code $LASTEXITCODE -Message "Build failed!!!"
}

#-----------------------------
# Run tests
#-----------------------------
if (-not $NoTests) {
    Write-Host "`nRunning tests in $Configuration..." -ForegroundColor Cyan
    dotnet test -c $Configuration --no-build --verbosity normal `
        /p:ContinuousIntegrationBuild=true

    if ($LASTEXITCODE -ne 0) {
        Exit-BuildScript -Code $LASTEXITCODE -Message "Testing failed!!!"
    }
}
else {
    Write-Host "`nSkipping tests (-NoTests)" -ForegroundColor Yellow
}

#-----------------------------------------
# Find all packable projects
#-----------------------------------------
$projects = Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object {
    $xml = [xml](Get-Content $_.FullName)

    $isPackable = $false
    $isRoslynComponent = $false
    $assemblyName = $null

    foreach ($pg in $xml.Project.PropertyGroup) {
        if ($pg.AssemblyName) {
            $assemblyName = $pg.AssemblyName
        }
        
        if ($pg.IsPackable -and $pg.IsPackable -eq "true") {
            $isPackable = $true
        }
        
        if ($pg.IsGenerator -and $pg.IsGenerator -eq "true") {
            $isRoslynComponent = $true
        }
        if ($pg.IsRoslynComponent -and $pg.IsRoslynComponent -eq "true") {
            $isRoslynComponent = $true
        }

        # If a project is packable and marked as a Roslyn component, we know everything we need to know about it and can stop parsing the XML.
        if ($isPackable -and $isRoslynComponent) {
            break
        }
    }

    if ($isPackable) {
        [PSCustomObject]@{
            Path              = $_.FullName
            IsRoslynComponent = $isRoslynComponent
            AssemblyName      = $assemblyName
        }
    }
}

$roslynProjects = $projects | Where-Object { $_.IsRoslynComponent }
$normalProjects = $projects | Where-Object { -not $_.IsRoslynComponent }

Write-Host "Roslyn Projects: $($roslynProjects.Count)"
Write-Host "Normal Projects: $($normalProjects.Count)"

#-----------------------------------------
# Pack Roslyn Components First Just Because
#-----------------------------------------
foreach ($proj in $roslynProjects) {
    Write-Host "`nPacking Roslyn Component: $($proj.AssemblyName) v$($Version.NuGetPackageVersion)" -ForegroundColor Cyan

    # Generator/analyzer packages set IncludeBuildOutput=false, so there is no lib DLL or PDB
    # to place in a symbols/source package. Passing --include-symbols or --include-source would
    # create an empty .snupkg/.symbols.nupkg and fail with NU5017. Those flags are MSBuild global
    # properties that override the per-project <IncludeSymbols>false</IncludeSymbols> setting,
    # so they must be omitted here rather than suppressed from the project file alone.
    dotnet pack $proj.Path `
        --no-build `
        -c $Configuration `
        -o $LocalPackagesPath `
        /p:ContinuousIntegrationBuild=true `
    $(if ($TreatWarningsAsErrors) { $TreatWarningsAsErrors }) `
    $(if ($PublicRelease) { $PublicRelease })

    if ($LASTEXITCODE -ne 0) {
        Exit-BuildScript -Code $LASTEXITCODE -Message "Packing Roslyn Components Failed!!!"
    }
}

#-----------------------------------------
# Pack normal projects
#-----------------------------------------
if (-not $ToolingOnly) {
    foreach ($proj in $normalProjects) {
        Write-Host "`nPacking: $($proj.AssemblyName) v$($Version.NuGetPackageVersion)" -ForegroundColor Cyan

        dotnet pack $proj.Path `
            --no-build `
            -c $Configuration `
            -o $LocalPackagesPath `
            --include-source `
            --include-symbols `
            /p:ContinuousIntegrationBuild=true `
        $(if ($TreatWarningsAsErrors) { $TreatWarningsAsErrors })

        if ($LASTEXITCODE -ne 0) {
            Exit-BuildScript -Code $LASTEXITCODE -Message "Packing Normal Projects Failed!!!"
        }
    }
}

#-----------------------------
# Embed checksum/provenance banner
#-----------------------------
Write-Host "`nEmbedding checksum/provenance banners..." -ForegroundColor Cyan
Add-Type -AssemblyName System.IO.Compression.FileSystem

Get-ChildItem $LocalPackagesPath -Filter *.nupkg | ForEach-Object {
    $pkg = $_
    $checksum = Get-FileHash $pkg.FullName -Algorithm SHA256
    $gitHash = (git rev-parse HEAD)

    Write-Host "Embedding provenance for $($pkg.Name)..." -ForegroundColor Green
    $banner = @"
Build Provenance: Two Rivers - Aiel
Git Hash: $gitHash
Package: $($pkg.Name)
SHA256: $($checksum.Hash)
AssemblyInformationalVersion: $InformationalVersion
NuGetPackageVersion: $PackageVersion
Configuration: $Configuration
Timestamp: $(Get-Date -Format o)
"@

    $zip = [System.IO.Compression.ZipFile]::Open($pkg.FullName, 'Update')
    try {
        $existing = $zip.GetEntry("CHECKSUM.txt")
        if ($existing) { $existing.Delete() }

        $entry = $zip.CreateEntry("CHECKSUM.txt")
        $stream = $entry.Open()
        $writer = New-Object System.IO.StreamWriter($stream)
        $writer.Write($banner)
        $writer.Dispose()
    }
    finally {
        $zip.Dispose()
    }
}

#-----------------------------
# Publish / DryRun
#-----------------------------
if ($Publish -or $DryRun) {
    Write-Host "`nPreparing to publish artifacts..." -ForegroundColor Cyan

    $apiKey = [System.Environment]::GetEnvironmentVariable($NuGetApiKeyName)
    if (-not $apiKey -and -not $DryRun) {
        Exit-BuildScript -Code 1 -Message "$NuGetApiKeyName environment variable is missing or empty."
    }

    if ($Publish) {

        Write-Host "Publishing packages..." -ForegroundColor Cyan

        dotnet nuget push (Join-Path $LocalPackagesPath "*.nupkg") `
            --api-key $apiKey `
            --source $NuGetSource `
            --skip-duplicate `
            --no-symbols
            
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Publishing failed. Might not have any PDB files in it. Continuing on..." -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Dry run: skipping actual publish." -ForegroundColor Yellow
    }
}

if ($Publish -or ($DryRun -and $PreserveArtifacts)) {
    Write-Host "`nPreserving Artifacts: $ArtifactsPath" -ForegroundColor Cyan
    Move-Artifacts -SourcePath $LocalPackagesPath -TargetPath $ArtifactsPath
}

if ($Publish) {
    Write-Host "`nPublish completed." -ForegroundColor Green
}
elseif ($DryRun) {
    Write-Host "`nDry run completed." -ForegroundColor Yellow
} else {
    Write-Host "`nBuild completed." -ForegroundColor Green
}

if ($script:TranscriptStarted) {
    Stop-Transcript | Out-Null
    $script:TranscriptStarted = $false
}
