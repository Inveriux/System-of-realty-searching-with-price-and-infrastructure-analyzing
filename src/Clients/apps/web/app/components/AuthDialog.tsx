"use client";

import type { SignInDto, SignUpDto } from "@diploma/contracts/auth";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "../providers";

type AuthDialogProps = {
  mode: "signin" | "signup" | null;
  onClose(): void;
};

const initialSignInState: SignInDto = {
  email: "",
  password: "",
};

const initialSignUpState: SignUpDto = {
  name: "",
  email: "",
  password: "",
  accountImageUrl: "",
};

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://diploma.com:5102";

export function AuthDialog({ mode, onClose }: AuthDialogProps) {
  const { signIn, signUp } = useAuth();
  const [activeMode, setActiveMode] = useState<"signin" | "signup">("signin");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [signInValues, setSignInValues] = useState(initialSignInState);
  const [signUpValues, setSignUpValues] = useState(initialSignUpState);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);

  useEffect(() => {
    if (mode) {
      setActiveMode(mode);
      setError(null);
    }
  }, [mode]);

  if (!mode) {
    return null;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      if (activeMode === "signin") {
        await signIn(signInValues);
      } else {
        await signUp({
          ...signUpValues,
          accountImageUrl: signUpValues.accountImageUrl?.trim() || null,
        }, avatarFile);
      }

      onClose();
    } catch (submissionError) {
      setError(
        submissionError instanceof Error
          ? submissionError.message
          : "Не удалось выполнить запрос.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  const title = activeMode === "signin" ? "Вход в профиль" : "Создание аккаунта";

  function handleGoogleSignIn() {
    const form = document.createElement("form");
    form.method = "POST";
    form.action = `${API_BASE_URL}/api/users/google/signup`;
    document.body.appendChild(form);
    form.submit();
    form.remove();
  }

  return (
    <div
      onClick={onClose}
      style={{
        position: "fixed",
        inset: 0,
        background: "rgba(10, 7, 20, 0.62)",
        display: "grid",
        placeItems: "center",
        padding: 16,
        zIndex: 50,
      }}
    >
      <div
        className="surface"
        onClick={(event) => event.stopPropagation()}
        style={{
          width: "min(100%, 460px)",
          borderRadius: 28,
          padding: 24,
        }}
      >
        <div className="eyebrow">Профиль</div>
        <h2 className="section-title" style={{ marginTop: 8 }}>
          {title}
        </h2>
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr",
            gap: 8,
            marginTop: 18,
            marginBottom: 18,
            padding: 6,
            borderRadius: 20,
            background: "var(--background-muted)",
            border: "1px solid var(--border-color)",
          }}
        >
          <button
            type="button"
            onClick={() => {
              setActiveMode("signin");
              setError(null);
            }}
            style={{
              border: "none",
              borderRadius: 14,
              padding: "10px 14px",
              font: "inherit",
              fontWeight: 700,
              cursor: "pointer",
              background: activeMode === "signin" ? "var(--background-elevated)" : "transparent",
              color: activeMode === "signin" ? "var(--foreground)" : "var(--foreground-muted)",
              boxShadow: activeMode === "signin" ? "0 10px 24px rgba(15, 23, 42, 0.08)" : "none",
            }}
          >
            Вход
          </button>
          <button
            type="button"
            onClick={() => {
              setActiveMode("signup");
              setError(null);
            }}
            style={{
              border: "none",
              borderRadius: 14,
              padding: "10px 14px",
              font: "inherit",
              fontWeight: 700,
              cursor: "pointer",
              background: activeMode === "signup" ? "var(--background-elevated)" : "transparent",
              color: activeMode === "signup" ? "var(--foreground)" : "var(--foreground-muted)",
              boxShadow: activeMode === "signup" ? "0 10px 24px rgba(15, 23, 42, 0.08)" : "none",
            }}
          >
            Регистрация
          </button>
        </div>

        <button
          type="button"
          onClick={handleGoogleSignIn}
          style={{
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
          }}
        >
          <span
            aria-hidden="true"
            style={{
              display: "grid",
              placeItems: "center",
              width: 34,
              height: 34,
              borderRadius: 999,
              background: "color-mix(in srgb, var(--color-primary-soft) 72%, white)",
              color: "var(--color-primary-strong)",
              fontSize: 20,
              fontWeight: 900,
            }}
          >
            G
          </span>
          <span>Войти через Google</span>
        </button>

        <form onSubmit={handleSubmit} style={{ display: "grid", gap: 14 }}>
          {activeMode === "signup" ? (
            <>
              <input
                className="field"
                onChange={(event) =>
                  setSignUpValues((current) => ({ ...current, name: event.target.value }))
                }
                placeholder="Имя"
                required
                value={signUpValues.name}
              />
              <input
                accept="image/*"
                id="auth-avatar-upload"
                onChange={(event) => {
                  const file = event.target.files?.[0] ?? null;
                  setAvatarFile(file);
                }}
                style={{ display: "none" }}
                type="file"
              />
              <label
                htmlFor="auth-avatar-upload"
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
                  {avatarFile ? "Фото профиля выбрано" : "Загрузить фото профиля"}
                </span>
                <span style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
                  {avatarFile ? avatarFile.name : "PNG, JPG, WEBP"}
                </span>
              </label>
            </>
          ) : null}

          <input
            className="field"
            onChange={(event) => {
              const email = event.target.value;
              if (activeMode === "signin") {
                setSignInValues((current) => ({ ...current, email }));
              } else {
                setSignUpValues((current) => ({ ...current, email }));
              }
            }}
            placeholder="Email"
            required
            type="email"
            value={activeMode === "signin" ? signInValues.email : signUpValues.email}
          />

          <input
            className="field"
            minLength={8}
            onChange={(event) => {
              const password = event.target.value;
              if (activeMode === "signin") {
                setSignInValues((current) => ({ ...current, password }));
              } else {
                setSignUpValues((current) => ({ ...current, password }));
              }
            }}
            placeholder="Пароль"
            required
            type="password"
            value={activeMode === "signin" ? signInValues.password : signUpValues.password}
          />

          {error ? (
            <div
              style={{
                borderRadius: 18,
                padding: "12px 14px",
                background: "color-mix(in srgb, #d04b73 12%, var(--background-elevated))",
                border: "1px solid color-mix(in srgb, #d04b73 36%, var(--border-color))",
                color: "var(--foreground)",
              }}
            >
              {error}
            </div>
          ) : null}

          <div style={{ display: "flex", gap: 10, justifyContent: "flex-end" }}>
            <button className="button-secondary" onClick={onClose} type="button">
              Отмена
            </button>
            <button className="button-primary" disabled={isSubmitting} type="submit">
              {isSubmitting ? "Отправка..." : activeMode === "signin" ? "Войти" : "Создать аккаунт"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
