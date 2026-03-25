"use client";

import { AppHeader } from "./AppHeader";
import { appShellStyles } from "./app-shell.styles";

export function AppShell({ children }: { children: React.ReactNode }) {
  return (
    <div style={appShellStyles.root}>
      <AppHeader />
      <div className="shell" style={appShellStyles.content}>
        {children}
      </div>
    </div>
  );
}
