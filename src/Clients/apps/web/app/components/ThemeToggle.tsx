"use client";

import { useTheme } from "../providers";

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme();

  return (
    <button
      aria-label="Переключить тему"
      className="icon-button"
      onClick={toggleTheme}
      style={{
        width: 42,
        height: 42,
        display: "grid",
        placeItems: "center",
        fontSize: "1rem",
      }}
      type="button"
    >
      {theme === "light" ? "◐" : "◑"}
    </button>
  );
}
