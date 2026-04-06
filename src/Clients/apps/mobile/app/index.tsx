import React, { useMemo, useRef, useState } from "react";
import { Pressable, Text, View } from "react-native";
import { StatusBar } from "expo-status-bar";
import { SafeAreaView, useSafeAreaInsets } from "react-native-safe-area-context";
import { WebView } from "react-native-webview";

const DEFAULT_WEB_APP_URL = process.env.EXPO_PUBLIC_WEB_APP_URL ?? "https://diploma.com";

type ThemeMode = "light" | "dark";

const MOBILE_TABS = [
  { key: "listings", label: "Каталог", path: "/listings" },
  { key: "analytics", label: "Аналитика", path: "/analytics/pricing" },
  { key: "favorites", label: "Избранное", path: "/favorites" },
  { key: "profile", label: "Профиль", path: "/profile" },
] as const;

const NATIVE_SHELL_PALETTES = {
  light: {
    active: "#5b21b6",
    inactive: "#8b78a7",
    barBackground: "#f2eaff",
    screenBackground: "#f2eaff",
    barBorder: "rgba(109, 40, 217, 0.22)",
    activeBackground: "rgba(109, 40, 217, 0.12)",
    shadowColor: "#6d28d9",
  },
  dark: {
    active: "#e9ddff",
    inactive: "#9f8ac7",
    barBackground: "#1a1328",
    screenBackground: "#120d1f",
    barBorder: "rgba(186, 158, 255, 0.24)",
    activeBackground: "rgba(159, 123, 255, 0.18)",
    shadowColor: "#090511",
  },
} as const;

type ShellPalette = (typeof NATIVE_SHELL_PALETTES)[ThemeMode];

function SearchTabIcon({ active, palette }: { active: boolean; palette: ShellPalette }) {
  const color = active ? palette.active : palette.inactive;

  return (
    <View style={{ width: 22, height: 22, alignItems: "center", justifyContent: "center" }}>
      <View
        style={{
          width: 12,
          height: 12,
          borderRadius: 999,
          borderWidth: 1.8,
          borderColor: color,
        }}
      />
      <View
        style={{
          position: "absolute",
          width: 8,
          height: 1.8,
          backgroundColor: color,
          borderRadius: 999,
          transform: [{ translateX: 5 }, { translateY: 5 }, { rotate: "45deg" }],
        }}
      />
    </View>
  );
}

function ChartTabIcon({ active, palette }: { active: boolean; palette: ShellPalette }) {
  const color = active ? palette.active : palette.inactive;

  return (
    <View style={{ width: 22, height: 22, alignItems: "center", justifyContent: "flex-end", gap: 2 }}>
      <View style={{ width: 14, height: 1.8, backgroundColor: color, borderRadius: 999, marginBottom: 1 }} />
      <View style={{ flexDirection: "row", alignItems: "flex-end", gap: 3 }}>
        <View style={{ width: 2.2, height: 7, borderRadius: 999, backgroundColor: color }} />
        <View style={{ width: 2.2, height: 11, borderRadius: 999, backgroundColor: color }} />
        <View style={{ width: 2.2, height: 15, borderRadius: 999, backgroundColor: color }} />
      </View>
    </View>
  );
}

function FavoritesTabIcon({ active, palette }: { active: boolean; palette: ShellPalette }) {
  const color = active ? palette.active : palette.inactive;

  return (
    <View style={{ width: 22, height: 22, alignItems: "center", justifyContent: "center" }}>
      <View
        style={{
          width: 14,
          height: 16,
          borderWidth: 1.8,
          borderColor: color,
          borderRadius: 2,
          backgroundColor: "transparent",
        }}
      />
      <View
        style={{
          position: "absolute",
          bottom: 3.2,
          width: 7,
          height: 7,
          borderLeftWidth: 1.8,
          borderBottomWidth: 1.8,
          borderColor: color,
          backgroundColor: palette.barBackground,
          transform: [{ rotate: "-45deg" }],
        }}
      />
    </View>
  );
}

