Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "test-common.ps1")

Invoke-DotnetTests `
    -ProjectPath (Join-Path $RepoRoot "src/Backend/tests/Backend.IntegrationTests/Backend.IntegrationTests.csproj") `
    -Kind "integration" `
    -Filter "Category=Integration&Category!=Smoke" `
    -Name "backend-integration-tests"
