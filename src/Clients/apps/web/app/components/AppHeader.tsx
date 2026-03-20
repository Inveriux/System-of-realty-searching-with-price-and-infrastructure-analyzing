"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { useAuth, useTheme } from "../providers";
import { AuthDialog } from "./AuthDialog";
import { ThemeToggle } from "./ThemeToggle";

export function AppHeader() {
  const pathname = usePathname();
  const { status, user, signOut } = useAuth();
  const { theme } = useTheme();
  const [dialogMode, setDialogMode] = useState<"signin" | "signup" | null>(null);
  const [isAccountMenuOpen, setIsAccountMenuOpen] = useState(false);
  const accountMenuRef = useRef<HTMLDivElement | null>(null);
  const isFavoritesPage = pathname.startsWith("/favorites");
  const isCreateListingPage = pathname.startsWith("/create-listing");
  const iconFilter = theme === "dark" ? "brightness(0) invert(1)" : "none";

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
            borderRadius: 22,
            padding: "12px 16px",
            display: "grid",
            gap: 12,
            backdropFilter: "blur(18px)",
            background: "color-mix(in srgb, var(--background-elevated) 88%, transparent)",
          }}
        >
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              flexWrap: "wrap",
              gap: 14,
              alignItems: "center",
            }}
          >
            <Link
              href="/listings"
              style={{
                display: "flex",
                alignItems: "center",
                gap: 12,
                minWidth: 0,
              }}
            >
              <div
                style={{
                  width: 38,
                  height: 38,
                  borderRadius: 14,
                  display: "grid",
                  placeItems: "center",
                  background:
                    "linear-gradient(135deg, var(--color-primary) 0%, color-mix(in srgb, var(--color-accent) 75%, white) 100%)",
                  color: "white",
                  fontWeight: 700,
                  fontFamily: "var(--font-display)",
                  fontSize: "1.1rem",
                }}
              >
                R
              </div>
              <div style={{ minWidth: 0 }}>
                <div
                  style={{
                    fontFamily: "var(--font-display)",
                    fontSize: "1.15rem",
                    letterSpacing: "-0.04em",
                    fontWeight: 700,
                  }}
                >
                  Realt
                </div>
                <div className="eyebrow" style={{ marginTop: 2 }}>
                  Недвижимость
                </div>
              </div>
            </Link>

            {!isCreateListingPage ? (
              <div
                style={{
                  position: "relative",
                  flex: "1 1 420px",
                  minWidth: "min(100%, 220px)",
                  maxWidth: 620,
                }}
              >
                <input
                  className="field"
                  aria-label="Поиск"
                  placeholder="Поиск по объявлениям, адресу или жилому комплексу"
                  style={{
                    borderRadius: 999,
                    padding: "0.8rem 1.1rem",
                    paddingLeft: "1.2rem",
                    background: "color-mix(in srgb, var(--background) 55%, var(--background-elevated))",
                  }}
                />
              </div>
            ) : (
              <div style={{ flex: "1 1 auto" }} />
            )}

            <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
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
                      width: 42,
                      height: 42,
                      display: "grid",
                      placeItems: "center",
                    }}
                    title={user.name}
                  >
                    <img alt="" src="/icons/account.svg" style={{ width: 20, height: 20, filter: iconFilter }} />
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
                  onClick={() => setDialogMode("signin")}
                  type="button"
                  style={{
                    width: 42,
                    height: 42,
                    display: "grid",
                    placeItems: "center",
                  }}
                >
                  <img alt="" src="/icons/account.svg" style={{ width: 20, height: 20, filter: iconFilter }} />
                </button>
              )}

              <Link
                aria-label="Избранное"
                className="icon-button"
                href="/favorites"
                style={{
                  width: 42,
                  height: 42,
                  display: "grid",
                  placeItems: "center",
                  background: isFavoritesPage ? "var(--color-primary-soft)" : undefined,
                  color: isFavoritesPage ? "var(--color-primary-strong)" : undefined,
                }}
              >
                <img alt="" src="/icons/favorites.svg" style={{ width: 20, height: 20, filter: iconFilter }} />
              </Link>
              <ThemeToggle />
            </div>
          </div>
        </div>
      </header>

      <AuthDialog mode={dialogMode} onClose={() => setDialogMode(null)} />
    </>
  );
}
