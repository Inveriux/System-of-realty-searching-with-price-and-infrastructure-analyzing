"use client";

import { markConversationAsRead } from "@diploma/api/chat";
import type { ConversationDetailsDto, ConversationListItemDto, MessageDto, MessagesReadDto } from "@diploma/contracts/chat";
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

export function MessagesClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const { api, status } = useAuth();
  const [draft, setDraft] = useState("");
  const [messages, setMessages] = useState<MessageDto[]>([]);
  const [isWideLayout, setIsWideLayout] = useState(false);
  const listRef = useRef<HTMLDivElement | null>(null);
  const selectedConversationId = searchParams.get("conversationId");
  const conversationsQuery = useMyConversations(api, status === "authenticated");
  const selectedConversationFromList = useMemo(
    () => conversationsQuery.data?.find((item) => item.conversationId === selectedConversationId) ?? null,
    [conversationsQuery.data, selectedConversationId],
  );
  const conversationDetailsQuery = useConversationDetails(
    api,
    selectedConversationId,
    status === "authenticated" && !selectedConversationFromList,
  );
  const conversationDetails: ConversationDetailsDto | null = useMemo(() => {
    if (selectedConversationFromList) {
      return {
        conversationId: selectedConversationFromList.conversationId,
        listingId: selectedConversationFromList.listingId,
        listingTitle: selectedConversationFromList.listingTitle,
        listingPrice: selectedConversationFromList.listingPrice,
        listingPriceCurrency: selectedConversationFromList.listingPriceCurrency,
        listingImageUrl: selectedConversationFromList.listingImageUrl,
        otherUserId: selectedConversationFromList.otherUserId,
        otherUserName: selectedConversationFromList.otherUserName,
        otherUserAvatarUrl: selectedConversationFromList.otherUserAvatarUrl,
      };
    }

    return conversationDetailsQuery.data ?? null;
  }, [conversationDetailsQuery.data, selectedConversationFromList]);
  const messagesQuery = useConversationMessages(api, selectedConversationId, 1, 50, status === "authenticated");
  const sendMessageMutation = useSendMessage(api, selectedConversationId);

  useEffect(() => {
    const mediaQuery = window.matchMedia("(min-width: 960px)");
    const sync = () => setIsWideLayout(mediaQuery.matches);
    sync();
    mediaQuery.addEventListener("change", sync);
    return () => mediaQuery.removeEventListener("change", sync);
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
      clientMessageId: crypto.randomUUID(),
    });

    setDraft("");
    setMessages((current) => current.some((item) => item.id === created.id) ? current : [...current, created]);
    await queryClient.invalidateQueries({ queryKey: ["chat", "conversations"] });
  }

  function handleConversationSelect(conversationId: string) {
    router.push(`/messages?conversationId=${conversationId}`);
  }

  if (status === "guest") {
    return (
      <AppShell>
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
    <AppShell>
      <main className="grid gap-4 pb-6" style={{ marginTop: isWideLayout ? -20 : 0, paddingBottom: 0 }}>
        <section
          className="surface"
          style={{
            display: "grid",
            gap: 0,
            minHeight: isWideLayout ? "calc(100dvh - 136px)" : "calc(100dvh - 120px)",
            borderRadius: 32,
            overflow: "hidden",
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
            }}
          >
            <div style={{ overflowY: "auto", minHeight: 0 }}>
              <div
                style={{
                  padding: isWideLayout ? "14px 18px 10px" : "10px 16px 8px",
                  borderBottom: "1px solid var(--border-color)",
                  fontFamily: "var(--font-display)",
                  fontSize: isWideLayout ? "1.45rem" : "1.25rem",
                  fontWeight: 700,
                  letterSpacing: "-0.04em",
                  lineHeight: 0.92,
                }}
              >
                Мои диалоги
              </div>
              {(conversationsQuery.data ?? []).length === 0 ? (
                <div style={{ padding: 24, color: "var(--foreground-muted)", display: "grid", gap: 8 }}>
                  <div style={{ fontWeight: 700 }}>У вас пока нет диалогов.</div>
                  <div>Откройте объявление и напишите продавцу.</div>
                </div>
              ) : (
                (conversationsQuery.data ?? []).map((item, index) => {
                  const isActive = item.conversationId === selectedConversationId;
                  return (
                    <button
                      key={item.conversationId}
                      type="button"
                      onClick={() => handleConversationSelect(item.conversationId)}
                      style={{
                        width: "100%",
                        padding: index === 0
                          ? (isWideLayout ? "8px 18px 18px" : "6px 16px 16px")
                          : (isWideLayout ? 18 : 16),
                        border: "none",
                        borderBottom: "1px solid var(--border-color)",
                        background: isActive
                          ? "color-mix(in srgb, var(--color-primary-soft) 22%, var(--background-elevated))"
                          : "transparent",
                        color: "inherit",
                        textAlign: "left",
                        display: "grid",
                        gap: 10,
                        cursor: "pointer",
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
                            {item.listingTitle}
                          </div>
                        </div>
                      </div>
                      <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "center" }}>
                        <div style={{ color: "var(--foreground-muted)", fontSize: 14, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                          {item.lastMessagePreview ?? "Диалог создан"}
                        </div>
                        {item.unreadCount > 0 ? (
                          <span
                            style={{
                              minWidth: 22,
                              height: 22,
                              borderRadius: 999,
                              background: "var(--color-primary)",
                              color: "#fff",
                              display: "grid",
                              placeItems: "center",
                              padding: "0 6px",
                              fontSize: 12,
                              fontWeight: 800,
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
                {conversationDetails ? (
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
                      {conversationDetails.otherUserName}
                    </div>
                  </div>
                ) : null}
              </div>
            ) : null}

            {conversationDetails ? (
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
                        {conversationDetails.otherUserName}
                      </div>
                      <div style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
                        Обсуждение объявления
                      </div>
                    </div>

                    <div style={{ padding: 18, borderBottom: "1px solid var(--border-color)" }}>
                      <Link
                        href={`/listings/${conversationDetails.listingId}`}
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
                            {conversationDetails.listingImageUrl ? (
                              <img alt={conversationDetails.listingTitle} src={conversationDetails.listingImageUrl} style={{ width: "100%", height: "100%", objectFit: "cover" }} />
                            ) : null}
                          </div>
                          <div style={{ minWidth: 0 }}>
                            <div style={{ fontWeight: 800, lineHeight: 1.2, fontSize: "1rem" }}>{conversationDetails.listingTitle}</div>
                            <div style={{ marginTop: 4, color: "var(--foreground-muted)", fontSize: "1rem" }}>
                              {formatPrice(conversationDetails.listingPrice, conversationDetails.listingPriceCurrency)}
                            </div>
                          </div>
                        </div>
                      </Link>
                    </div>
                  </>
                ) : (
                  <div style={{ padding: "6px 10px 4px", borderBottom: "1px solid var(--border-color)" }}>
                    <Link
                      href={`/listings/${conversationDetails.listingId}`}
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
                        {conversationDetails.listingImageUrl ? (
                          <img
                            alt={conversationDetails.listingTitle}
                            src={conversationDetails.listingImageUrl}
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
                          {conversationDetails.listingTitle}
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
                          {formatPrice(conversationDetails.listingPrice, conversationDetails.listingPriceCurrency)}
                        </div>
                      </div>
                    </Link>
                  </div>
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
                            ? "linear-gradient(180deg, color-mix(in srgb, var(--color-primary) 82%, white), color-mix(in srgb, var(--color-primary-strong) 82%, black))"
                            : "var(--background-elevated)",
                          color: message.isMine ? "#fff" : "var(--foreground)",
                          border: message.isMine ? "none" : "1px solid var(--border-color)",
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
                            color: message.isMine ? "color-mix(in srgb, #ffffff 82%, transparent)" : "var(--foreground-muted)",
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
                    padding: isWideLayout ? "10px 12px 12px" : "3px 6px 4px",
                    borderTop: "1px solid var(--border-color)",
                    display: "grid",
                    gridTemplateColumns: isWideLayout ? "minmax(0,1fr) 56px" : "minmax(0,1fr) minmax(72px, 20%)",
                    gap: isWideLayout ? 12 : 6,
                    alignItems: "stretch",
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
                  <div>Откройте любой разговор слева или напишите продавцу из карточки объявления.</div>
                </div>
              </div>
            )}
          </section>
        </section>
      </main>
    </AppShell>
  );
}
