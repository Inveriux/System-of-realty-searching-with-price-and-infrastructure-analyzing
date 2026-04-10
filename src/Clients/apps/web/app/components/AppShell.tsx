"use client";

import { Suspense } from "react";
import { AppHeader } from "./AppHeader";
import { ChatUnreadBridge } from "./ChatUnreadBridge";
import { appShellStyles } from "./app-shell.styles";

export function AppShell({
  children,
  lockScroll = false,
}: {
  children: React.ReactNode;
  lockScroll?: boolean;
}) {
  return (
    <div
      className="app-shell-root"
      style={lockScroll ? appShellStyles.lockedRoot : appShellStyles.root}
    >
      <ChatUnreadBridge />
      <Suspense fallback={null}>
        <AppHeader />
      </Suspense>
      <div
        className="shell app-shell-content"
        style={lockScroll ? appShellStyles.lockedContent : appShellStyles.content}
      >
        {children}
      </div>
    </div>
  );
}
