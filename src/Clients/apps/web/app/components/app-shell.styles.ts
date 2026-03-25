import type { CSSProperties } from "react";

export const appShellStyles = {
  root: {
    padding: "24px 0 40px",
  } satisfies CSSProperties,
  content: {
    marginTop: 24,
  } satisfies CSSProperties,
} as const;
