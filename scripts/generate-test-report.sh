#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TESTS_ROOT="$REPO_ROOT/Tests"
REPORT_ROOT="$TESTS_ROOT/coverage-report"
TMP_DIR="$(mktemp -d)"

cleanup() {
  rm -rf "$TMP_DIR"
}

trap cleanup EXIT

mkdir -p "$REPORT_ROOT"/unit "$REPORT_ROOT"/integration "$REPORT_ROOT"/smoke "$REPORT_ROOT"/combined

generate_coverage_report() {
  local source_pattern="$1"
  local target_dir="$2"

  shopt -s nullglob
  local matches=($source_pattern)
  shopt -u nullglob

  if [[ ${#matches[@]} -eq 0 ]]; then
    return 0
  fi

  local reports_arg
  IFS=';'; reports_arg="${matches[*]}"; unset IFS

  (
    cd "$REPO_ROOT"
    dotnet tool run reportgenerator \
      "-reports:$reports_arg" \
      "-targetdir:$target_dir" \
      "-reporttypes:Html;TextSummary"
  )
}

read_counter() {
  local trx_path="$1"
  local key="$2"

  if [[ ! -f "$trx_path" ]]; then
    printf '%s' "-"
    return 0
  fi

  local value
  value="$(grep -o "${key}=\"[0-9]*\"" "$trx_path" | head -n 1 | sed -E "s/${key}=\"([0-9]+)\"/\\1/")"
  printf '%s' "${value:-0}"
}

html_escape() {
  local value="$1"
  value="${value//&/&amp;}"
  value="${value//</&lt;}"
  value="${value//>/&gt;}"
  printf '%s' "$value"
}

append_group_item() {
  local key="$1"
  local test_name="$2"
  local safe_key="${key//\//_}"
  safe_key="${safe_key// /_}"
  printf '<li>%s</li>\n' "$(html_escape "$test_name")" >>"$TMP_DIR/$safe_key.items"
}

count_group_items() {
  local key="$1"
  local safe_key="${key//\//_}"
  safe_key="${safe_key// /_}"
  if [[ -f "$TMP_DIR/$safe_key.items" ]]; then
    wc -l <"$TMP_DIR/$safe_key.items" | tr -d ' '
  else
    printf '0'
  fi
}

render_group_card() {
  local key="$1"
  local title="$2"
  local safe_key="${key//\//_}"
  safe_key="${safe_key// /_}"
  local count
  count="$(count_group_items "$key")"

  if [[ "$count" == "0" ]]; then
    return 0
  fi

  cat <<EOF
<section class="service-card">
  <div class="service-head">
    <h3>$title</h3>
    <span class="service-badge">$count passed</span>
  </div>
  <ul class="passed-list">
$(cat "$TMP_DIR/$safe_key.items")
  </ul>
</section>
EOF
}

extract_grouped_tests() {
  local trx_path="$1"
  local suite="$2"

  [[ -f "$trx_path" ]] || return 0

  while IFS=$'\t' read -r class_name test_name; do
    [[ -n "${class_name:-}" && -n "${test_name:-}" ]] || continue

    local group_key=""
    case "$suite" in
      integration)
        case "$class_name" in
          *UserServiceTests*) group_key="integration_user" ;;
          *ListingServiceTests*) group_key="integration_listing" ;;
          *PricingAnalyticsTests*) group_key="integration_pricing" ;;
        esac
        ;;
      smoke)
        case "$class_name" in
          *UserServiceTests*) group_key="smoke_user" ;;
          *ListingServiceTests*) group_key="smoke_listing" ;;
          *PricingAnalyticsTests*) group_key="smoke_pricing" ;;
        esac
        ;;
      unit)
        case "$class_name" in
          *Backend.UnitTests.Authorization*) group_key="unit_authorization" ;;
          *Backend.UnitTests.Infrastructure*) group_key="unit_infrastructure" ;;
          *Backend.UnitTests.Listings*) group_key="unit_listings" ;;
          *Backend.UnitTests.Pricing*) group_key="unit_pricing" ;;
          *Backend.UnitTests.Search*) group_key="unit_search" ;;
          *Backend.UnitTests.Users*) group_key="unit_users" ;;
        esac
        ;;
    esac

    if [[ -n "$group_key" ]]; then
      append_group_item "$group_key" "$test_name"
    fi
  done < <(
    grep '<TestMethod ' "$trx_path" |
      sed -E 's/.*className="([^"]+)" name="([^"]+)".*/\1\t\2/'
  )
}

