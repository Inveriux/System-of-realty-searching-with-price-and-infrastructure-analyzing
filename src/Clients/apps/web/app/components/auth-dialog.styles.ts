import type { CSSProperties } from "react";

export const authDialogStyles = {
  backdrop: {
    position: "fixed",
    inset: 0,
    background: "rgba(10, 7, 20, 0.62)",
    display: "grid",
    placeItems: "center",
    padding: 16,
    zIndex: 50,
  } satisfies CSSProperties,
  panel: {
    width: "min(100%, 460px)",
    borderRadius: 28,
    padding: 24,
  } satisfies CSSProperties,
  title: {
    marginTop: 8,
  } satisfies CSSProperties,
  modeSwitch: {
    display: "grid",
    gridTemplateColumns: "1fr 1fr",
    gap: 8,
    marginTop: 18,
    marginBottom: 18,
    padding: 6,
    borderRadius: 20,
    background: "var(--background-muted)",
    border: "1px solid var(--border-color)",
  } satisfies CSSProperties,
  modeButton: {
    border: "none",
    borderRadius: 14,
    padding: "10px 14px",
    font: "inherit",
    fontWeight: 700,
    cursor: "pointer",
  } satisfies CSSProperties,
  modeButtonActive: {
    background: "var(--background-elevated)",
    color: "var(--foreground)",
    boxShadow: "0 10px 24px rgba(15, 23, 42, 0.08)",
  } satisfies CSSProperties,
  modeButtonIdle: {
    background: "transparent",
    color: "var(--foreground-muted)",
    boxShadow: "none",
  } satisfies CSSProperties,
  googleButton: {
    width: "100%",
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    gap: 12,
    marginBottom: 18,
    borderRadius: 18,
    border: "1px solid var(--border-color)",
    background: "var(--background-elevated)",
    color: "var(--foreground)",
    padding: "13px 16px",
    font: "inherit",
    fontWeight: 700,
    cursor: "pointer",
  } satisfies CSSProperties,
  googleBadge: {
    display: "grid",
    placeItems: "center",
    width: 34,
    height: 34,
    borderRadius: 999,
    background: "color-mix(in srgb, var(--background-elevated) 94%, var(--background-muted))",
    color: "var(--color-primary-strong)",
    fontSize: 20,
    fontWeight: 900,
  } satisfies CSSProperties,
  form: {
    display: "grid",
    gap: 14,
  } satisfies CSSProperties,
  hiddenFileInput: {
    display: "none",
  } satisfies CSSProperties,
  uploadLabel: {
    display: "grid",
    gap: 6,
    borderRadius: 18,
    border: "1px dashed var(--border-color)",
    background: "var(--background-elevated)",
    padding: "14px 16px",
    color: "var(--foreground)",
    cursor: "pointer",
  } satisfies CSSProperties,
  uploadTitle: {
    fontWeight: 700,
  } satisfies CSSProperties,
  uploadMeta: {
    color: "var(--foreground-muted)",
    fontSize: 14,
  } satisfies CSSProperties,
  fieldGroup: {
    display: "grid",
    gap: 6,
  } satisfies CSSProperties,
  fieldErrorText: {
    fontSize: 12,
    lineHeight: 1.35,
    color: "var(--color-danger)",
    paddingLeft: 2,
  } satisfies CSSProperties,
  error: {
    borderRadius: 18,
    padding: "12px 14px",
    background: "color-mix(in srgb, var(--color-danger) 12%, var(--background-elevated))",
    border: "1px solid color-mix(in srgb, var(--color-danger) 36%, var(--border-color))",
    color: "var(--foreground)",
  } satisfies CSSProperties,
  actions: {
    display: "flex",
    gap: 10,
    justifyContent: "flex-end",
  } satisfies CSSProperties,
} as const;

export function getAuthDialogModeButtonStyle(active: boolean) {
  return {
    ...authDialogStyles.modeButton,
    ...(active ? authDialogStyles.modeButtonActive : authDialogStyles.modeButtonIdle),
  };
}
