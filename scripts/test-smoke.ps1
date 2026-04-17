Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "test-common.ps1")

Invoke-DotnetTests `
    -ProjectPath (Join-Path $RepoRoot "src/Backend/tests/Backend.IntegrationTests/Backend.IntegrationTests.csproj") `
    -Kind "smoke" `
    -Filter "Category=Smoke" `
    -Name "backend-smoke-tests"
