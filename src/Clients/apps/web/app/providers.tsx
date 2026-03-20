"use client";

import { getCurrentUser, refreshToken, signIn, signUp } from "@diploma/api/users";
import { ApiClient } from "@diploma/api/http";
import type { CurrentUserDto, SignInDto, SignUpDto } from "@diploma/contracts/auth";
import React, {
  createContext,
  useContext,
  useEffect,
  useEffectEvent,
  useRef,
  useState,
} from "react";
import { QueryClient, QueryClientProvider, useQueryClient } from "@tanstack/react-query";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://diploma.com:5102";
const ACCESS_TOKEN_COOKIE = "diploma_access_token";
const REFRESH_TOKEN_COOKIE = "diploma_refresh_token";
const THEME_STORAGE_KEY = "diploma_theme";

type ThemeMode = "light" | "dark";
type AuthStatus = "loading" | "authenticated" | "guest";

type ThemeContextValue = {
  theme: ThemeMode;
  toggleTheme(): void;
};

type AuthContextValue = {
  api: ApiClient;
  status: AuthStatus;
  user: CurrentUserDto | null;
  signIn(payload: SignInDto): Promise<void>;
  signUp(payload: SignUpDto): Promise<void>;
  signOut(): void;
};

const ThemeContext = createContext<ThemeContextValue | null>(null);
const AuthContext = createContext<AuthContextValue | null>(null);

function readCookie(name: string) {
  if (typeof document === "undefined") {
    return null;
  }

  const value = document.cookie
    .split("; ")
    .find((entry) => entry.startsWith(`${name}=`));

  return value ? decodeURIComponent(value.split("=").slice(1).join("=")) : null;
}

function writeCookie(name: string, value: string, days: number) {
  if (typeof document === "undefined") {
    return;
  }

  const expires = new Date(Date.now() + days * 24 * 60 * 60 * 1000).toUTCString();
  document.cookie = `${name}=${encodeURIComponent(value)}; path=/; expires=${expires}; SameSite=Lax`;
}

function deleteCookie(name: string) {
  if (typeof document === "undefined") {
    return;
  }

  document.cookie = `${name}=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Lax`;
}

function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<ThemeMode>(() => {
    if (typeof window === "undefined") {
      return "light";
    }

    const storedTheme = window.localStorage.getItem(THEME_STORAGE_KEY);
    if (storedTheme === "light" || storedTheme === "dark") {
      return storedTheme;
    }

    return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  });

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    window.localStorage.setItem(THEME_STORAGE_KEY, theme);
  }, [theme]);

  return (
    <ThemeContext.Provider
      value={{
        theme,
        toggleTheme() {
          setTheme((currentTheme) => (currentTheme === "light" ? "dark" : "light"));
        },
      }}
    >
      {children}
    </ThemeContext.Provider>
  );
}

function AuthProvider({ children }: { children: React.ReactNode }) {
  const queryClient = useQueryClient();
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [user, setUser] = useState<CurrentUserDto | null>(null);
  const accessTokenRef = useRef<string | null>(null);
  const refreshTokenRef = useRef<string | null>(null);
  const refreshPromiseRef = useRef<Promise<string | null> | null>(null);
  const [api] = useState(
    () =>
      new ApiClient({
        baseUrl: API_BASE_URL,
        authSession: {
          getAccessToken: async () => accessTokenRef.current,
          refreshAccessToken: async () => refreshAccessTokenInternal(),
          clearAuth: () => clearAuthInternal(),
        },
      }),
  );

  function applyTokens(accessToken: string | null, refreshTokenValue: string | null) {
    accessTokenRef.current = accessToken;
    refreshTokenRef.current = refreshTokenValue;

    if (accessToken) {
      writeCookie(ACCESS_TOKEN_COOKIE, accessToken, 1);
    } else {
      deleteCookie(ACCESS_TOKEN_COOKIE);
    }

    if (refreshTokenValue) {
      writeCookie(REFRESH_TOKEN_COOKIE, refreshTokenValue, 7);
    } else {
      deleteCookie(REFRESH_TOKEN_COOKIE);
    }
  }

  function clearAuthInternal() {
    applyTokens(null, null);
    setUser(null);
    setStatus("guest");
    void queryClient.cancelQueries();
    queryClient.removeQueries({ queryKey: ["current-user"] });
  }

  async function refreshAccessTokenInternal() {
    if (refreshPromiseRef.current) {
      return refreshPromiseRef.current;
    }

    refreshPromiseRef.current = (async () => {
      const currentRefreshToken = refreshTokenRef.current ?? readCookie(REFRESH_TOKEN_COOKIE);
      if (!currentRefreshToken) {
        clearAuthInternal();
        return null;
      }

      try {
        const tokens = await refreshToken(api, currentRefreshToken);
        applyTokens(tokens.token, tokens.refreshToken);
        return tokens.token;
      } catch {
        clearAuthInternal();
        return null;
      } finally {
        refreshPromiseRef.current = null;
      }
    })();

    return refreshPromiseRef.current;
  }

  async function loadCurrentUser() {
    const currentUser = await getCurrentUser(api);
    setUser(currentUser);
    setStatus("authenticated");
  }

  const bootstrapAuth = useEffectEvent(async (isCancelled: () => boolean) => {
    const accessToken = readCookie(ACCESS_TOKEN_COOKIE);
    const refreshTokenValue = readCookie(REFRESH_TOKEN_COOKIE);

    applyTokens(accessToken, refreshTokenValue);

    if (!accessToken && !refreshTokenValue) {
      if (!isCancelled()) {
        setStatus("guest");
      }
      return;
    }

    try {
      if (!accessToken && refreshTokenValue) {
        await refreshAccessTokenInternal();
      }

      await loadCurrentUser();
      if (isCancelled()) {
        return;
      }
    } catch {
      if (!isCancelled()) {
        clearAuthInternal();
      }
    }
  });

  useEffect(() => {
    let cancelled = false;
    void bootstrapAuth(() => cancelled);

    return () => {
      cancelled = true;
    };
  }, []);

  async function handleSignIn(payload: SignInDto) {
    const tokens = await signIn(api, payload);
    applyTokens(tokens.token, tokens.refreshToken);
    await loadCurrentUser();
    await queryClient.invalidateQueries({ queryKey: ["listings"] });
  }

  async function handleSignUp(payload: SignUpDto) {
    await signUp(api, payload);
    await handleSignIn({
      email: payload.email,
      password: payload.password,
    });
  }

  return (
    <AuthContext.Provider
      value={{
        api,
        status,
        user,
        signIn: handleSignIn,
        signUp: handleSignUp,
        signOut: clearAuthInternal,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 30_000,
            refetchOnWindowFocus: false,
          },
        },
      }),
  );

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AuthProvider>{children}</AuthProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error("useTheme must be used inside Providers.");
  }

  return context;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside Providers.");
  }

  return context;
}
