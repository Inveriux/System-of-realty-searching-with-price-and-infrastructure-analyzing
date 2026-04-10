"use client";

import { markConversationAsRead, startSupportConversation } from "@diploma/api/chat";
import type {
  ConversationDetailsDto,
  ConversationListItemDto,
  ConversationType,
  MessageDto,
  MessagesReadDto,
} from "@diploma/contracts/chat";
import {
  useConversationDetails,
  useConversationMessages,
  useMyConversations,
  useSendMessage,
} from "@diploma/features";
import { useQueryClient } from "@tanstack/react-query";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { ArrowLeft, MessageCircleMore, SendHorizontal } from "lucide-react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { FormEvent, useEffect, useMemo, useRef, useState } from "react";
import { AppShell } from "../components/AppShell";
import { useAuth } from "../providers";

const ACCESS_TOKEN_COOKIE = "diploma_access_token";
const CHAT_HUB_URL =
  typeof window !== "undefined"
    ? "/hubs/chat"
    : `${process.env.NEXT_PUBLIC_API_BASE_URL || "http://diploma.com:5102"}/hubs/chat`;

function readCookie(name: string) {
  if (typeof document === "undefined") {
    return null;
  }

  const value = document.cookie
    .split("; ")
    .find((entry) => entry.startsWith(`${name}=`));

  return value ? decodeURIComponent(value.split("=").slice(1).join("=")) : null;
}

function formatTime(value?: string | null) {
  if (!value) return "";
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) return "";

  return new Intl.DateTimeFormat("ru-BY", {
    hour: "2-digit",
    minute: "2-digit",
    day: "2-digit",
    month: "2-digit",
    timeZone: "Europe/Minsk",
  }).format(parsed);
}

function formatPrice(value?: number | null, currency?: string | null) {
  if (value == null) return "Цена по запросу";
  const suffix = currency ? ` ${currency}` : "";
  return `${new Intl.NumberFormat("ru-RU", { maximumFractionDigits: Number.isInteger(value) ? 0 : 2 }).format(value)}${suffix}`;
}

function syncMessagesRead(messages: MessageDto[], payload: MessagesReadDto) {
  const ids = new Set(payload.messageIds);
  return messages.map((message) => (
    ids.has(message.id)
      ? { ...message, isRead: true }
      : message
  ));
}

function upsertConversationItem(items: ConversationListItemDto[], incoming: ConversationListItemDto) {
  const withoutCurrent = items.filter((item) => item.conversationId !== incoming.conversationId);
  return [incoming, ...withoutCurrent].sort((left, right) => {
    const leftTime = left.lastMessageAt ? Date.parse(left.lastMessageAt) : 0;
    const rightTime = right.lastMessageAt ? Date.parse(right.lastMessageAt) : 0;
    return rightTime - leftTime;
  });
}

function createClientMessageId() {
  if (typeof globalThis !== "undefined" && globalThis.crypto && typeof globalThis.crypto.randomUUID === "function") {
    return globalThis.crypto.randomUUID();
  }

  return `msg-${Date.now()}-${Math.random().toString(36).slice(2, 11)}`;
}

type MessagesScope = "listing" | "support";

function renderSupportUserInfo(details: ConversationDetailsDto) {
  return (
    <div
      style={{
        display: "grid",
        gap: 8,
        padding: "12px 14px",
        borderRadius: 18,
        background: "var(--color-primary-surface)",
        border: "1px solid var(--color-primary-border)",
      }}
    >
      <div style={{ fontWeight: 800, fontSize: "0.96rem", lineHeight: 1.15 }}>
        Пользователь
      </div>
      <div style={{ display: "grid", gap: 4, color: "var(--foreground-muted)", fontSize: 13, lineHeight: 1.3 }}>
        <div><span style={{ color: "var(--foreground)" }}>Имя:</span> {details.otherUserName}</div>
        <div><span style={{ color: "var(--foreground)" }}>ID:</span> {details.otherUserId}</div>
        <div><span style={{ color: "var(--foreground)" }}>Email:</span> {details.otherUserEmail ?? "нет данных"}</div>
        <div><span style={{ color: "var(--foreground)" }}>Телефон:</span> {details.otherUserPhone ?? "нет данных"}</div>
      </div>
    </div>
  );
}