function ProfileTabIcon({ active, palette }: { active: boolean; palette: ShellPalette }) {
  const color = active ? palette.active : palette.inactive;

  return (
    <View style={{ width: 22, height: 22, alignItems: "center", justifyContent: "center" }}>
      <View
        style={{
          width: 16,
          height: 16,
          borderRadius: 999,
          borderWidth: 1.8,
          borderColor: color,
          alignItems: "center",
        }}
      >
        <View
          style={{
            marginTop: 2.4,
            width: 4.6,
            height: 4.6,
            borderRadius: 999,
            backgroundColor: color,
          }}
        />
        <View
          style={{
            marginTop: 1.8,
            width: 8.2,
            height: 4.6,
            borderTopLeftRadius: 6,
            borderTopRightRadius: 6,
            backgroundColor: color,
          }}
        />
      </View>
    </View>
  );
}

function TabIcon({
  tabKey,
  active,
  palette,
}: {
  tabKey: string;
  active: boolean;
  palette: ShellPalette;
}) {
  switch (tabKey) {
    case "analytics":
      return <ChartTabIcon active={active} palette={palette} />;
    case "favorites":
      return <FavoritesTabIcon active={active} palette={palette} />;
    case "profile":
      return <ProfileTabIcon active={active} palette={palette} />;
    default:
      return <SearchTabIcon active={active} palette={palette} />;
  }
}

function resolveMobileSourceUrl(rawUrl: string) {
  try {
    const url = new URL(rawUrl);

    if (url.pathname === "/" || url.pathname === "") {
      url.pathname = "/listings";
    }

    url.searchParams.set("__mobile_client", "1");

    return url.toString();
  } catch {
    const baseUrl = rawUrl.endsWith("/listings") ? rawUrl : `${rawUrl.replace(/\/+$/, "")}/listings`;
    const separator = baseUrl.includes("?") ? "&" : "?";
    return `${baseUrl}${separator}__mobile_client=1`;
  }
}