generate_coverage_report "$TESTS_ROOT/coverage/unit/*.xml" "$REPORT_ROOT/unit"
generate_coverage_report "$TESTS_ROOT/coverage/integration/*.xml" "$REPORT_ROOT/integration"
generate_coverage_report "$TESTS_ROOT/coverage/smoke/*.xml" "$REPORT_ROOT/smoke"
generate_coverage_report "$TESTS_ROOT/coverage/unit/*.xml $TESTS_ROOT/coverage/integration/*.xml $TESTS_ROOT/coverage/smoke/*.xml" "$REPORT_ROOT/combined"

unit_trx="$TESTS_ROOT/test-results/unit/backend-unit-tests.trx"
integration_trx="$TESTS_ROOT/test-results/integration/backend-integration-tests.trx"
smoke_trx="$TESTS_ROOT/test-results/smoke/backend-smoke-tests.trx"

unit_total="$(read_counter "$unit_trx" "total")"
unit_passed="$(read_counter "$unit_trx" "passed")"
unit_failed="$(read_counter "$unit_trx" "failed")"

integration_total="$(read_counter "$integration_trx" "total")"
integration_passed="$(read_counter "$integration_trx" "passed")"
integration_failed="$(read_counter "$integration_trx" "failed")"

smoke_total="$(read_counter "$smoke_trx" "total")"
smoke_passed="$(read_counter "$smoke_trx" "passed")"
smoke_failed="$(read_counter "$smoke_trx" "failed")"

extract_grouped_tests "$unit_trx" "unit"
extract_grouped_tests "$integration_trx" "integration"
extract_grouped_tests "$smoke_trx" "smoke"

cat >"$TESTS_ROOT/index.html" <<EOF
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
        <div class="value ok">$unit_passed / $unit_total</div>
        <div class="label">Failed: <span class="bad">$unit_failed</span></div>
      </section>
      <section class="card">
        <div class="label">Integration</div>
        <div class="value ok">$integration_passed / $integration_total</div>
        <div class="label">Failed: <span class="bad">$integration_failed</span></div>
      </section>
      <section class="card">
        <div class="label">Smoke</div>
        <div class="value ok">$smoke_passed / $smoke_total</div>
        <div class="label">Failed: <span class="bad">$smoke_failed</span></div>
      </section>
    </div>

    <h2>Integration By Service</h2>
    <div class="services">
$(render_group_card "integration_user" "UserService")
$(render_group_card "integration_listing" "ListingService")
$(render_group_card "integration_pricing" "PricingAnalyticsService")
    </div>

    <h2>Smoke By Service</h2>
    <div class="services">
$(render_group_card "smoke_user" "UserService")
$(render_group_card "smoke_listing" "ListingService")
$(render_group_card "smoke_pricing" "PricingAnalyticsService")
    </div>

    <h2>Unit By Module</h2>
    <div class="services">
$(render_group_card "unit_authorization" "Authorization")
$(render_group_card "unit_listings" "Listings")
$(render_group_card "unit_search" "Search")
$(render_group_card "unit_infrastructure" "Infrastructure")
$(render_group_card "unit_pricing" "Pricing")
$(render_group_card "unit_users" "Users")
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
EOF

printf 'Generated HTML reports in %s\n' "$TESTS_ROOT"
