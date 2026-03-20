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

export function AuthDialog({ mode, onClose }: AuthDialogProps) {
  const { signIn, signUp } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [signInValues, setSignInValues] = useState(initialSignInState);
  const [signUpValues, setSignUpValues] = useState(initialSignUpState);

  useEffect(() => {
    if (mode) {
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
      if (mode === "signin") {
        await signIn(signInValues);
      } else {
        await signUp({
          ...signUpValues,
          accountImageUrl: signUpValues.accountImageUrl?.trim() || null,
        });
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

  const title = mode === "signin" ? "Вход в профиль" : "Создание аккаунта";
  const description =
    mode === "signin"
      ? "Сохраняем access и refresh токены в cookie, а потом автоматически обновляем сессию."
      : "Сразу после регистрации делаем вход и подтягиваем профиль пользователя.";

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
        <div className="eyebrow">{mode === "signin" ? "Auth" : "Sign Up"}</div>
        <h2 className="section-title" style={{ marginTop: 8 }}>
          {title}
        </h2>
        <p style={{ color: "var(--foreground-muted)", marginBottom: 20 }}>{description}</p>

        <form onSubmit={handleSubmit} style={{ display: "grid", gap: 14 }}>
          {mode === "signup" ? (
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
                className="field"
                onChange={(event) =>
                  setSignUpValues((current) => ({
                    ...current,
                    accountImageUrl: event.target.value,
                  }))
                }
                placeholder="Ссылка на аватар, если есть"
                value={signUpValues.accountImageUrl ?? ""}
              />
            </>
          ) : null}

          <input
            className="field"
            onChange={(event) => {
              const email = event.target.value;
              if (mode === "signin") {
                setSignInValues((current) => ({ ...current, email }));
              } else {
                setSignUpValues((current) => ({ ...current, email }));
              }
            }}
            placeholder="Email"
            required
            type="email"
            value={mode === "signin" ? signInValues.email : signUpValues.email}
          />

          <input
            className="field"
            minLength={8}
            onChange={(event) => {
              const password = event.target.value;
              if (mode === "signin") {
                setSignInValues((current) => ({ ...current, password }));
              } else {
                setSignUpValues((current) => ({ ...current, password }));
              }
            }}
            placeholder="Пароль"
            required
            type="password"
            value={mode === "signin" ? signInValues.password : signUpValues.password}
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
              {isSubmitting ? "Отправка..." : mode === "signin" ? "Войти" : "Создать аккаунт"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
