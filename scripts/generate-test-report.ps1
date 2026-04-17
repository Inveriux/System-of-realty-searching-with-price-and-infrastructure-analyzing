Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$TestsRoot = Join-Path $RepoRoot "Tests"
$ReportRoot = Join-Path $TestsRoot "coverage-report"
$GroupedTests = @{}

New-Item -ItemType Directory -Force -Path (Join-Path $ReportRoot "unit") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ReportRoot "integration") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ReportRoot "smoke") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $ReportRoot "combined") | Out-Null

function New-CoverageReport {
    param(
        [Parameter(Mandatory = $true)][string[]]$Patterns,
        [Parameter(Mandatory = $true)][string]$TargetDir
    )

    $files = foreach ($pattern in $Patterns) {
        Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName
    }

    if (-not $files) {
        return
    }

    Push-Location $RepoRoot
    try {
        dotnet tool run reportgenerator `
            "-reports:$($files -join ';')" `
            "-targetdir:$TargetDir" `
            "-reporttypes:Html;TextSummary"
    }
    finally {
        Pop-Location
    }
}

function Get-TrxCounters {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return @{ total = "-"; passed = "-"; failed = "-" }
    }

    [xml]$trx = Get-Content -Path $Path
    $counters = $trx.TestRun.ResultSummary.Counters
    return @{
        total = [string]$counters.total
        passed = [string]$counters.passed
        failed = [string]$counters.failed
    }
}

function Add-GroupedTestItem {
    param(
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$TestName
    )

    if (-not $GroupedTests.ContainsKey($Key)) {
        $GroupedTests[$Key] = New-Object System.Collections.Generic.List[string]
    }

    $GroupedTests[$Key].Add($TestName)
}

function Get-HtmlEncoded {
    param([string]$Text)

    return [System.Net.WebUtility]::HtmlEncode($Text)
}

function Import-GroupedTests {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][ValidateSet("unit", "integration", "smoke")][string]$Suite
    )

    if (-not (Test-Path $Path)) {
        return
    }

    [xml]$trx = Get-Content -Path $Path
    $methods = $trx.TestRun.TestDefinitions.UnitTest.TestMethod
    foreach ($method in $methods) {
        $className = [string]$method.className
        $testName = [string]$method.name
        $key = $null

        switch ($Suite) {
            "integration" {
                if ($className -like "*UserServiceTests*") { $key = "integration_user" }
                elseif ($className -like "*ListingServiceTests*") { $key = "integration_listing" }
                elseif ($className -like "*PricingAnalyticsTests*") { $key = "integration_pricing" }
            }
            "smoke" {
                if ($className -like "*UserServiceTests*") { $key = "smoke_user" }
                elseif ($className -like "*ListingServiceTests*") { $key = "smoke_listing" }
                elseif ($className -like "*PricingAnalyticsTests*") { $key = "smoke_pricing" }
            }
            "unit" {
                if ($className -like "*Backend.UnitTests.Authorization*") { $key = "unit_authorization" }
                elseif ($className -like "*Backend.UnitTests.Infrastructure*") { $key = "unit_infrastructure" }
                elseif ($className -like "*Backend.UnitTests.Listings*") { $key = "unit_listings" }
                elseif ($className -like "*Backend.UnitTests.Pricing*") { $key = "unit_pricing" }
                elseif ($className -like "*Backend.UnitTests.Search*") { $key = "unit_search" }
                elseif ($className -like "*Backend.UnitTests.Users*") { $key = "unit_users" }
            }
        }

        if ($null -ne $key) {
            Add-GroupedTestItem -Key $key -TestName $testName
        }
    }
}

function Get-GroupCardHtml {
    param(
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$Title
    )

    if (-not $GroupedTests.ContainsKey($Key) -or $GroupedTests[$Key].Count -eq 0) {
        return ""
    }

    $items = ($GroupedTests[$Key] | ForEach-Object { "      <li>$(Get-HtmlEncoded $_)</li>" }) -join [Environment]::NewLine
    return @"
<section class="service-card">
  <div class="service-head">
    <h3>$Title</h3>
    <span class="service-badge">$($GroupedTests[$Key].Count) passed</span>
  </div>
  <ul class="passed-list">
$items
  </ul>
</section>
"@
}

New-CoverageReport -Patterns @(
    (Join-Path $TestsRoot "coverage/unit/*.xml")
) -TargetDir (Join-Path $ReportRoot "unit")

New-CoverageReport -Patterns @(
    (Join-Path $TestsRoot "coverage/integration/*.xml")
) -TargetDir (Join-Path $ReportRoot "integration")

New-CoverageReport -Patterns @(
    (Join-Path $TestsRoot "coverage/smoke/*.xml")
) -TargetDir (Join-Path $ReportRoot "smoke")

New-CoverageReport -Patterns @(
    (Join-Path $TestsRoot "coverage/unit/*.xml"),
    (Join-Path $TestsRoot "coverage/integration/*.xml"),
    (Join-Path $TestsRoot "coverage/smoke/*.xml")
) -TargetDir (Join-Path $ReportRoot "combined")

$unitTrx = Join-Path $TestsRoot "test-results/unit/backend-unit-tests.trx"
$integrationTrx = Join-Path $TestsRoot "test-results/integration/backend-integration-tests.trx"
$smokeTrx = Join-Path $TestsRoot "test-results/smoke/backend-smoke-tests.trx"

$unit = Get-TrxCounters $unitTrx
$integration = Get-TrxCounters $integrationTrx
$smoke = Get-TrxCounters $smokeTrx

Import-GroupedTests -Path $unitTrx -Suite "unit"
Import-GroupedTests -Path $integrationTrx -Suite "integration"
Import-GroupedTests -Path $smokeTrx -Suite "smoke"

$html = @"
<!DOCTYPE html>
<html lang="ru">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Backend Test Report</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #f4efe8;
      --panel: #fffaf3;
      --line: #d8ccb9;
      --text: #1f2933;
      --muted: #6b7280;
      --accent: #0f766e;
      --danger: #b91c1c;
    }
    body {
      margin: 0;
      font-family: "Segoe UI", sans-serif;
      background: linear-gradient(180deg, #f7f1e8 0%, #f2f4f7 100%);
      color: var(--text);
    }
    main {
      max-width: 1120px;
      margin: 0 auto;
      padding: 32px 20px 48px;
    }
    h1, h2 { margin: 0 0 16px; }
    p { color: var(--muted); }
    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 16px;
      margin: 24px 0 32px;
    }
    .card {
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 16px;
      padding: 18px;
      box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
    }
    .label { font-size: 13px; text-transform: uppercase; letter-spacing: 0.08em; color: var(--muted); }
    .value { font-size: 34px; font-weight: 700; margin: 8px 0; }
    .ok { color: var(--accent); }
    .bad { color: var(--danger); }
    .services {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 16px;
      margin: 18px 0 32px;
    }
    .service-card {
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 16px;
      padding: 18px;
      box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
    }
    .service-head {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      align-items: baseline;
      margin-bottom: 12px;
    }
    .service-head h3 {
      margin: 0;
      font-size: 18px;
    }
    .service-badge {
      font-size: 12px;
      color: var(--accent);
      background: #dbf3ef;
      border-radius: 999px;
      padding: 6px 10px;
      white-space: nowrap;
    }
    .passed-list {
      margin: 0;
      padding-left: 18px;
      color: var(--text);
    }
    .passed-list li {
      margin: 0 0 8px;
      line-height: 1.4;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 16px;
      overflow: hidden;
    }
    th, td {
      padding: 14px 16px;
      border-bottom: 1px solid var(--line);
      text-align: left;
    }
    th { background: #f3eadf; }
    tr:last-child td { border-bottom: 0; }
    a { color: var(--accent); text-decoration: none; font-weight: 600; }
    a:hover { text-decoration: underline; }
  </style>
</head>
<body>
  <main>
    <h1>Backend Test Dashboard</h1>
    <p>HTML-сводка по unit, integration и smoke тестам с разбивкой по сервисам и ссылками на артефакты.</p>

    <div class="grid">
      <section class="card">
        <div class="label">Unit</div>
        <div class="value ok">$($unit.passed) / $($unit.total)</div>
        <div class="label">Failed: <span class="bad">$($unit.failed)</span></div>
      </section>
      <section class="card">
        <div class="label">Integration</div>
        <div class="value ok">$($integration.passed) / $($integration.total)</div>
        <div class="label">Failed: <span class="bad">$($integration.failed)</span></div>
      </section>
      <section class="card">
        <div class="label">Smoke</div>
        <div class="value ok">$($smoke.passed) / $($smoke.total)</div>
        <div class="label">Failed: <span class="bad">$($smoke.failed)</span></div>
      </section>
    </div>

    <h2>Integration By Service</h2>
    <div class="services">
      $(Get-GroupCardHtml -Key "integration_user" -Title "UserService")
      $(Get-GroupCardHtml -Key "integration_listing" -Title "ListingService")
      $(Get-GroupCardHtml -Key "integration_pricing" -Title "PricingAnalyticsService")
    </div>

    <h2>Smoke By Service</h2>
    <div class="services">
      $(Get-GroupCardHtml -Key "smoke_user" -Title "UserService")
      $(Get-GroupCardHtml -Key "smoke_listing" -Title "ListingService")
      $(Get-GroupCardHtml -Key "smoke_pricing" -Title "PricingAnalyticsService")
    </div>

    <h2>Unit By Module</h2>
    <div class="services">
      $(Get-GroupCardHtml -Key "unit_authorization" -Title "Authorization")
      $(Get-GroupCardHtml -Key "unit_listings" -Title "Listings")
      $(Get-GroupCardHtml -Key "unit_search" -Title "Search")
      $(Get-GroupCardHtml -Key "unit_infrastructure" -Title "Infrastructure")
      $(Get-GroupCardHtml -Key "unit_pricing" -Title "Pricing")
      $(Get-GroupCardHtml -Key "unit_users" -Title "Users")
    </div>

    <h2>Artifacts</h2>
    <table>
      <thead>
        <tr>
          <th>Suite</th>
          <th>TRX</th>
          <th>Coverage HTML</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td>Unit</td>
          <td><a href="test-results/unit/backend-unit-tests.trx">backend-unit-tests.trx</a></td>
          <td><a href="coverage-report/unit/index.html">coverage-report/unit</a></td>
        </tr>
        <tr>
          <td>Integration</td>
          <td><a href="test-results/integration/backend-integration-tests.trx">backend-integration-tests.trx</a></td>
          <td><a href="coverage-report/integration/index.html">coverage-report/integration</a></td>
        </tr>
        <tr>
          <td>Smoke</td>
          <td><a href="test-results/smoke/backend-smoke-tests.trx">backend-smoke-tests.trx</a></td>
          <td><a href="coverage-report/smoke/index.html">coverage-report/smoke</a></td>
        </tr>
        <tr>
          <td>Combined coverage</td>
          <td>-</td>
          <td><a href="coverage-report/combined/index.html">coverage-report/combined</a></td>
        </tr>
      </tbody>
    </table>
  </main>
</body>
</html>
"@

Set-Content -Path (Join-Path $TestsRoot "index.html") -Value $html -Encoding UTF8
Write-Host "Generated HTML reports in $TestsRoot"
