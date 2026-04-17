"use client";

import { changePassword, updateOwnProfile, uploadUserAvatar } from "@diploma/api/users";
import { useListingsQuery } from "@diploma/features";
import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { AppShell } from "../components/AppShell";
import { AuthDialog } from "../components/AuthDialog";
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
  const [removeAvatar, setRemoveAvatar] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [showPasswordForm, setShowPasswordForm] = useState(false);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [passwordMessage, setPasswordMessage] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [authDialogMode, setAuthDialogMode] = useState<"signin" | "signup" | null>(null);

  useEffect(() => {
    setName(user?.name ?? "");
    setPhoneNumber(user?.phoneNumber ?? "");
    setRemoveAvatar(false);
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
        accountImageUrl: removeAvatar ? null : user.accountImageUrl,
      });

      if (avatarFile) {
        await uploadUserAvatar(api, avatarFile);
      }

      await reloadCurrentUser();
      setAvatarFile(null);
      setRemoveAvatar(false);
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

  async function handleChangePassword() {
    if (!user) {
      return;
    }

    setIsChangingPassword(true);
    setPasswordError(null);
    setPasswordMessage(null);

    try {
      await changePassword(api, {
        email: user.email,
        currentPassword: user.hasPassword ? currentPassword : null,
        newPassword,
      });

      await reloadCurrentUser();
      setCurrentPassword("");
      setNewPassword("");
      setPasswordMessage(user.hasPassword ? "Пароль обновлён." : "Пароль установлен.");
      setShowPasswordForm(false);
    } catch (submissionError) {
      setPasswordError(
        submissionError instanceof Error
          ? submissionError.message
          : "Не удалось сохранить пароль.",
      );
    } finally {
      setIsChangingPassword(false);
    }
  }

  if (status !== "authenticated" || !user) {
    return (
      <AppShell>
        <>
          <section className="surface grid gap-4 rounded-[28px] p-7">
            <div className="eyebrow">Профиль</div>
            <h1 className="section-title">Войдите в аккаунт</h1>
            <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 680 }}>
              Профиль, избранное и управление своими объявлениями доступны только после входа.
            </p>
            <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
              <button className="button-primary" type="button" onClick={() => setAuthDialogMode("signin")}>
                Войти в аккаунт
              </button>
              <button className="button-secondary" type="button" onClick={() => setAuthDialogMode("signup")}>
                Регистрация
              </button>
            </div>
          </section>
          <AuthDialog mode={authDialogMode} onClose={() => setAuthDialogMode(null)} />
        </>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <main
        className="grid gap-6 pb-10 profile-page-main"
        style={{
          width: "100%",
          marginInline: "auto",
        }}
      >
        <section className="profile-main-grid grid gap-6 xl:grid-cols-[minmax(0,1fr)_400px] xl:items-start">
          <section className="profile-primary-column grid gap-6">
            <section className="surface profile-hero grid gap-6 rounded-[30px] p-7">
              <div className="profile-top-grid grid gap-5 lg:grid-cols-[auto_minmax(0,1fr)] lg:items-start">
                <div
                  className="profile-avatar"
                  style={{
                    width: 108,
                    height: 108,
                    borderRadius: 30,
                    overflow: "hidden",
                    display: "grid",
                    placeItems: "center",
                    background: "color-mix(in srgb, var(--color-primary-soft) 28%, var(--background-elevated))",
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

                <div className="profile-header-info">
                  <div className="eyebrow">Профиль</div>
                  <h1 className="section-title profile-title" style={{ marginTop: 8 }}>
                    {user.name}
                  </h1>
                  <p className="profile-meta" style={{ margin: "0.5rem 0 0", color: "var(--foreground-muted)" }}>
                    {user.email}
                  </p>
                  {user.phoneNumber ? (
                    <p className="profile-meta" style={{ margin: "0.35rem 0 0", color: "var(--foreground-muted)" }}>
                      {user.phoneNumber}
                    </p>
                  ) : null}
                </div>
              </div>
            </section>

            <section className="surface profile-actions-card grid gap-3 rounded-[30px] p-5">
              <div>
                <div className="eyebrow">Быстрые действия</div>
                <h2 className="section-title" style={{ marginTop: 6 }}>Управление объявлениями</h2>
              </div>

              <div className="profile-actions-grid">
                <Link className="button-primary profile-action-button" href="/profile/listings">
                  Открыть мои объявления
                </Link>
                <Link className="button-secondary profile-action-button" href="/create-listing">
                  Добавить новое объявление
                </Link>
              </div>
            </section>

            <section className="profile-sections-grid grid gap-3 sm:grid-cols-2">
              <Link
                href="/profile/listings"
                className="surface profile-section-card"
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
                className="surface profile-section-card"
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
                href="/history"
                className="surface profile-section-card"
                style={{
                  borderRadius: 22,
                  padding: 18,
                  textDecoration: "none",
                  color: "inherit",
                  border: "1px solid var(--border-color)",
                }}
              >
                <div className="eyebrow">Раздел</div>
                <div style={{ marginTop: 8, fontWeight: 800, fontSize: "1.05rem" }}>История просмотра</div>
                <div style={{ marginTop: 6, color: "var(--foreground-muted)", fontSize: 14 }}>
                  Недавно открытые объекты
                </div>
              </Link>
              <Link
                href="/create-listing"
                className="surface profile-section-card"
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
            </section>
          </section>

          <aside className="surface profile-settings-card grid gap-4 rounded-[30px] p-7 self-start">
            <div>
              <div className="eyebrow">Настройки</div>
              <h2 className="section-title" style={{ marginTop: 8 }}>Редактирование профиля</h2>
            </div>

            <form onSubmit={handleSubmit} className="profile-settings-form grid gap-4">
              <input
                className="field"
                value={name}
                onChange={(event) => setName(event.target.value)}
                placeholder="Имя"
                required
              />
              <input
                className="field"
                value={user.email ?? ""}
                disabled
                readOnly
                placeholder="Email"
              />
              <div
                style={{
                  borderRadius: 16,
                  border: "1px solid var(--border-color)",
                  background: "var(--background-elevated)",
                  padding: "12px 14px",
                  color: "var(--foreground-muted)",
                  fontSize: 14,
                }}
              >
                {user.emailConfirmed ? "Почта подтверждена" : "Почта не подтверждена"}
                {user.isGoogleAccount ? " • вход через Google" : ""}
              </div>
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
                onChange={(event) => {
                  setAvatarFile(event.target.files?.[0] ?? null);
                  setRemoveAvatar(false);
                }}
              />
              <label
                htmlFor="profile-avatar-upload"
                className="profile-upload-label"
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

              {user.accountImageUrl || avatarFile ? (
                <button
                  className="button-secondary"
                  type="button"
                  onClick={() => {
                    setAvatarFile(null);
                    setRemoveAvatar(true);
                    setSaveMessage(null);
                    setError(null);
                  }}
                >
                  Убрать фото профиля
                </button>
              ) : null}

              {error ? (
                <div style={{ color: "#b42318", fontSize: 14 }}>{error}</div>
              ) : null}
              {saveMessage ? (
                <div style={{ color: "#166534", fontSize: 14 }}>{saveMessage}</div>
              ) : null}

              <button className="button-primary" type="submit" disabled={isSaving}>
                {isSaving ? "Сохранение..." : "Сохранить изменения"}
              </button>

              <button
                className="button-secondary"
                type="button"
                onClick={() => {
                  setShowPasswordForm((current) => !current);
                  setPasswordError(null);
                  setPasswordMessage(null);
                }}
              >
                {user.hasPassword ? "Изменить пароль" : "Установить пароль"}
              </button>

              {showPasswordForm ? (
                <div className="grid gap-3">
                  {user.hasPassword ? (
                    <input
                      className="field"
                      type="password"
                      placeholder="Текущий пароль"
                      value={currentPassword}
                      onChange={(event) => setCurrentPassword(event.target.value)}
                      required
                    />
                  ) : null}

                  <input
                    className="field"
                    type="password"
                    placeholder={user.hasPassword ? "Новый пароль" : "Придумайте пароль"}
                    value={newPassword}
                    onChange={(event) => setNewPassword(event.target.value)}
                    minLength={8}
                    required
                  />

                  {passwordError ? (
                    <div style={{ color: "#b42318", fontSize: 14 }}>{passwordError}</div>
                  ) : null}
                  {passwordMessage ? (
                    <div style={{ color: "#166534", fontSize: 14 }}>{passwordMessage}</div>
                  ) : null}

                  <button
                    className="button-primary"
                    type="button"
                    disabled={isChangingPassword}
                    onClick={() => {
                      void handleChangePassword();
                    }}
                  >
                    {isChangingPassword
                      ? "Сохранение..."
                      : user.hasPassword
                        ? "Обновить пароль"
                        : "Установить пароль"}
                  </button>
                </div>
              ) : null}
            </form>
          </aside>
        </section>
      </main>
    </AppShell>
  );
}
