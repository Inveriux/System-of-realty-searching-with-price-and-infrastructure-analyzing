export const colors = {
  light: {
    background: "#f6f3ff",
    backgroundElevated: "#ffffff",
    backgroundMuted: "#ece5ff",
    foreground: "#1d1633",
    foregroundMuted: "#6f6692",
    border: "#d8cdf8",
    primary: "#6d3df2",
    primaryStrong: "#5326d4",
    primarySoft: "#efe8ff",
    accent: "#9f7bff",
    success: "#127a50",
    danger: "#d04b73",
    shadow: "rgba(85, 46, 189, 0.14)",
  },
  dark: {
    background: "#0f1115",
    backgroundElevated: "#151922",
    backgroundMuted: "#1b2130",
    foreground: "#f3f4f8",
    foregroundMuted: "#98a2b3",
    border: "#2a3142",
    primary: "#8b5cf6",
    primaryStrong: "#a78bfa",
    primarySoft: "#201a2e",
    accent: "#c4b5fd",
    success: "#4cd39a",
    danger: "#ff8aac",
    shadow: "rgba(0, 0, 0, 0.34)",
  },
} as const;

export const radii = {
  sm: "12px",
  md: "18px",
  lg: "28px",
  pill: "999px",
} as const;

export const shadows = {
  card: "0 24px 60px var(--shadow-color)",
  glow: "0 0 0 1px color-mix(in srgb, var(--color-primary) 12%, transparent), 0 18px 45px var(--shadow-color)",
} as const;