export default function MobileWebViewScreen() {
  const insets = useSafeAreaInsets();
  const webViewRef = useRef<WebView>(null);
  const [hasError, setHasError] = useState(false);
  const [nativeTheme, setNativeTheme] = useState<ThemeMode>("light");
  const initialUrl = useMemo(() => resolveMobileSourceUrl(DEFAULT_WEB_APP_URL), []);
  const [currentUrl, setCurrentUrl] = useState(initialUrl);
  const [pendingTabKey, setPendingTabKey] = useState<string | null>(null);
  const source = useMemo(() => ({ uri: initialUrl }), [initialUrl]);
  const palette = NATIVE_SHELL_PALETTES[nativeTheme];

  const currentTabKey = useMemo(() => {
    try {
      const pathname = new URL(currentUrl).pathname;
      if (pathname.startsWith("/analytics/pricing")) {
        return "analytics";
      }
      if (pathname.startsWith("/favorites")) {
        return "favorites";
      }
      if (pathname.startsWith("/profile")) {
        return "profile";
      }
      return "listings";
    } catch {
      return "listings";
    }
  }, [currentUrl]);

  const activeTabKey = pendingTabKey ?? currentTabKey;

  function navigateTo(path: string) {
    try {
      const url = new URL(path, initialUrl).toString();
      const nextTabKey =
        path.startsWith("/analytics/pricing")
          ? "analytics"
          : path.startsWith("/favorites")
            ? "favorites"
            : path.startsWith("/profile")
              ? "profile"
              : "listings";
      setPendingTabKey(nextTabKey);
      setCurrentUrl(url);
      webViewRef.current?.injectJavaScript(`
        (function() {
          try {
            var currentTheme = null;
            try {
              currentTheme = window.localStorage.getItem("diploma_theme");
            } catch (e) {}
            if (currentTheme !== "light" && currentTheme !== "dark") {
              var root = document.documentElement || document.querySelector("html");
              var rootTheme = root ? root.getAttribute("data-theme") : null;
              currentTheme = rootTheme === "light" || rootTheme === "dark" ? rootTheme : "light";
            }
            window.localStorage.setItem("diploma_theme", currentTheme);
            document.cookie = "diploma_theme=" + encodeURIComponent(currentTheme) + "; path=/; max-age=31536000; SameSite=Lax";
            var root = document.documentElement || document.querySelector("html");
            if (root) {
              root.setAttribute("data-theme", currentTheme);
              root.style.colorScheme = currentTheme;
            }
            var nextUrl = new URL(${JSON.stringify(url)});
            nextUrl.searchParams.set("__mobile_client", "1");
            nextUrl.searchParams.set("__mobile_theme", currentTheme);
            window.location.assign(nextUrl.toString());
            return true;
          } catch (e) {
            window.location.assign(${JSON.stringify(url)});
            return true;
          }
        })();
      `);
    } catch {
      // Ignore malformed path input.
    }
  }

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: palette.screenBackground }}>
      <StatusBar style={nativeTheme === "dark" ? "light" : "dark"} />

      <View style={{ flex: 1, backgroundColor: palette.screenBackground }}>
        <WebView
          ref={webViewRef}
          source={source}
          originWhitelist={["*"]}
          cacheEnabled={false}
          incognito
          injectedJavaScriptBeforeContentLoaded={`
            (function() {
              var SHELL_STYLE_ID = "mobile-client-shell-style";
              function send(type, payload) {
                try {
                  window.ReactNativeWebView.postMessage(JSON.stringify({ type: type, payload: payload }));
                } catch (e) {}
              }
              function getRoot() {
                return document.documentElement || document.querySelector("html");
              }
              send("boot", window.location.href);
              function ensureShellStyle() {
                var css = ''
                  + '[data-mobile-client-shell="true"] .app-header-icon-cluster > button,'
                  + '[data-mobile-client-shell="true"] .app-header-account,'
                  + '[data-mobile-client-shell="true"] .app-header-create-button{display:none !important;}'
                  + '[data-mobile-client-shell="true"] .app-header-search-icon,'
                  + '[data-mobile-client-shell="true"] .theme-toggle-button,'
                  + '[data-mobile-client-shell="true"] .app-header-icon-cluster > div,'
                  + '[data-mobile-client-shell="true"] .app-header-account-button,'
                  + '[data-mobile-client-shell="true"] .app-header-account-button img,'
                  + '[data-mobile-client-shell="true"] .theme-toggle-button img{transition:none !important;animation:none !important;}'
                  + 'html[data-theme="dark"][data-mobile-client-shell="true"] .app-header-search-icon,'
                  + 'html[data-theme="dark"][data-mobile-client-shell="true"] .app-header-account-button img,'
                  + 'html[data-theme="dark"][data-mobile-client-shell="true"] .app-header-icon-cluster img{filter:brightness(0) invert(1) !important;}'
                  + 'html[data-theme="light"][data-mobile-client-shell="true"] .app-header-search-icon,'
                  + 'html[data-theme="light"][data-mobile-client-shell="true"] .app-header-account-button img,'
                  + 'html[data-theme="light"][data-mobile-client-shell="true"] .app-header-icon-cluster img{filter:none !important;}'
                  + 'html[data-theme="dark"][data-mobile-client-shell="true"] .theme-toggle-button{'
                  + 'color:#f5e9ff !important;'
                  + 'background:linear-gradient(180deg, rgba(109, 40, 217, 0.92), rgba(76, 29, 149, 0.96)) !important;'
                  + 'border:1px solid rgba(196, 181, 253, 0.34) !important;'
                  + 'box-shadow:0 10px 24px rgba(76, 29, 149, 0.28) !important;}'
                  + 'html[data-theme="light"][data-mobile-client-shell="true"] .theme-toggle-button{'
                  + 'color:#5b21b6 !important;'
                  + 'background:linear-gradient(180deg, rgba(245, 232, 255, 0.98), rgba(233, 213, 255, 0.98)) !important;'
                  + 'border:1px solid rgba(139, 92, 246, 0.28) !important;'
                  + 'box-shadow:0 10px 24px rgba(139, 92, 246, 0.16) !important;}';
                var existingStyle = document.getElementById(SHELL_STYLE_ID);
                if (existingStyle) {
                  existingStyle.textContent = css;
                  return;
                }
                var style = document.createElement("style");
                style.id = SHELL_STYLE_ID;
                style.textContent = css;
                if (document.head) {
                  document.head.appendChild(style);
                  return;
                }
                var root = getRoot();
                if (root) {
                  root.appendChild(style);
                }
              }
              window.addEventListener("error", function(event) {
                send("error", String(event.message || "window error"));
              });
              window.addEventListener("unhandledrejection", function(event) {
                var reason = event && event.reason ? String(event.reason) : "unhandled rejection";
                send("error", reason);
              });
              document.addEventListener("DOMContentLoaded", function() {
                send("dom-ready", window.location.href);
                applyShellStyles();
              });
              function applyShellStyles() {
                var root = getRoot();
                if (!root) {
                  return;
                }
                root.setAttribute("data-mobile-client-shell", "true");
                ensureShellStyle();
              }
              applyShellStyles();
              var observerInterval = window.setInterval(function() {
                var root = getRoot();
                if (!root) {
                  return;
                }
                applyShellStyles();
                window.clearInterval(observerInterval);
              }, 10);
            })();
            true;
          `}
          onLoadStart={(event) => {
            setHasError(false);
            setCurrentUrl(event.nativeEvent.url);
          }}
          onLoad={() => {
            setHasError(false);
          }}
          onLoadEnd={() => {
            setHasError(false);
          }}
          onNavigationStateChange={(event) => {
            setCurrentUrl(event.url);
            try {
              const pathname = new URL(event.url).pathname;
              const nextTabKey =
                pathname.startsWith("/analytics/pricing")
                  ? "analytics"
                  : pathname.startsWith("/favorites")
                    ? "favorites"
                    : pathname.startsWith("/profile")
                      ? "profile"
                      : "listings";
              setPendingTabKey((current) => (current === nextTabKey ? null : current));
            } catch {
              // Ignore malformed navigation state URLs.
            }
          }}
          onError={() => {
            setHasError(true);
          }}
          onMessage={(event) => {
            try {
              const message = JSON.parse(event.nativeEvent.data) as { type?: string; payload?: unknown };
              if (
                (message.type === "theme-init" || message.type === "theme-toggle") &&
                (message.payload === "light" || message.payload === "dark")
              ) {
                setNativeTheme(message.payload);
              }
            } catch {
              // Ignore debug messages from the page.
            }
          }}
          pullToRefreshEnabled
          allowsBackForwardNavigationGestures
          setSupportMultipleWindows={false}
          sharedCookiesEnabled
          thirdPartyCookiesEnabled
          javaScriptEnabled
          domStorageEnabled
          style={{ flex: 1, backgroundColor: palette.screenBackground }}
        />

        {hasError ? (
          <View
            style={{
              position: "absolute",
              inset: 0,
              alignItems: "center",
              justifyContent: "center",
              backgroundColor: "#fbf8f2",
              paddingHorizontal: 24,
              gap: 12,
            }}
          >
            <Text style={{ fontSize: 22, fontWeight: "800", color: "#1f2937", textAlign: "center" }}>
              Веб-версия недоступна
            </Text>
            <Text style={{ fontSize: 14, lineHeight: 20, color: "#6b7280", textAlign: "center" }}>
              Проверьте доступность {DEFAULT_WEB_APP_URL} и повторите загрузку.
            </Text>
            <Pressable
              onPress={() => {
                setHasError(false);
                webViewRef.current?.reload();
              }}
              style={{
                borderRadius: 14,
                backgroundColor: "#8c3d21",
                paddingHorizontal: 18,
                paddingVertical: 12,
              }}
            >
              <Text style={{ fontSize: 14, fontWeight: "700", color: "#ffffff" }}>Повторить</Text>
            </Pressable>
          </View>
        ) : null}
      </View>

      <View
        style={{
          borderTopWidth: 1,
          borderTopColor: palette.barBorder,
          backgroundColor: palette.barBackground,
          paddingTop: 8,
          paddingBottom: Math.max(insets.bottom, 8),
          paddingHorizontal: 8,
          flexDirection: "row",
          alignItems: "flex-end",
          shadowColor: palette.shadowColor,
          shadowOpacity: 0.1,
          shadowRadius: 16,
          shadowOffset: { width: 0, height: -4 },
          elevation: 12,
        }}
      >
        {MOBILE_TABS.map((tab) => {
          const isActive = activeTabKey === tab.key;
          return (
            <Pressable
              key={tab.key}
              onPress={() => navigateTo(tab.path)}
              style={{
                flex: 1,
                minHeight: 58,
                borderRadius: 16,
                alignItems: "center",
                justifyContent: "center",
                gap: 4,
                backgroundColor: isActive ? palette.activeBackground : "transparent",
                paddingHorizontal: 6,
                paddingVertical: 6,
              }}
            >
              <TabIcon tabKey={tab.key} active={isActive} palette={palette} />
              <Text
                style={{
                  fontSize: 11,
                  fontWeight: isActive ? "800" : "700",
                  color: isActive ? palette.active : palette.inactive,
                  textAlign: "center",
                }}
              >
                {tab.label}
              </Text>
            </Pressable>
          );
        })}
      </View>
    </SafeAreaView>
  );
}
