"use client";

import { useTheme } from "../providers";
import { themeToggleStyles } from "./theme-toggle.styles";

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme();

  return (
    <button
      aria-label="Переключить тему"
      className="icon-button"
      onClick={toggleTheme}
      style={themeToggleStyles.button}
      type="button"
    >
      {theme === "light" ? "◐" : "◑"}
    </button>
  );
}
