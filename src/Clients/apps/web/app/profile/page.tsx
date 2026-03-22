"use client";

import { updateOwnProfile, uploadUserAvatar } from "@diploma/api/users";
import { useListingsQuery } from "@diploma/features";
import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { AppShell } from "../components/AppShell";
import { useAuth } from "../providers";

function getInitials(name?: string | null) {
  if (!name) {
    return "U";
  }

  return name
    .split(" ")
    .map((part) => part.trim()[0])
    .filter(Boolean)
    .slice(0, 2)
    .join("")
    .toUpperCase();
}

export default function ProfilePage() {
  const { api, status, user, reloadCurrentUser, signOut } = useAuth();
  const { data: listingsData, isLoading: listingsLoading } = useListingsQuery(api);
  const [name, setName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setName(user?.name ?? "");
    setPhoneNumber(user?.phoneNumber ?? "");
  }, [user]);

  const ownListingsCount =
    user == null
      ? 0
      : (listingsData?.items ?? []).filter((listing) => listing.agentId === user.id).length;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!user) {
      return;
    }

    setIsSaving(true);
    setError(null);
    setSaveMessage(null);

    try {
      await updateOwnProfile(api, {
        name: name.trim(),
        phoneNumber: phoneNumber.trim() || null,
      });

      if (avatarFile) {
        await uploadUserAvatar(api, avatarFile);
      }

      await reloadCurrentUser();
      setAvatarFile(null);
      setSaveMessage("Профиль обновлен.");
    } catch (submissionError) {
      if (
        submissionError instanceof Error &&
        submissionError.message.includes("User not found")
      ) {
        signOut();
        setError("Сессия устарела после перезапуска сервиса. Войдите в аккаунт заново.");
        return;
      }

      setError(
        submissionError instanceof Error
          ? submissionError.message
          : "Не удалось сохранить профиль.",
      );
    } finally {
      setIsSaving(false);
    }
  }

  if (status !== "authenticated" || !user) {
    return (
      <AppShell>
        <section className="surface grid gap-4 rounded-[28px] p-7">
          <div className="eyebrow">Профиль</div>
          <h1 className="section-title">Войдите в аккаунт</h1>
          <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 680 }}>
            Профиль, избранное и управление своими объявлениями доступны только после входа.
          </p>
          <div>
            <Link className="button-primary" href="/listings">
              Перейти к каталогу
            </Link>
          </div>
        </section>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <main className="grid gap-6 pb-10">
        <section className="surface grid gap-6 rounded-[30px] p-7">
          <div className="grid gap-5 lg:grid-cols-[auto_minmax(0,1fr)] lg:items-start">
            <div
              style={{
                width: 108,
                height: 108,
                borderRadius: 30,
                overflow: "hidden",
                display: "grid",
                placeItems: "center",
                background: "color-mix(in srgb, var(--color-primary-soft) 78%, white)",
                color: "var(--color-primary-strong)",
                fontSize: "2rem",
                fontWeight: 800,
                letterSpacing: "-0.06em",
              }}
            >
              {user.accountImageUrl ? (
                <img
                  alt={user.name}
                  src={user.accountImageUrl}
                  style={{ width: "100%", height: "100%", objectFit: "cover" }}
                />
              ) : (
                <span>{getInitials(user.name)}</span>
              )}
            </div>

            <div className="grid gap-4">
              <div>
                <div className="eyebrow">Профиль</div>
                <h1 className="section-title" style={{ marginTop: 8 }}>
                  {user.name}
                </h1>
                <p style={{ margin: "0.5rem 0 0", color: "var(--foreground-muted)" }}>
                  {user.email}
                </p>
                {user.phoneNumber ? (
                  <p style={{ margin: "0.35rem 0 0", color: "var(--foreground-muted)" }}>
                    {user.phoneNumber}
                  </p>
                ) : null}
              </div>

              <div className="grid gap-3 sm:grid-cols-3">
                <Link
                  href="/profile/listings"
                  className="surface"
                  style={{
                    borderRadius: 22,
                    padding: 18,
                    textDecoration: "none",
                    color: "inherit",
                    border: "1px solid var(--border-color)",
                  }}
                >
                  <div className="eyebrow">Раздел</div>
                  <div style={{ marginTop: 8, fontWeight: 800, fontSize: "1.05rem" }}>Мои объявления</div>
                  <div style={{ marginTop: 6, color: "var(--foreground-muted)", fontSize: 14 }}>
                    {listingsLoading ? "Загрузка..." : `${ownListingsCount} объектов`}
                  </div>
                </Link>
                <Link
                  href="/favorites"
                  className="surface"
                  style={{
                    borderRadius: 22,
                    padding: 18,
                    textDecoration: "none",
                    color: "inherit",
                    border: "1px solid var(--border-color)",
                  }}
                >
                  <div className="eyebrow">Раздел</div>
                  <div style={{ marginTop: 8, fontWeight: 800, fontSize: "1.05rem" }}>Избранное</div>
                  <div style={{ marginTop: 6, color: "var(--foreground-muted)", fontSize: 14 }}>
                    Сохраненные объекты
                  </div>
                </Link>
                <Link
                  href="/create-listing"
                  className="surface"
                  style={{
                    borderRadius: 22,
                    padding: 18,
                    textDecoration: "none",
                    color: "inherit",
                    border: "1px solid var(--border-color)",
                  }}
                >
                  <div className="eyebrow">Действие</div>
                  <div style={{ marginTop: 8, fontWeight: 800, fontSize: "1.05rem" }}>Добавить объявление</div>
                  <div style={{ marginTop: 6, color: "var(--foreground-muted)", fontSize: 14 }}>
                    Создание нового объекта
                  </div>
                </Link>
              </div>
            </div>
          </div>
        </section>

        <section className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_380px]">
          <section className="surface grid gap-4 rounded-[30px] p-7">
            <div>
              <div className="eyebrow">Управление объектами</div>
              <h2 className="section-title" style={{ marginTop: 8 }}>Каталог ваших объявлений</h2>
            </div>

            <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 560 }}>
              Список ваших объектов вынесен на отдельную страницу, чтобы редактирование профиля оставалось компактным.
            </p>

            <div>
              <Link className="button-primary" href="/profile/listings">
                Открыть мой каталог
              </Link>
            </div>
          </section>

          <aside className="surface grid gap-4 rounded-[30px] p-7 self-start">
            <div>
              <div className="eyebrow">Настройки</div>
              <h2 className="section-title" style={{ marginTop: 8 }}>Редактирование профиля</h2>
            </div>

            <form onSubmit={handleSubmit} className="grid gap-4">
              <input
                className="field"
                value={name}
                onChange={(event) => setName(event.target.value)}
                placeholder="Имя"
                required
              />
              <input
                className="field"
                value={user.email}
                disabled
                readOnly
                placeholder="Email"
              />
              <input
                className="field"
                value={phoneNumber}
                onChange={(event) => setPhoneNumber(event.target.value)}
                placeholder="Номер телефона"
                type="tel"
              />

              <input
                accept="image/*"
                id="profile-avatar-upload"
                type="file"
                style={{ display: "none" }}
                onChange={(event) => setAvatarFile(event.target.files?.[0] ?? null)}
              />
              <label
                htmlFor="profile-avatar-upload"
                style={{
                  display: "grid",
                  gap: 6,
                  borderRadius: 18,
                  border: "1px dashed var(--border-color)",
                  background: "var(--background-elevated)",
                  padding: "14px 16px",
                  color: "var(--foreground)",
                  cursor: "pointer",
                }}
              >
                <span style={{ fontWeight: 700 }}>
                  {avatarFile ? "Новое фото выбрано" : "Загрузить новый аватар"}
                </span>
                <span style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
                  {avatarFile ? avatarFile.name : "PNG, JPG, WEBP"}
                </span>
              </label>

              {error ? (
                <div style={{ color: "#b42318", fontSize: 14 }}>{error}</div>
              ) : null}
              {saveMessage ? (
                <div style={{ color: "#166534", fontSize: 14 }}>{saveMessage}</div>
              ) : null}

              <button className="button-primary" type="submit" disabled={isSaving}>
                {isSaving ? "Сохранение..." : "Сохранить изменения"}
              </button>
            </form>
          </aside>
        </section>
      </main>
    </AppShell>
  );
}
