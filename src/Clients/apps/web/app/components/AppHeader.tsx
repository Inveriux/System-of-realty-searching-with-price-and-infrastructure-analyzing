"use client";

import { useUnreadCount } from "@diploma/features";
import { MessageCircleMore } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { type FormEvent, type MouseEvent as ReactMouseEvent, type TouchEvent, useEffect, useRef, useState } from "react";
import { useAuth } from "../providers";
import { AuthDialog } from "./AuthDialog";
import { ThemeToggle } from "./ThemeToggle";

export function AppHeader() {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const { api, status, user, signOut } = useAuth();
  const unreadCountQuery = useUnreadCount(api, status === "authenticated");
  const [dialogMode, setDialogMode] = useState<"signin" | "signup" | null>(null);
  const [isAccountMenuOpen, setIsAccountMenuOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [hasHydrated, setHasHydrated] = useState(false);
  const accountMenuRef = useRef<HTMLDivElement | null>(null);
  const isFavoritesPage = pathname.startsWith("/favorites");
  const isMessagesPage = pathname.startsWith("/messages");
  const isCreateListingPage = pathname.startsWith("/create-listing");
  const isPricingAnalyticsPage = pathname.startsWith("/analytics/pricing");
  const isListingDetailsPage = /^\/listings\/[^/]+$/.test(pathname);
  const isProfilePage = pathname.startsWith("/profile");
  const unreadCount = hasHydrated && status === "authenticated" ? (unreadCountQuery.data?.count ?? 0) : 0;
  const canRenderAuthState = hasHydrated && status !== "loading";
  const isAuthenticated = canRenderAuthState && status === "authenticated";
  const activeIconButtonStyle = {
    background: "color-mix(in srgb, var(--color-primary-soft) 86%, var(--background-elevated))",
    color: "var(--color-primary-strong)",
    border: "1px solid var(--color-primary-border)",
    boxShadow: "0 10px 22px color-mix(in srgb, var(--shadow-color) 64%, transparent)",
  } as const;
  const idleIconButtonStyle = {
    background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
    color: "inherit",
    border: "1px solid color-mix(in srgb, var(--border-color) 88%, transparent)",
    boxShadow: "0 8px 18px color-mix(in srgb, var(--shadow-color) 54%, transparent)",
  } as const;
  const unreadBadgeStyle = {
    position: "absolute",
    top: -4,
    right: -4,
    minWidth: 19,
    height: 19,
    borderRadius: 999,
    background: "linear-gradient(180deg, var(--color-primary-strong), var(--color-primary-active))",
    color: "var(--text-on-accent)",
    display: "grid",
    placeItems: "center",
    padding: "0 5px",
    fontSize: 11,
    fontWeight: 800,
    border: "1px solid color-mix(in srgb, var(--color-primary-strong) 70%, transparent)",
    boxShadow: "0 8px 16px color-mix(in srgb, var(--shadow-color) 66%, transparent)",
  } as const;

  useEffect(() => {
    setHasHydrated(true);
  }, []);

  function renderAccountAvatar() {
    if (user?.accountImageUrl) {
      return (
        <img
          alt={user.name}
          src={user.accountImageUrl}
          style={{ width: "100%", height: "100%", objectFit: "cover" }}
        />
      );
    }

    if (user?.name) {
      const initials = user.name
        .split(" ")
        .map((part) => part.trim()[0])
        .filter(Boolean)
        .slice(0, 2)
        .join("")
        .toUpperCase();

      return (
        <span
          style={{
            fontWeight: 800,
            fontSize: "0.9rem",
            letterSpacing: "-0.05em",
            color: "var(--color-primary-strong)",
          }}
        >
          {initials}
        </span>
      );
    }

    return <img alt="" src="/icons/account.svg" className="app-header-icon-image" style={{ width: 20, height: 20 }} />;
  }

  useEffect(() => {
    if (!isAccountMenuOpen) {
      return;
    }

    function handlePointerDown(event: globalThis.MouseEvent) {
      if (!accountMenuRef.current?.contains(event.target as Node)) {
        setIsAccountMenuOpen(false);
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setIsAccountMenuOpen(false);
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("keydown", handleEscape);
    };
  }, [isAccountMenuOpen]);

  useEffect(() => {
    setSearchQuery(searchParams.get("q")?.trim() ?? "");
  }, [searchParams]);

  function handleSearchSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const query = searchQuery.trim();
    if (query) {
      router.push(`/listings?q=${encodeURIComponent(query)}`);
      return;
    }

    router.push("/listings");
  }

  function toggleAccountMenu() {
    setIsAccountMenuOpen((current) => !current);
  }

  function handleAccountButtonTouch(event: TouchEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    toggleAccountMenu();
  }

  function handleAccountButtonClick(event: ReactMouseEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    toggleAccountMenu();
  }

  function navigateFromHeader(href: string) {
    router.push(href);
    if (document.documentElement.hasAttribute("data-mobile-client-shell")) {
      const nextUrl = new URL(href, window.location.origin);
      const currentTheme = document.documentElement.dataset.theme;
      nextUrl.searchParams.set("__mobile_theme", currentTheme === "dark" ? "dark" : "light");
      window.location.assign(nextUrl.toString());
    }
  }

  function handlePricingAnalyticsClick(event: ReactMouseEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    navigateFromHeader("/analytics/pricing");
  }

  function handleFavoritesClick(event: ReactMouseEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    navigateFromHeader("/favorites");
  }

  function handlePricingAnalyticsTouch(event: TouchEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    navigateFromHeader("/analytics/pricing");
  }

  function handleFavoritesTouch(event: TouchEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    navigateFromHeader("/favorites");
  }

  function handleMessagesClick(event: ReactMouseEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    navigateFromHeader("/messages");
  }

  function handleMessagesTouch(event: TouchEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();
    navigateFromHeader("/messages");
  }

  return (
    <>
      <header
        className="shell app-header"
        style={{
          position: "sticky",
          top: 12,
          zIndex: 30,
        }}
      >
        <div
          className="surface app-header-surface"
          style={{
            borderRadius: 28,
            padding: "12px 14px",
            display: "grid",
            gap: 10,
            backdropFilter: "blur(18px)",
            background: "color-mix(in srgb, var(--background-elevated) 88%, transparent)",
            border: "1px solid color-mix(in srgb, var(--border-color) 82%, transparent)",
            boxShadow: "0 16px 34px color-mix(in srgb, var(--shadow-color) 72%, transparent)",
          }}
        >
          <div
            className="app-header-top"
            style={{
              display: "flex",
              justifyContent: "space-between",
              flexWrap: "wrap",
              gap: 10,
              alignItems: "center",
            }}
          >
            <Link
              href="/listings"
              className="app-header-brand"
              style={{
                display: "flex",
                alignItems: "center",
                gap: 2,
                minWidth: 0,
                padding: "4px 0",
                borderRadius: 0,
                textDecoration: "none",
                color: "inherit",
                marginLeft: -8,
              }}
            >
              <div
                className="app-header-logo-wrap"
                style={{
                  width: 102,
                  height: 60,
                  borderRadius: 0,
                  overflow: "visible",
                  display: "grid",
                  placeItems: "center",
                }}
              >
                <img
                  alt="Domio"
                  src="/new_icon.png"
                  style={{
                    width: "100%",
                    height: "100%",
                    objectFit: "contain",
                    transform: "translateY(-14%)",
                  }}
                />
              </div>
              <div className="app-header-brand-text" style={{ minWidth: 0, marginLeft: -8 }}>
                <div
                  className="app-header-brand-title"
                  style={{
                    fontFamily: "var(--font-display)",
                    fontSize: "2.64rem",
                    letterSpacing: "-0.07em",
                    fontWeight: 800,
                    lineHeight: 1,
                    minHeight: 60,
                    display: "flex",
                    alignItems: "center",
                  }}
                >
                  Domio
                </div>
              </div>
            </Link>

            {!isCreateListingPage ? (
              <form
                className={`app-header-search${isPricingAnalyticsPage || isListingDetailsPage || isMessagesPage ? " app-header-search-hidden-mobile" : ""}`}
                onSubmit={handleSearchSubmit}
                style={{
                  position: "relative",
                  flex: "1 1 520px",
                  minWidth: "min(100%, 220px)",
                  maxWidth: 760,
                  marginLeft: 28,
                  display: isProfilePage ? "none" : undefined,
                }}
              >
                <img
                  alt=""
                  aria-hidden="true"
                  src="/icons/search.svg"
                  className="app-header-search-icon"
                  style={{
                    position: "absolute",
                    left: 16,
                    top: "50%",
                    width: 18,
                    height: 18,
                    transform: "translateY(-50%)",
                    opacity: 0.62,
                    pointerEvents: "none",
                  }}
                />
                <input
                  className="field"
                  aria-label="Поиск"
                  placeholder="Поиск по объявлениям, адресу или жилому комплексу"
                  value={searchQuery ?? ""}
                  onChange={(event) => setSearchQuery(event.target.value)}
                  style={{
                    borderRadius: 22,
                    minHeight: 52,
                    padding: "0.92rem 1.1rem",
                    paddingLeft: "2.85rem",
                    paddingRight: "1.15rem",
                    fontSize: "1.04rem",
                    fontWeight: 500,
                    background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
                    border: "1px solid color-mix(in srgb, var(--border-color) 88%, transparent)",
                    boxShadow: "0 8px 18px color-mix(in srgb, var(--shadow-color) 56%, transparent)",
                  }}
                />
              </form>
            ) : (
              <div style={{ flex: "1 1 auto" }} />
            )}

            <div className="app-header-actions" style={{ display: "flex", gap: 10, alignItems: "center", flexWrap: "wrap", justifyContent: "flex-end" }}>
              {isAuthenticated ? (
                <Link
                  aria-label="Добавить объявление"
                  href="/create-listing"
                  className="app-header-create-button"
                  style={{
                    minHeight: 42,
                    width: 42,
                    minWidth: 42,
                    justifyContent: "center",
                    padding: 0,
                    display: "inline-flex",
                    alignItems: "center",
                    gap: 0,
                    textDecoration: "none",
                    borderRadius: 14,
                    fontSize: "0.95rem",
                    fontWeight: 700,
                    letterSpacing: "-0.02em",
                    background: isCreateListingPage
                      ? "color-mix(in srgb, var(--color-primary-active) 94%, black)"
                      : "color-mix(in srgb, var(--color-primary-active) 82%, var(--background-elevated))",
                    border: "1px solid color-mix(in srgb, var(--color-primary) 44%, var(--border-color))",
                    color: "var(--text-on-accent)",
                    boxShadow: "0 14px 26px color-mix(in srgb, var(--shadow-color) 74%, transparent)",
                  }}
                  title="Добавить объявление"
                >
                  <span
                    style={{
                      width: 18,
                      height: 18,
                      display: "grid",
                      placeItems: "center",
                      borderRadius: 999,
                      background: "transparent",
                      fontSize: "1.08rem",
                      lineHeight: 1,
                    }}
                  >
                    +
                  </span>
                </Link>
              ) : null}

              <div
                className="app-header-icon-cluster"
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 6,
                  padding: 6,
                  borderRadius: 18,
                  background: "color-mix(in srgb, var(--background-elevated) 92%, var(--background-muted))",
                  border: "1px solid color-mix(in srgb, var(--border-color) 88%, transparent)",
                  boxShadow: "inset 0 0 0 1px color-mix(in srgb, var(--background-elevated) 18%, transparent)",
                }}
              >
                <button
                  aria-label="Сообщения"
                  onClick={handleMessagesClick}
                  onTouchEnd={handleMessagesTouch}
                  type="button"
                  style={{
                    width: 40,
                    height: 40,
                    display: "grid",
                    placeItems: "center",
                    borderRadius: 14,
                    ...(isMessagesPage ? activeIconButtonStyle : idleIconButtonStyle),
                    padding: 0,
                    position: "relative",
                  }}
                  title="Сообщения"
                >
                  <MessageCircleMore size={18} />
                  {unreadCount > 0 ? (
                    <span
                      style={unreadBadgeStyle}
                    >
                      {unreadCount > 99 ? "99+" : unreadCount}
                    </span>
                  ) : null}
                </button>
                <button
                  aria-label="Ценовая аналитика"
                  onClick={handlePricingAnalyticsClick}
                  onTouchEnd={handlePricingAnalyticsTouch}
                  type="button"
                  style={{
                    width: 40,
                    height: 40,
                    display: "grid",
                    placeItems: "center",
                    borderRadius: 14,
                    ...(isPricingAnalyticsPage ? activeIconButtonStyle : idleIconButtonStyle),
                    padding: 0,
                  }}
                  title="Ценовая аналитика"
                >
                  <img alt="" src="/icons/chart.svg" className="app-header-icon-image" style={{ width: 18, height: 18 }} />
                </button>
                <button
                  aria-label="Избранное"
                  onClick={handleFavoritesClick}
                  onTouchEnd={handleFavoritesTouch}
                  type="button"
                  style={{
                    width: 40,
                    height: 40,
                    display: "grid",
                    placeItems: "center",
                    borderRadius: 14,
                    ...(isFavoritesPage ? activeIconButtonStyle : idleIconButtonStyle),
                    padding: 0,
                  }}
                  title="Избранное"
                >
                  <img alt="" src="/icons/favorites.svg" className="app-header-icon-image" style={{ width: 18, height: 18 }} />
                </button>
                <div
                  style={{
                    width: 40,
                    height: 40,
                    display: "grid",
                    placeItems: "center",
                    borderRadius: 14,
                    ...idleIconButtonStyle,
                    overflow: "hidden",
                  }}
                >
                  <ThemeToggle />
                </div>
              </div>

              {isAuthenticated && user ? (
                <div
                  ref={accountMenuRef}
                  className="app-header-account"
                  style={{
                    position: "relative",
                  }}
                >
                  <button
                    aria-label="Аккаунт"
                    aria-expanded={isAccountMenuOpen}
                    aria-haspopup="menu"
                    className="icon-button app-header-account-button"
                    onTouchEnd={handleAccountButtonTouch}
                    onClick={handleAccountButtonClick}
                    type="button"
                    style={{
                      width: 46,
                      height: 46,
                      display: "grid",
                      placeItems: "center",
                      overflow: "hidden",
                      borderRadius: 16,
                      background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
                      border: "1px solid color-mix(in srgb, var(--border-color) 88%, transparent)",
                      boxShadow: "0 10px 20px color-mix(in srgb, var(--shadow-color) 60%, transparent)",
                    }}
                    title={user.name}
                  >
                    {renderAccountAvatar()}
                  </button>

                  {isAccountMenuOpen ? (
                    <div
                      role="menu"
                      aria-label="Меню аккаунта"
                      className="surface app-header-account-menu"
                      style={{
                        position: "absolute",
                        top: "calc(100% + 10px)",
                        right: 0,
                        minWidth: 220,
                        padding: 8,
                        borderRadius: 18,
                        display: "grid",
                        gap: 6,
                        backdropFilter: "blur(18px)",
                        background: "color-mix(in srgb, var(--background-elevated) 94%, transparent)",
                        boxShadow: "0 18px 42px color-mix(in srgb, var(--shadow-color) 78%, transparent)",
                      }}
                    >
                      <div
                        style={{
                          padding: "10px 12px",
                          borderRadius: 14,
                          background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
                        }}
                      >
                        <div
                          style={{
                            fontWeight: 600,
                            lineHeight: 1.2,
                          }}
                        >
                          {user.name}
                        </div>
                        <div
                          className="eyebrow"
                          style={{
                            marginTop: 4,
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                          }}
                        >
                          {user.email}
                        </div>
                      </div>

                      <div
                        className="app-header-account-grid"
                        style={{
                          display: "grid",
                          gridTemplateColumns: "1fr 1fr",
                          gap: 8,
                        }}
                      >
                        <Link
                          href="/profile/listings"
                          onClick={() => setIsAccountMenuOpen(false)}
                          role="menuitem"
                          style={{
                            display: "grid",
                            gap: 4,
                            padding: "12px 12px",
                            borderRadius: 14,
                            textDecoration: "none",
                            color: "inherit",
                            background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
                          }}
                        >
                          <div className="eyebrow">Раздел</div>
                          <div style={{ fontWeight: 700, lineHeight: 1.2 }}>Мои объявления</div>
                        </Link>

                        <Link
                          href="/messages"
                          onClick={() => setIsAccountMenuOpen(false)}
                          role="menuitem"
                          style={{
                            display: "grid",
                            gap: 4,
                            padding: "12px 12px",
                            borderRadius: 14,
                            textDecoration: "none",
                            color: "inherit",
                            background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
                            position: "relative",
                          }}
                        >
                          <div className="eyebrow">Раздел</div>
                          <div style={{ fontWeight: 700, lineHeight: 1.2 }}>Сообщения</div>
                          {unreadCount > 0 ? (
                            <span
                              style={{
                                ...unreadBadgeStyle,
                                top: 10,
                                right: 10,
                              }}
                            >
                              {unreadCount > 99 ? "99+" : unreadCount}
                            </span>
                          ) : null}
                        </Link>

                        <Link
                          href="/favorites"
                          onClick={() => setIsAccountMenuOpen(false)}
                          role="menuitem"
                          style={{
                            display: "grid",
                            gap: 4,
                            padding: "12px 12px",
                            borderRadius: 14,
                            textDecoration: "none",
                            color: "inherit",
                            background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
                          }}
                        >
                          <div className="eyebrow">Раздел</div>
                          <div style={{ fontWeight: 700, lineHeight: 1.2 }}>Избранное</div>
                        </Link>

                        <Link
                          href="/create-listing"
                          onClick={() => setIsAccountMenuOpen(false)}
                          role="menuitem"
                          style={{
                            gridColumn: "1 / -1",
                            display: "grid",
                            gap: 4,
                            padding: "12px 12px",
                            borderRadius: 14,
                            textDecoration: "none",
                            color: "var(--color-primary-strong)",
                            background: "color-mix(in srgb, var(--color-primary-soft) 78%, var(--background-elevated))",
                            border: "1px solid var(--color-primary-border)",
                          }}
                        >
                          <div className="eyebrow">Действие</div>
                          <div style={{ fontWeight: 800, lineHeight: 1.2 }}>Добавить объявление</div>
                        </Link>
                      </div>

                      <Link
                        href="/profile"
                        onClick={() => setIsAccountMenuOpen(false)}
                        role="menuitem"
                        style={{
                          padding: "10px 12px",
                          borderRadius: 14,
                          textDecoration: "none",
                          color: "inherit",
                        }}
                      >
                        Профиль
                      </Link>

                      <button
                        onClick={() => {
                          setIsAccountMenuOpen(false);
                          signOut();
                        }}
                        role="menuitem"
                        type="button"
                        style={{
                          padding: "10px 12px",
                          borderRadius: 14,
                          border: "none",
                          textAlign: "left",
                          font: "inherit",
                          color: "var(--color-danger-strong)",
                          background: "transparent",
                          cursor: "pointer",
                        }}
                      >
                        Выйти
                      </button>
                    </div>
                  ) : null}
                </div>
              ) : canRenderAuthState ? (
                <button
                  aria-label="Аккаунт"
                  className="icon-button app-header-account-button"
                  onClick={(event) => {
                    event.stopPropagation();
                    setDialogMode("signin");
                  }}
                  type="button"
                  style={{
                    width: 46,
                    height: 46,
                    display: "grid",
                    placeItems: "center",
                    borderRadius: 16,
                    background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
                    border: "1px solid color-mix(in srgb, var(--border-color) 88%, transparent)",
                    boxShadow: "0 10px 20px color-mix(in srgb, var(--shadow-color) 60%, transparent)",
                  }}
                >
                  <img alt="" src="/icons/account.svg" className="app-header-icon-image" style={{ width: 20, height: 20 }} />
                </button>
              ) : (
                <div
                  aria-hidden="true"
                  className="app-header-account-button"
                  style={{
                    width: 46,
                    height: 46,
                    borderRadius: 16,
                    background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
                    border: "1px solid color-mix(in srgb, var(--border-color) 88%, transparent)",
                    boxShadow: "0 10px 20px color-mix(in srgb, var(--shadow-color) 60%, transparent)",
                    opacity: 0,
                    pointerEvents: "none",
                  }}
                />
              )}
            </div>
          </div>
        </div>
      </header>

      <AuthDialog mode={dialogMode} onClose={() => setDialogMode(null)} />
    </>
  );
}
