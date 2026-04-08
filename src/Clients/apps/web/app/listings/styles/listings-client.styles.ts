import type { CSSProperties } from "react";

export const listingsClientStyles = {
  pageTitle: {
    marginTop: 0,
  } satisfies CSSProperties,
  heroGrid: {
    display: "grid",
    gap: 16,
    gridTemplateColumns: "minmax(0, 1.35fr) minmax(280px, 0.9fr)",
    alignItems: "stretch",
  } satisfies CSSProperties,
  heroPanel: {
    borderRadius: 26,
    padding: 22,
    background:
      "linear-gradient(135deg, color-mix(in srgb, var(--background-elevated) 97%, white), color-mix(in srgb, var(--background-muted) 90%, white))",
    border: "1px solid color-mix(in srgb, var(--border-color) 88%, transparent)",
    display: "grid",
    gap: 14,
  } satisfies CSSProperties,
  heroCopy: {
    display: "grid",
    gap: 8,
  } satisfies CSSProperties,
  heroTitle: {
    fontSize: "clamp(1.2rem, 2vw, 1.8rem)",
    fontWeight: 900,
    lineHeight: 1.05,
  } satisfies CSSProperties,
  mutedParagraph: {
    margin: 0,
    color: "var(--foreground-muted)",
    lineHeight: 1.6,
  } satisfies CSSProperties,
  heroActions: {
    display: "flex",
    gap: 12,
    flexWrap: "wrap",
    alignItems: "center",
  } satisfies CSSProperties,
  summaryPanel: {
    borderRadius: 26,
    padding: 20,
    display: "grid",
    gap: 12,
    alignContent: "start",
  } satisfies CSSProperties,
  summaryTags: {
    display: "flex",
    gap: 10,
    flexWrap: "wrap",
  } satisfies CSSProperties,
  filterTag: {
    borderRadius: 999,
    padding: "8px 12px",
    fontSize: 13,
  } satisfies CSSProperties,
  headingTitle: {
    marginTop: 8,
  } satisfies CSSProperties,
  metaText: {
    margin: 0,
    color: "var(--foreground-muted)",
  } satisfies CSSProperties,
  footerRow: {
    display: "flex",
    gap: 12,
    alignItems: "center",
    justifyContent: "space-between",
    flexWrap: "wrap",
  } satisfies CSSProperties,
  footerMeta: {
    color: "var(--foreground-muted)",
    fontSize: 14,
  } satisfies CSSProperties,
  footerActions: {
    display: "flex",
    gap: 10,
    alignItems: "center",
    flexWrap: "wrap",
  } satisfies CSSProperties,
  pageSizeSelect: {
    width: 120,
  } satisfies CSSProperties,
  floatingMapButton: {
    position: "fixed",
    zIndex: 24,
    borderRadius: 24,
    padding: "18px 16px",
    display: "grid",
    gap: 10,
    justifyItems: "center",
    minWidth: 96,
    border: "1px solid var(--border-color)",
    background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
  } satisfies CSSProperties,
  floatingMapIcon: {
    fontSize: "1.9rem",
    lineHeight: 1,
  } satisfies CSSProperties,
  floatingMapLabel: {
    fontSize: 13,
    fontWeight: 800,
    lineHeight: 1.2,
  } satisfies CSSProperties,
  modalPanel: {
    width: "min(1180px, 100%)",
    maxHeight: "calc(100vh - 48px)",
    display: "grid",
    gridTemplateRows: "auto",
    gap: 10,
    overflow: "auto",
  } satisfies CSSProperties,
  modalHeader: {
    display: "flex",
    gap: 16,
    justifyContent: "space-between",
    alignItems: "start",
    flexWrap: "wrap",
  } satisfies CSSProperties,
  modalHeading: {
    marginTop: 0,
    marginBottom: 0,
  } satisfies CSSProperties,
  mapControls: {
    display: "flex",
    gap: 12,
    flexWrap: "wrap",
    alignItems: "center",
  } satisfies CSSProperties,
  radiusInput: {
    minWidth: 260,
    maxWidth: 320,
    width: "100%",
  } satisfies CSSProperties,
  modalFooter: {
    display: "flex",
    gap: 12,
    justifyContent: "space-between",
    flexWrap: "wrap",
  } satisfies CSSProperties,
  modalFooterActions: {
    display: "flex",
    gap: 12,
    flexWrap: "wrap",
  } satisfies CSSProperties,
} as const;
