Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "test-common.ps1")

Invoke-DotnetTests `
    -ProjectPath (Join-Path $RepoRoot "src/Backend/tests/Backend.UnitTests/Backend.UnitTests.csproj") `
    -Kind "unit" `
    -Filter "Category=Unit" `
    -Name "backend-unit-tests"
