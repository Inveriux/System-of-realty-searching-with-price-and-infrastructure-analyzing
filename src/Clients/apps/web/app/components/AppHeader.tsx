"use client";

import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { type FormEvent, useEffect, useRef, useState } from "react";
import { useAuth, useTheme } from "../providers";
import { AuthDialog } from "./AuthDialog";
import { ThemeToggle } from "./ThemeToggle";

export function AppHeader() {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const { status, user, signOut } = useAuth();
  const { theme } = useTheme();
  const [dialogMode, setDialogMode] = useState<"signin" | "signup" | null>(null);
  const [isAccountMenuOpen, setIsAccountMenuOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const accountMenuRef = useRef<HTMLDivElement | null>(null);
  const isFavoritesPage = pathname.startsWith("/favorites");
  const isCreateListingPage = pathname.startsWith("/create-listing");
  const iconFilter = theme === "dark" ? "brightness(0) invert(1)" : "none";

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

    return <img alt="" src="/icons/account.svg" style={{ width: 20, height: 20, filter: iconFilter }} />;
  }

  useEffect(() => {
    if (!isAccountMenuOpen) {
      return;
    }

    function handlePointerDown(event: MouseEvent) {
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

  return (
    <>
      <header
        className="shell"
        style={{
          position: "sticky",
          top: 12,
          zIndex: 30,
        }}
      >
        <div
          className="surface"
          style={{
            borderRadius: 28,
            padding: "12px 14px",
            display: "grid",
            gap: 10,
            backdropFilter: "blur(18px)",
            background: "color-mix(in srgb, var(--background-elevated) 90%, transparent)",
            border: "1px solid color-mix(in srgb, var(--border-color) 70%, transparent)",
            boxShadow: "0 18px 42px rgba(15, 23, 42, 0.08)",
          }}
        >
          <div
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
              style={{
                display: "flex",
                alignItems: "center",
                gap: 10,
                minWidth: 0,
                padding: "8px 12px",
                borderRadius: 22,
                textDecoration: "none",
                color: "inherit",
                background: "color-mix(in srgb, var(--background) 76%, transparent)",
                border: "1px solid color-mix(in srgb, var(--border-color) 60%, transparent)",
                boxShadow: "inset 0 1px 0 rgba(255,255,255,0.24)",
              }}
            >
              <div
                style={{
                  width: 78,
                  height: 44,
                  borderRadius: 16,
                  overflow: "visible",
                  display: "grid",
                  placeItems: "center",
                  background: "linear-gradient(180deg, color-mix(in srgb, var(--color-primary-soft) 84%, white), color-mix(in srgb, var(--background) 70%, white))",
                }}
              >
                <img
                  alt="Realt"
                  src="/header-logo.png"
                  style={{
                    width: "100%",
                    height: "100%",
                    objectFit: "contain",
                    transform: "translateY(-20%)",
                  }}
                />
              </div>
              <div style={{ minWidth: 0 }}>
                <div
                  style={{
                    fontFamily: "var(--font-display)",
                    fontSize: "1.22rem",
                    letterSpacing: "-0.07em",
                    fontWeight: 800,
                    lineHeight: 1,
                  }}
                >
                  Realt
                </div>
                <div
                  className="eyebrow"
                  style={{
                    marginTop: 4,
                    fontSize: "0.66rem",
                    letterSpacing: "0.12em",
                    color: "var(--foreground-muted)",
                  }}
                >
                  Недвижимость
                </div>
              </div>
            </Link>

            {!isCreateListingPage ? (
              <form
                onSubmit={handleSearchSubmit}
                style={{
                  position: "relative",
                  flex: "1 1 520px",
                  minWidth: "min(100%, 220px)",
                  maxWidth: 760,
                }}
              >
                <img
                  alt=""
                  aria-hidden="true"
                  src="/icons/search.svg"
                  style={{
                    position: "absolute",
                    left: 16,
                    top: "50%",
                    width: 18,
                    height: 18,
                    transform: "translateY(-50%)",
                    filter: iconFilter,
                    opacity: 0.62,
                    pointerEvents: "none",
                  }}
                />
                <input
                  className="field"
                  aria-label="Поиск"
                  placeholder="Поиск по объявлениям, адресу или жилому комплексу"
                  value={searchQuery}
                  onChange={(event) => setSearchQuery(event.target.value)}
                  style={{
                    borderRadius: 22,
                    minHeight: 52,
                    padding: "0.92rem 1.1rem",
                    paddingLeft: "2.85rem",
                    paddingRight: "1.15rem",
                    fontSize: "0.98rem",
                    fontWeight: 500,
                    background: "color-mix(in srgb, var(--background) 84%, var(--background-elevated))",
                    border: "1px solid color-mix(in srgb, var(--border-color) 72%, transparent)",
                    boxShadow: "0 10px 24px rgba(15, 23, 42, 0.05), inset 0 1px 0 rgba(255,255,255,0.38)",
                  }}
                />
              </form>
            ) : (
              <div style={{ flex: "1 1 auto" }} />
            )}

            <div style={{ display: "flex", gap: 10, alignItems: "center", flexWrap: "wrap", justifyContent: "flex-end" }}>
              {status === "authenticated" ? (
                <Link
                  aria-label="Добавить объявление"
                  href="/create-listing"
                  style={{
                    minHeight: 46,
                    padding: "0 16px",
                    display: "inline-flex",
                    alignItems: "center",
                    gap: 10,
                    textDecoration: "none",
                    borderRadius: 18,
                    fontSize: "0.95rem",
                    fontWeight: 700,
                    letterSpacing: "-0.02em",
                    background: isCreateListingPage
                      ? "linear-gradient(180deg, color-mix(in srgb, var(--color-primary-strong) 86%, black), color-mix(in srgb, var(--color-primary) 86%, black))"
                      : "linear-gradient(180deg, color-mix(in srgb, var(--color-primary) 84%, white), color-mix(in srgb, var(--color-primary-strong) 88%, black))",
                    color: "white",
                    boxShadow: "0 14px 26px color-mix(in srgb, var(--color-primary) 28%, transparent)",
                  }}
                  title="Добавить объявление"
                >
                  <span
                    style={{
                      width: 20,
                      height: 20,
                      display: "grid",
                      placeItems: "center",
                      borderRadius: 999,
                      background: "rgba(255,255,255,0.18)",
                      fontSize: "1rem",
                      lineHeight: 1,
                    }}
                  >
                    +
                  </span>
                  <span>Добавить</span>
                </Link>
              ) : null}

              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 6,
                  padding: 6,
                  borderRadius: 18,
                  background: "color-mix(in srgb, var(--background) 82%, transparent)",
                  border: "1px solid color-mix(in srgb, var(--border-color) 64%, transparent)",
                }}
              >
                <Link
                  aria-label="Ценовая аналитика"
                  href="/analytics/pricing"
                  style={{
                    width: 40,
                    height: 40,
                    display: "grid",
                    placeItems: "center",
                    textDecoration: "none",
                    borderRadius: 14,
                    background: "color-mix(in srgb, var(--background-elevated) 88%, white)",
                    color: "inherit",
                    boxShadow: "0 6px 18px rgba(15, 23, 42, 0.05)",
                  }}
                  title="Ценовая аналитика"
                >
                  <img alt="" src="/icons/chart.svg" style={{ width: 18, height: 18, filter: iconFilter }} />
                </Link>
                <Link
                  aria-label="Избранное"
                  href="/favorites"
                  style={{
                    width: 40,
                    height: 40,
                    display: "grid",
                    placeItems: "center",
                    textDecoration: "none",
                    borderRadius: 14,
                    background: isFavoritesPage
                      ? "color-mix(in srgb, var(--color-primary-soft) 92%, white)"
                      : "color-mix(in srgb, var(--background-elevated) 88%, white)",
                    color: isFavoritesPage ? "var(--color-primary-strong)" : "inherit",
                    boxShadow: "0 6px 18px rgba(15, 23, 42, 0.05)",
                  }}
                  title="Избранное"
                >
                  <img alt="" src="/icons/favorites.svg" style={{ width: 18, height: 18, filter: iconFilter }} />
                </Link>
                <div
                  style={{
                    width: 40,
                    height: 40,
                    display: "grid",
                    placeItems: "center",
                    borderRadius: 14,
                    background: "color-mix(in srgb, var(--background-elevated) 88%, white)",
                    boxShadow: "0 6px 18px rgba(15, 23, 42, 0.05)",
                  }}
                >
                  <ThemeToggle />
                </div>
              </div>

              {status === "authenticated" && user ? (
                <div
                  ref={accountMenuRef}
                  style={{
                    position: "relative",
                  }}
                >
                  <button
                    aria-label="Аккаунт"
                    aria-expanded={isAccountMenuOpen}
                    aria-haspopup="menu"
                    className="icon-button"
                    onClick={() => setIsAccountMenuOpen((current) => !current)}
                    type="button"
                    style={{
                      width: 46,
                      height: 46,
                      display: "grid",
                      placeItems: "center",
                      overflow: "hidden",
                      borderRadius: 16,
                      background: "color-mix(in srgb, var(--background) 84%, transparent)",
                      border: "1px solid color-mix(in srgb, var(--border-color) 68%, transparent)",
                      boxShadow: "0 10px 24px rgba(15, 23, 42, 0.06)",
                    }}
                    title={user.name}
                  >
                    {renderAccountAvatar()}
                  </button>

                  {isAccountMenuOpen ? (
                    <div
                      role="menu"
                      aria-label="Меню аккаунта"
                      className="surface"
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
                        background: "color-mix(in srgb, var(--background-elevated) 92%, transparent)",
                        boxShadow: "0 18px 50px rgba(15, 23, 42, 0.16)",
                      }}
                    >
                      <div
                        style={{
                          padding: "10px 12px",
                          borderRadius: 14,
                          background: "color-mix(in srgb, var(--background) 72%, transparent)",
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
                            background: "color-mix(in srgb, var(--background) 72%, transparent)",
                          }}
                        >
                          <div className="eyebrow">Раздел</div>
                          <div style={{ fontWeight: 700, lineHeight: 1.2 }}>Мои объявления</div>
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
                            background: "color-mix(in srgb, var(--background) 72%, transparent)",
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
                            background: "color-mix(in srgb, var(--color-primary-soft) 74%, var(--background-elevated))",
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
                          color: "var(--color-danger, #b42318)",
                          background: "transparent",
                          cursor: "pointer",
                        }}
                      >
                        Выйти
                      </button>
                    </div>
                  ) : null}
                </div>
              ) : (
                <button
                  aria-label="Аккаунт"
                  className="icon-button"
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
                    background: "color-mix(in srgb, var(--background) 84%, transparent)",
                    border: "1px solid color-mix(in srgb, var(--border-color) 68%, transparent)",
                    boxShadow: "0 10px 24px rgba(15, 23, 42, 0.06)",
                  }}
                >
                  <img alt="" src="/icons/account.svg" style={{ width: 20, height: 20, filter: iconFilter }} />
                </button>
              )}
            </div>
          </div>
        </div>
      </header>

      <AuthDialog mode={dialogMode} onClose={() => setDialogMode(null)} />
    </>
  );
}
