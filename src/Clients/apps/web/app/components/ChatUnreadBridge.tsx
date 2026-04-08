"use client";

import { useUnreadCount } from "@diploma/features";
import { useEffect } from "react";
import { useAuth } from "../providers";

export function ChatUnreadBridge() {
  const { api, status } = useAuth();
  const unreadQuery = useUnreadCount(api, status === "authenticated");
  const count = status === "authenticated" ? (unreadQuery.data?.count ?? 0) : 0;

  useEffect(() => {
    if (typeof window === "undefined" || !window.ReactNativeWebView?.postMessage) {
      return;
    }

    window.ReactNativeWebView.postMessage(JSON.stringify({
      type: "chat-unread-count",
      payload: count,
    }));
  }, [count]);

  return null;
}