export function MessagesClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const { api, status, user } = useAuth();
  const [draft, setDraft] = useState("");
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [isWideLayout, setIsWideLayout] = useState(false);
  const [isMobileClientShell, setIsMobileClientShell] = useState(false);
  const [isStartingSupportConversation, setIsStartingSupportConversation] = useState(false);
  const listRef = useRef<HTMLDivElement | null>(null);
  const selectedConversationId = searchParams.get("conversationId");
  const requestedScope: MessagesScope = searchParams.get("scope") === "support" ? "support" : "listing";
  const isAdmin = user?.role?.toLowerCase() === "admin";
  const conversationsQuery = useMyConversations(api, status === "authenticated");
  const allConversations = conversationsQuery.data ?? [];
  const supportConversation = useMemo(
    () => allConversations.find((item) => item.conversationType === "support") ?? null,
    [allConversations],
  );
  const filteredConversations = useMemo(
    () => allConversations.filter((item) => item.conversationType === requestedScope),
    [allConversations, requestedScope],
  );
  const selectedConversationFromList = useMemo(
    () => allConversations.find((item) => item.conversationId === selectedConversationId) ?? null,
    [allConversations, selectedConversationId],
  );
  const conversationDetailsQuery = useConversationDetails(
    api,
    selectedConversationId,
    status === "authenticated" && (!selectedConversationFromList || selectedConversationFromList.conversationType === "support"),
  );
  const conversationDetails: ConversationDetailsDto | null = useMemo(() => {
    if (selectedConversationFromList) {
      return {
        conversationId: selectedConversationFromList.conversationId,
        conversationType: selectedConversationFromList.conversationType,
        listingId: selectedConversationFromList.listingId,
        listingTitle: selectedConversationFromList.listingTitle,
        listingPrice: selectedConversationFromList.listingPrice,
        listingPriceCurrency: selectedConversationFromList.listingPriceCurrency,
        listingImageUrl: selectedConversationFromList.listingImageUrl,
        otherUserId: selectedConversationFromList.otherUserId,
        otherUserName: selectedConversationFromList.otherUserName,
        otherUserAvatarUrl: selectedConversationFromList.otherUserAvatarUrl,
        otherUserEmail: null,
        otherUserPhone: null,
      };
    }

    return conversationDetailsQuery.data ?? null;
  }, [conversationDetailsQuery.data, selectedConversationFromList]);
  const effectiveConversationDetails = conversationDetailsQuery.data ?? conversationDetails;
  const messagesQuery = useConversationMessages(api, selectedConversationId, 1, 50, status === "authenticated");
  const sendMessageMutation = useSendMessage(api, selectedConversationId);
  const isSupportConversation = effectiveConversationDetails?.conversationType === "support";

  useEffect(() => {
    const mediaQuery = window.matchMedia("(min-width: 960px)");
    const sync = () => setIsWideLayout(mediaQuery.matches);
    sync();
    mediaQuery.addEventListener("change", sync);
    return () => mediaQuery.removeEventListener("change", sync);
  }, []);

  useEffect(() => {
    if (typeof document === "undefined") {
      return;
    }

    setIsMobileClientShell(document.documentElement.hasAttribute("data-mobile-client-shell"));
  }, []);

  useEffect(() => {
    setMessages(messagesQuery.data?.messages ?? []);
  }, [messagesQuery.data?.messages, selectedConversationId]);

  useEffect(() => {
    if (!selectedConversationId || status !== "authenticated") {
      return;
    }

    void markConversationAsRead(api, selectedConversationId)
      .then(() => {
        void queryClient.invalidateQueries({ queryKey: ["chat", "unread-count"] });
        void queryClient.invalidateQueries({ queryKey: ["chat", "conversations"] });
      })
      .catch(() => {});
  }, [api, queryClient, selectedConversationId, status]);

  useEffect(() => {
    if (!selectedConversationId || status !== "authenticated") {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(CHAT_HUB_URL, {
        accessTokenFactory: async () => readCookie(ACCESS_TOKEN_COOKIE) ?? "",
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("message_received", (message: MessageDto) => {
      void queryClient.invalidateQueries({ queryKey: ["chat", "conversations"] });
      void queryClient.invalidateQueries({ queryKey: ["chat", "unread-count"] });

      if (message.conversationId !== selectedConversationId) {
        return;
      }

      setMessages((current) => current.some((item) => item.id === message.id) ? current : [...current, message]);

      if (!message.isMine) {
        void markConversationAsRead(api, selectedConversationId).catch(() => {});
      }
    });

    connection.on("conversation_updated", (item: ConversationListItemDto) => {
      queryClient.setQueryData<ConversationListItemDto[] | undefined>(["chat", "conversations"], (current) =>
        current ? upsertConversationItem(current, item) : [item]);
      void queryClient.invalidateQueries({ queryKey: ["chat", "unread-count"] });
    });

    connection.on("messages_read", (payload: MessagesReadDto) => {
      if (payload.conversationId !== selectedConversationId) {
        return;
      }

      setMessages((current) => syncMessagesRead(current, payload));
    });

    void connection.start()
      .then(() => connection.invoke("JoinConversation", selectedConversationId))
      .catch(() => {});

    return () => {
      void connection.invoke("LeaveConversation", selectedConversationId).catch(() => {});
      void connection.stop().catch(() => {});
    };
  }, [api, queryClient, selectedConversationId, status]);

  useEffect(() => {
    if (!listRef.current) {
      return;
    }

    listRef.current.scrollTop = listRef.current.scrollHeight;
  }, [messages]);

  async function handleSendMessage(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const text = draft.trim();
    if (!selectedConversationId || !text) {
      return;
    }

    const created = await sendMessageMutation.mutateAsync({
      text,
      clientMessageId: createClientMessageId(),
    });

    setDraft("");
    setMessages((current) => current.some((item) => item.id === created.id) ? current : [...current, created]);
    await queryClient.invalidateQueries({ queryKey: ["chat", "conversations"] });
  }

  function buildMessagesHref(scope: MessagesScope, conversationId?: string | null) {
    const params = new URLSearchParams();
    if (scope === "support") {
      params.set("scope", "support");
    }
    if (conversationId) {
      params.set("conversationId", conversationId);
    }

    const query = params.toString();
    return query ? `/messages?${query}` : "/messages";
  }

  function handleConversationSelect(conversationId: string, conversationType: ConversationType) {
    const nextScope: MessagesScope = conversationType === "support" ? "support" : "listing";
    router.push(buildMessagesHref(nextScope, conversationId));
  }

  function handleScopeChange(scope: MessagesScope) {
    const matchingSelectedConversation = selectedConversationFromList?.conversationType === scope
      ? selectedConversationFromList.conversationId
      : null;
    router.push(buildMessagesHref(scope, matchingSelectedConversation));
  }

  async function handleOpenSupportConversation() {
    if (status !== "authenticated" || isAdmin) {
      return;
    }

    if (supportConversation) {
      router.push(buildMessagesHref("support", supportConversation.conversationId));
      return;
    }

    setIsStartingSupportConversation(true);

    try {
      const conversation = await startSupportConversation(api);
      await queryClient.invalidateQueries({ queryKey: ["chat", "conversations"] });
      void queryClient.invalidateQueries({ queryKey: ["chat", "unread-count"] });
      router.push(buildMessagesHref("support", conversation.conversationId));
    } finally {
      setIsStartingSupportConversation(false);
    }
  }

  if (status === "guest") {
    return (
      <AppShell lockScroll>
        <main className="grid gap-6">
          <section className="surface grid gap-4 rounded-[30px] p-8">
            <div className="eyebrow">Сообщения</div>
            <h1 className="section-title" style={{ marginTop: 6 }}>Войдите, чтобы открыть диалоги</h1>
            <p style={{ margin: 0, color: "var(--foreground-muted)" }}>
              Напишите продавцу из карточки объявления, и диалог появится здесь.
            </p>
          </section>
        </main>
      </AppShell>
    );
  }

  return (
    <AppShell lockScroll>
      <main
        className="grid gap-4 pb-6"
        style={{
          marginTop: isWideLayout ? 0 : 0,
          paddingBottom: 0,
          height: isWideLayout ? "calc(100dvh - 152px)" : "100%",
          minHeight: 0,
          overflow: "hidden",
        }}
      >
        <section
          className="surface"
          style={{
            display: "grid",
            gap: 0,
            height: "100%",
            minHeight: 0,
            borderRadius: 32,
            overflow: "hidden",
            paddingBottom: isWideLayout ? 0 : isMobileClientShell ? 0 : 3,
            gridTemplateColumns: isWideLayout ? "360px minmax(0, 1fr)" : "minmax(0,1fr)",
          }}
        >
          <aside
            style={{
              display: selectedConversationId && !isWideLayout ? "none" : "grid",
              gridTemplateRows: "minmax(0, 1fr)",
              borderRight: isWideLayout ? "1px solid var(--border-color)" : "none",
              background: "var(--background-elevated)",
              minWidth: 0,
              minHeight: 0,
              overflow: "hidden",
            }}
          >
            <div style={{ overflowY: "auto", overflowX: "hidden", minHeight: 0, minWidth: 0 }}>
              <div
                style={{
                  padding: isWideLayout ? "14px 18px 12px" : "10px 16px 10px",
                  borderBottom: "1px solid var(--border-color)",
                  display: "grid",
                  gap: 12,
                }}
              >
                <div
                  style={{
                    fontFamily: "var(--font-display)",
                    fontSize: isWideLayout ? "1.45rem" : "1.25rem",
                    fontWeight: 700,
                    letterSpacing: "-0.04em",
                    lineHeight: 0.92,
                  }}
                >
                  {isAdmin ? "Сообщения" : requestedScope === "support" ? "Поддержка" : "Мои диалоги"}
                </div>
                <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                  <button
                    type="button"
                    className={requestedScope === "listing" ? "button-primary" : "button-secondary"}
                    onClick={() => handleScopeChange("listing")}
                    style={{ minHeight: 36, padding: "0 14px", borderRadius: 14, fontSize: 14 }}
                  >
                    {isAdmin ? "Объявления" : "Диалоги"}
                  </button>
                  <button
                    type="button"
                    className={requestedScope === "support" ? "button-primary" : "button-secondary"}
                    onClick={() => {
                      if (isAdmin) {
                        handleScopeChange("support");
                        return;
                      }

                      void handleOpenSupportConversation();
                    }}
                    style={{ minHeight: 36, padding: "0 14px", borderRadius: 14, fontSize: 14 }}
                  >
                    Поддержка
                  </button>
                </div>
              </div>
              {filteredConversations.length === 0 ? (
                <div style={{ padding: 24, color: "var(--foreground-muted)", display: "grid", gap: 8 }}>
                  <div style={{ fontWeight: 700 }}>
                    {requestedScope === "support"
                      ? (isAdmin ? "Обращений в поддержку пока нет." : "У вас пока нет обращения в поддержку.")
                      : "У вас пока нет диалогов."}
                  </div>
                  <div>
                    {requestedScope === "support"
                      ? (isAdmin ? "Новые обращения пользователей появятся здесь." : "Если что-то не работает, откройте обращение в поддержку.")
                      : "Откройте объявление и напишите продавцу."}
                  </div>
                  {!isAdmin && requestedScope === "support" ? (
                    <button
                      type="button"
                      className="button-primary"
                      onClick={() => void handleOpenSupportConversation()}
                      disabled={isStartingSupportConversation}
                      style={{ marginTop: 4, minHeight: 42, width: "fit-content", padding: "0 16px", borderRadius: 14 }}
                    >
                      {isStartingSupportConversation ? "Открываем..." : "Написать в поддержку"}
                    </button>
                  ) : null}
                </div>
              ) : (
                filteredConversations.map((item, index) => {
                  const isActive = item.conversationId === selectedConversationId;
                  return (
                    <button
                      key={item.conversationId}
                      type="button"
                      onClick={() => handleConversationSelect(item.conversationId, item.conversationType)}
                      style={{
                        width: "100%",
                        minWidth: 0,
                        padding: index === 0
                          ? (isWideLayout ? "8px 18px 18px" : "6px 16px 16px")
                          : (isWideLayout ? 18 : 16),
                        border: "none",
                        borderBottom: "1px solid var(--border-color)",
                        background: isActive
                          ? "color-mix(in srgb, var(--color-primary-soft) 86%, var(--background-elevated))"
                          : "transparent",
                        boxShadow: isActive ? "inset 0 0 0 1px var(--color-primary-border)" : "none",
                        color: "inherit",
                        textAlign: "left",
                        display: "grid",
                        gap: 10,
                        cursor: "pointer",
                        overflow: "hidden",
                      }}
                    >
                      <div style={{ display: "flex", alignItems: "center", gap: 12, minWidth: 0 }}>
                        <div
                          style={{
                            width: 52,
                            height: 52,
                            borderRadius: 18,
                            overflow: "hidden",
                            background: "var(--background-muted)",
                            flexShrink: 0,
                          }}
                        >
                          {item.otherUserAvatarUrl ? (
                            <img alt={item.otherUserName} src={item.otherUserAvatarUrl} style={{ width: "100%", height: "100%", objectFit: "cover" }} />
                          ) : (
                            <div style={{ width: "100%", height: "100%", display: "grid", placeItems: "center", fontWeight: 800 }}>
                              {item.otherUserName.slice(0, 1).toUpperCase()}
                            </div>
                          )}
                        </div>
                        <div style={{ minWidth: 0, flex: 1 }}>
                          <div style={{ display: "flex", justifyContent: "space-between", gap: 10, alignItems: "start" }}>
                            <div style={{ fontWeight: 800, lineHeight: 1.2 }}>{item.otherUserName}</div>
                            <div style={{ color: "var(--foreground-muted)", fontSize: 12, whiteSpace: "nowrap" }}>
                              {formatTime(item.lastMessageAt)}
                            </div>
                          </div>
                          <div style={{ marginTop: 4, color: "var(--foreground-muted)", fontSize: 13, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                            {item.conversationType === "support" ? "Техподдержка платформы" : item.listingTitle}
                          </div>
                        </div>
                      </div>
                      <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "center", minWidth: 0, overflow: "hidden" }}>
                        <div
                          style={{
                            minWidth: 0,
                            flex: 1,
                            color: "var(--foreground-muted)",
                            fontSize: 14,
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                            whiteSpace: "nowrap",
                          }}
                        >
                          {item.lastMessagePreview ?? "Диалог создан"}
                        </div>
                        {item.unreadCount > 0 ? (
                          <span
                            style={{
                              minWidth: 22,
                              height: 22,
                              borderRadius: 999,
                              background: "linear-gradient(180deg, var(--color-primary-strong), var(--color-primary-active))",
                              color: "var(--text-on-accent)",
                              display: "grid",
                              placeItems: "center",
                              padding: "0 6px",
                              fontSize: 12,
                              fontWeight: 800,
                              border: "1px solid color-mix(in srgb, var(--color-primary-strong) 70%, transparent)",
                              boxShadow: "0 8px 18px color-mix(in srgb, var(--shadow-color) 66%, transparent)",
                              flexShrink: 0,
                            }}
                          >
                            {item.unreadCount}
                          </span>
                        ) : null}
                      </div>
                    </button>
                  );
                })
              )}
            </div>
          </aside>

          <section
            style={{
              display: !selectedConversationId && !isWideLayout ? "none" : "grid",
              minWidth: 0,
              minHeight: 0,
              overflow: "hidden",
              background: "var(--background)",
              gridTemplateRows: isWideLayout ? "auto auto minmax(0,1fr) auto" : "auto auto minmax(0,1fr) auto",
            }}
          >
            {selectedConversationId && !isWideLayout ? (
              <div
                style={{
                  padding: "10px 12px 8px",
                  borderBottom: "1px solid var(--border-color)",
                  display: "flex",
                  alignItems: "center",
                  gap: 10,
                  minWidth: 0,
                }}
              >
                <button
                  type="button"
                  onClick={() => router.push("/messages")}
                  style={{
                    display: "inline-flex",
                    alignItems: "center",
                    gap: 6,
                    border: "none",
                    background: "transparent",
                    color: "var(--foreground)",
                    padding: 0,
                    fontWeight: 700,
                    cursor: "pointer",
                    flexShrink: 0,
                  }}
                >
                  <ArrowLeft size={18} />
                </button>
                {effectiveConversationDetails ? (
                  <div style={{ minWidth: 0 }}>
                    <div
                      style={{
                        fontWeight: 800,
                        fontSize: "0.98rem",
                        lineHeight: 1.1,
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                        whiteSpace: "nowrap",
                      }}
                    >
                      {effectiveConversationDetails.otherUserName}
                    </div>
                  </div>
                ) : null}
              </div>
            ) : null}

            {effectiveConversationDetails ? (
              <>
                {isWideLayout ? (
                  <>
                    <div
                      style={{
                        padding: "22px 24px 18px",
                        borderBottom: "1px solid var(--border-color)",
                        display: "grid",
                        gap: 6,
                      }}
                    >
                      <div
                        style={{
                          fontWeight: 800,
                          fontSize: "1.1rem",
                          lineHeight: 1.1,
                        }}
                      >
                        {effectiveConversationDetails.otherUserName}
                      </div>
                      <div style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
                        {isSupportConversation ? "Обращение в поддержку Domio" : "Обсуждение объявления"}
                      </div>
                      {isSupportConversation && isAdmin ? (
                        renderSupportUserInfo(effectiveConversationDetails)
                      ) : null}
                    </div>

                    {isSupportConversation ? (
                      <div style={{ padding: 18, borderBottom: "1px solid var(--border-color)" }}>
                        <div
                          style={{
                            display: "grid",
                            gap: 6,
                            padding: 14,
                            borderRadius: 22,
                            background: "var(--background-elevated)",
                            border: "1px solid var(--color-primary-border)",
                          }}
                        >
                          <div style={{ fontWeight: 800, fontSize: "1rem" }}>Поддержка Domio</div>
                          <div style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
                            Здесь можно решить технические и сервисные вопросы по платформе.
                          </div>
                        </div>
                      </div>
                    ) : (
                      <div style={{ padding: 18, borderBottom: "1px solid var(--border-color)" }}>
                        <Link
                          href={`/listings/${effectiveConversationDetails.listingId}`}
                          style={{
                            display: "grid",
                            gap: 8,
                            padding: 14,
                            borderRadius: 22,
                            background: "var(--background-elevated)",
                            border: "1px solid var(--border-color)",
                            color: "inherit",
                            textDecoration: "none",
                          }}
                        >
                          <div style={{ display: "flex", gap: 14, alignItems: "center" }}>
                            <div
                              style={{
                                width: 72,
                                height: 56,
                                borderRadius: 16,
                                overflow: "hidden",
                                background: "var(--background-muted)",
                                flexShrink: 0,
                              }}
                            >
                              {effectiveConversationDetails.listingImageUrl ? (
                                <img alt={effectiveConversationDetails.listingTitle} src={effectiveConversationDetails.listingImageUrl} style={{ width: "100%", height: "100%", objectFit: "cover" }} />
                              ) : null}
                            </div>
                            <div style={{ minWidth: 0 }}>
                              <div style={{ fontWeight: 800, lineHeight: 1.2, fontSize: "1rem" }}>{effectiveConversationDetails.listingTitle}</div>
                              <div style={{ marginTop: 4, color: "var(--foreground-muted)", fontSize: "1rem" }}>
                                {formatPrice(effectiveConversationDetails.listingPrice, effectiveConversationDetails.listingPriceCurrency)}
                              </div>
                            </div>
                          </div>
                        </Link>
                      </div>
                    )}
                  </>
                ) : (
                  isSupportConversation ? (
                    <div style={{ padding: "8px 10px 6px", borderBottom: "1px solid var(--border-color)" }}>
                      <div
                        style={{
                          display: "grid",
                          gap: 2,
                          padding: "8px 10px",
                          borderRadius: 16,
                          background: "var(--background-elevated)",
                          border: "1px solid var(--color-primary-border)",
                        }}
                      >
                        <div style={{ fontSize: "1rem", lineHeight: 1.15, fontWeight: 700 }}>Поддержка Domio</div>
                        <div style={{ color: "var(--foreground-muted)", fontSize: "0.86rem", lineHeight: 1.15 }}>
                          Технические и сервисные вопросы по платформе
                        </div>
                      </div>
                      {isSupportConversation && isAdmin ? (
                        <div style={{ marginTop: 6 }}>
                          {renderSupportUserInfo(effectiveConversationDetails)}
                        </div>
                      ) : null}
                    </div>
                  ) : (
                    <div style={{ padding: "6px 10px 4px", borderBottom: "1px solid var(--border-color)" }}>
                      <Link
                        href={`/listings/${effectiveConversationDetails.listingId}`}
                        style={{
                          display: "flex",
                          alignItems: "center",
                          gap: 8,
                          color: "inherit",
                          textDecoration: "none",
                          minWidth: 0,
                        }}
                      >
                        <div
                          style={{
                            width: 65,
                            height: 50,
                            borderRadius: 14,
                            overflow: "hidden",
                            background: "var(--background-muted)",
                            flexShrink: 0,
                          }}
                        >
                          {effectiveConversationDetails.listingImageUrl ? (
                            <img
                              alt={effectiveConversationDetails.listingTitle}
                              src={effectiveConversationDetails.listingImageUrl}
                              style={{ width: "100%", height: "100%", objectFit: "cover" }}
                            />
                          ) : null}
                        </div>
                        <div style={{ minWidth: 0 }}>
                          <div
                            style={{
                              fontSize: "1.08rem",
                              lineHeight: 1.15,
                              fontWeight: 700,
                              overflow: "hidden",
                              textOverflow: "ellipsis",
                              whiteSpace: "nowrap",
                            }}
                          >
                            {effectiveConversationDetails.listingTitle}
                          </div>
                          <div
                            style={{
                              marginTop: 2,
                              color: "var(--foreground-muted)",
                              fontSize: "0.98rem",
                              lineHeight: 1.1,
                              overflow: "hidden",
                              textOverflow: "ellipsis",
                              whiteSpace: "nowrap",
                            }}
                          >
                            {formatPrice(effectiveConversationDetails.listingPrice, effectiveConversationDetails.listingPriceCurrency)}
                          </div>
                        </div>
                      </Link>
                    </div>
                  )
                )}

                <div
                  ref={listRef}
                  style={{
                    overflowY: "auto",
                    padding: isWideLayout ? 24 : "2px 10px 4px",
                    marginTop: 0,
                    display: "flex",
                    flexDirection: "column",
                    gap: isWideLayout ? 3 : 3,
                    minHeight: 0,
                  }}
                >
                  {messages.map((message) => (
                    <div
                      key={message.id}
                      style={{
                        width: "100%",
                        display: "flex",
                        justifyContent: message.isMine ? "flex-end" : "flex-start",
                      }}
                    >
                      <div
                        style={{
                          padding: "7px 11px",
                          borderRadius: 16,
                          background: message.isMine
                            ? "linear-gradient(180deg, color-mix(in srgb, var(--color-primary-strong) 94%, white), var(--color-primary-active))"
                            : "var(--background-elevated)",
                          color: message.isMine ? "var(--text-on-accent)" : "var(--foreground)",
                          border: message.isMine
                            ? "1px solid color-mix(in srgb, var(--color-primary-strong) 52%, transparent)"
                            : "1px solid var(--border-color)",
                          boxShadow: message.isMine
                            ? "0 10px 24px color-mix(in srgb, var(--shadow-color) 62%, transparent)"
                            : "none",
                          lineHeight: 1.18,
                          fontWeight: 600,
                          whiteSpace: "pre-wrap",
                          overflowWrap: "anywhere",
                          width: "fit-content",
                          maxWidth: isWideLayout ? "min(68%, 520px)" : "72%",
                        }}
                      >
                        <div>{message.text}</div>
                        <div
                          style={{
                            marginTop: 3,
                            fontSize: 10,
                            lineHeight: 1,
                            color: message.isMine ? "color-mix(in srgb, var(--text-on-accent) 82%, transparent)" : "var(--foreground-muted)",
                            textAlign: "right",
                          }}
                        >
                          {formatTime(message.createdAt)}{message.isMine ? (message.isRead ? " · Прочитано" : " · Отправлено") : ""}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                <form
                  onSubmit={handleSendMessage}
                  style={{
                    padding: isWideLayout
                      ? "10px 12px 12px"
                      : isMobileClientShell
                        ? "6px 8px calc(12px + env(safe-area-inset-bottom, 0px))"
                        : "6px 8px calc(12px + env(safe-area-inset-bottom, 0px))",
                    borderTop: "1px solid var(--border-color)",
                    display: "grid",
                    gridTemplateColumns: isWideLayout ? "minmax(0,1fr) 56px" : "minmax(0,1fr) minmax(72px, 20%)",
                    gap: isWideLayout ? 12 : 6,
                    alignItems: "stretch",
                    transform: isWideLayout ? undefined : isMobileClientShell ? "translateY(8px)" : undefined,
                  }}
                >
                  <textarea
                    className="field"
                    value={draft}
                    onChange={(event) => setDraft(event.target.value)}
                    placeholder="Напишите сообщение"
                    rows={isWideLayout ? 2 : 1}
                    style={{
                      minHeight: isWideLayout ? 44 : 34,
                      resize: "none",
                      overflow: "hidden",
                      padding: isWideLayout ? "10px 14px" : "5px 9px",
                      borderRadius: isWideLayout ? 20 : 18,
                    }}
                  />
                  <button
                    type="submit"
                    className="button-primary"
                    disabled={!draft.trim() || sendMessageMutation.isPending}
                    style={{
                      width: "100%",
                      minWidth: isWideLayout ? 56 : 67,
                      minHeight: isWideLayout ? 44 : 32,
                      display: "grid",
                      placeItems: "center",
                      padding: 0,
                      alignSelf: "stretch",
                      marginBottom: 0,
                      borderRadius: isWideLayout ? 14 : 14,
                    }}
                    aria-label="Отправить сообщение"
                  >
                    <SendHorizontal size={isWideLayout ? 18 : 16} />
                  </button>
                </form>
              </>
            ) : (
              <div style={{ minHeight: 520, display: "grid", placeItems: "center", padding: 24 }}>
                <div style={{ display: "grid", gap: 12, justifyItems: "center", color: "var(--foreground-muted)", textAlign: "center", maxWidth: 320 }}>
                  <div
                    style={{
                      width: 72,
                      height: 72,
                      borderRadius: 24,
                      background: "var(--background-elevated)",
                      display: "grid",
                      placeItems: "center",
                      border: "1px solid var(--border-color)",
                    }}
                  >
                    <MessageCircleMore size={28} />
                  </div>
                  <div style={{ fontWeight: 800, color: "var(--foreground)" }}>Выберите диалог</div>
                  <div>
                    {requestedScope === "support"
                      ? (isAdmin ? "Выберите обращение пользователя слева." : "Откройте обращение в поддержку, и переписка появится здесь.")
                      : "Откройте любой разговор слева или напишите продавцу из карточки объявления."}
                  </div>
                  {!isAdmin && requestedScope === "support" ? (
                    <button
                      type="button"
                      className="button-primary"
                      onClick={() => void handleOpenSupportConversation()}
                      disabled={isStartingSupportConversation}
                      style={{ minHeight: 42, padding: "0 16px", borderRadius: 14 }}
                    >
                      {isStartingSupportConversation ? "Открываем..." : "Написать в поддержку"}
                    </button>
                  ) : null}
                </div>
              </div>
            )}
          </section>
        </section>
      </main>
    </AppShell>
  );
}
