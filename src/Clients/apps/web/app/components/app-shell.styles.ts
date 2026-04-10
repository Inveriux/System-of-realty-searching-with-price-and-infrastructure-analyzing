import type { CSSProperties } from "react";

export const appShellStyles = {
  root: {
    padding: "24px 0 40px",
  } satisfies CSSProperties,
  content: {
    marginTop: 24,
  } satisfies CSSProperties,
  lockedRoot: {
    padding: "24px 0 0",
    height: "100dvh",
    minHeight: "100dvh",
    overflow: "hidden",
    display: "grid",
    gridTemplateRows: "auto minmax(0, 1fr)",
  } satisfies CSSProperties,
  lockedContent: {
    marginTop: 24,
    minHeight: 0,
    overflow: "hidden",
  } satisfies CSSProperties,
} as const;
