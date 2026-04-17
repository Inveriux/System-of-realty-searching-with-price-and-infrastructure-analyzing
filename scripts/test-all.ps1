Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

& (Join-Path $PSScriptRoot "test-unit.ps1")
& (Join-Path $PSScriptRoot "test-integration.ps1")
& (Join-Path $PSScriptRoot "test-smoke.ps1")
