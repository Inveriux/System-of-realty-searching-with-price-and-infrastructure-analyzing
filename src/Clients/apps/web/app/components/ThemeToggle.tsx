"use client";

import { useTheme } from "../providers";
import { themeToggleStyles } from "./theme-toggle.styles";

export function ThemeToggle() {
  const { toggleTheme } = useTheme();

  return (
    <button
      aria-label="Переключить тему"
      className="icon-button theme-toggle-button"
      onClick={toggleTheme}
      style={themeToggleStyles.button}
      type="button"
    >
      ◐
    </button>
  );
}
