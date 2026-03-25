import type { CSSProperties } from "react";

export const themeToggleStyles = {
  button: {
    width: 42,
    height: 42,
    display: "grid",
    placeItems: "center",
    fontSize: "1rem",
  } satisfies CSSProperties,
} as const;
