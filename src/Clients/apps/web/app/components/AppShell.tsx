"use client";

import { Suspense } from "react";
import { AppHeader } from "./AppHeader";
import { appShellStyles } from "./app-shell.styles";

export function AppShell({ children }: { children: React.ReactNode }) {
  return (
    <div className="app-shell-root" style={appShellStyles.root}>
      <Suspense fallback={null}>
        <AppHeader />
      </Suspense>
      <div className="shell app-shell-content" style={appShellStyles.content}>
        {children}
      </div>
    </div>
  );
}
