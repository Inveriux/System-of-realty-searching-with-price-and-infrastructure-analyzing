#!/usr/bin/env bash

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLIENTS_ROOT="$REPO_ROOT/src/Clients"
MOBILE_ROOT="$CLIENTS_ROOT/apps/mobile"
ANDROID_ROOT="$MOBILE_ROOT/android"

ANDROID_SDK_PATH="${ANDROID_HOME:-${ANDROID_SDK_ROOT:-/opt/android-sdk}}"
API_PORT="${MOBILE_API_PORT:-15102}"
METRO_PORT="${RCT_METRO_PORT:-8081}"

export ANDROID_HOME="$ANDROID_SDK_PATH"
export ANDROID_SDK_ROOT="$ANDROID_SDK_PATH"
export PATH="$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin:$PATH"

if [[ ! -d "$ANDROID_ROOT" ]]; then
  echo "Android project not found: $ANDROID_ROOT"
  echo "Run: cd $MOBILE_ROOT && pnpm exec expo prebuild --platform android"
  exit 1
fi

if [[ ! -d "$ANDROID_HOME" ]]; then
  echo "Android SDK not found: $ANDROID_HOME"
  echo "Set ANDROID_HOME or ANDROID_SDK_ROOT to a valid SDK path."
  exit 1
fi

if ! command -v adb >/dev/null 2>&1; then
  echo "adb not found in PATH."
  exit 1
fi

if ! command -v pnpm >/dev/null 2>&1; then
  echo "pnpm not found in PATH."
  exit 1
fi

if [[ ! -f "$ANDROID_ROOT/local.properties" ]]; then
  printf 'sdk.dir=%s\n' "$ANDROID_HOME" > "$ANDROID_ROOT/local.properties"
fi

if ! adb get-state >/dev/null 2>&1; then
  echo "No Android device detected via adb."
  echo "Check USB debugging and run: adb devices"
  exit 1
fi

adb reverse "tcp:$METRO_PORT" "tcp:$METRO_PORT" >/dev/null
adb reverse "tcp:$API_PORT" "tcp:$API_PORT" >/dev/null

cd "$MOBILE_ROOT"
pnpm start -- --dev-client --host localhost
