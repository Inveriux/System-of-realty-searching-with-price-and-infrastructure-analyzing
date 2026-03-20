"use client";

import { AppHeader } from "./AppHeader";

export function AppShell({ children }: { children: React.ReactNode }) {
  return (
    <div style={{ padding: "24px 0 40px" }}>
      <AppHeader />
      <div className="shell" style={{ marginTop: 24 }}>
        {children}
      </div>
    </div>
  );
}
