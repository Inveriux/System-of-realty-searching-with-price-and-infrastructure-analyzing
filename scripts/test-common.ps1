Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$ArtifactsRoot = Join-Path $RepoRoot "Tests"

function Invoke-DotnetTests {
    param(
        [Parameter(Mandatory = $true)][string]$ProjectPath,
        [Parameter(Mandatory = $true)][string]$Kind,
        [Parameter(Mandatory = $true)][string]$Filter,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $resultsDir = Join-Path $ArtifactsRoot "test-results/$Kind"
    $coverageDir = Join-Path $ArtifactsRoot "coverage/$Kind"
    $combinedDir = Join-Path $ArtifactsRoot "coverage/combined"
    $logsDir = Join-Path $ArtifactsRoot "logs"
    $logFile = Join-Path $logsDir "$Name.log"

    New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null
    New-Item -ItemType Directory -Force -Path $coverageDir | Out-Null
    New-Item -ItemType Directory -Force -Path $combinedDir | Out-Null
    New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

    Push-Location $RepoRoot
    try {
        dotnet test $ProjectPath `
            --configuration Debug `
            --filter $Filter `
            --results-directory $resultsDir `
            --logger "trx;LogFileName=$Name.trx" `
            --collect:"XPlat Code Coverage" `
            -m:1 `
            -nodeReuse:false `
            -p:UseSharedCompilation=false
        if ($LASTEXITCODE -ne 0) { throw "dotnet test failed for $Name" }
    }
    finally {
        Pop-Location
    }

    @(
        "name=$Name"
        "kind=$Kind"
        "project=$ProjectPath"
        "filter=$Filter"
        "results_dir=$resultsDir"
        "coverage_dir=$coverageDir"
    ) | Set-Content -Path $logFile

    $coverageFile = Get-ChildItem -Path $resultsDir -Recurse -Filter "coverage.cobertura.xml" | Sort-Object FullName | Select-Object -Last 1
    if ($null -ne $coverageFile) {
        Copy-Item $coverageFile.FullName (Join-Path $coverageDir "$Name.cobertura.xml") -Force
    }
}
