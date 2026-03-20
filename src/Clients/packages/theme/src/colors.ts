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
    background: "#120d1f",
    backgroundElevated: "#1b1430",
    backgroundMuted: "#241a40",
    foreground: "#f5f1ff",
    foregroundMuted: "#b6abd8",
    border: "#3a2f61",
    primary: "#9f7bff",
    primaryStrong: "#ba9eff",
    primarySoft: "#2a1f46",
    accent: "#d6c7ff",
    success: "#4cd39a",
    danger: "#ff8aac",
    shadow: "rgba(12, 7, 24, 0.45)",
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
  glow: "0 0 0 1px color-mix(in srgb, var(--color-primary) 20%, transparent), 0 18px 45px var(--shadow-color)",
} as const;
