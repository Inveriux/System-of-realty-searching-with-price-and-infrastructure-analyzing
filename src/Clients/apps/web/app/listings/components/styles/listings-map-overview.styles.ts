import type { CSSProperties } from "react";

export const listingsMapOverviewStyles = {
  backdrop: {
    position: "fixed",
    inset: 0,
    zIndex: 80,
    background: "rgba(10, 7, 20, 0.5)",
  } satisfies CSSProperties,
  panel: {
    width: "100%",
    height: "100%",
    borderRadius: 0,
    padding: 18,
    display: "grid",
    gap: 14,
    gridTemplateRows: "auto minmax(0, 1fr)",
  } satisfies CSSProperties,
  header: {
    display: "flex",
    justifyContent: "space-between",
    gap: 16,
    alignItems: "center",
  } satisfies CSSProperties,
  title: {
    margin: 0,
  } satisfies CSSProperties,
  headerActions: {
    display: "flex",
    gap: 10,
    alignItems: "center",
    flexWrap: "wrap",
    justifyContent: "flex-end",
  } satisfies CSSProperties,
  helpText: {
    color: "var(--foreground-muted)",
    fontSize: 14,
    maxWidth: 420,
    textAlign: "right",
  } satisfies CSSProperties,
  mapCanvas: {
    minHeight: 0,
  } satisfies CSSProperties,
} as const;
