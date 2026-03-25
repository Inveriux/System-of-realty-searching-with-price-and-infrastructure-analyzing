"use client";

import type { SignInDto, SignUpDto } from "@diploma/contracts/auth";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "../providers";
import { authDialogStyles, getAuthDialogModeButtonStyle } from "./auth-dialog.styles";

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
      style={authDialogStyles.backdrop}
    >
      <div
        className="surface"
        onClick={(event) => event.stopPropagation()}
        style={authDialogStyles.panel}
      >
        <div className="eyebrow">Профиль</div>
        <h2 className="section-title" style={authDialogStyles.title}>
          {title}
        </h2>
        <div style={authDialogStyles.modeSwitch}>
          <button
            type="button"
            onClick={() => {
              setActiveMode("signin");
              setError(null);
            }}
            style={getAuthDialogModeButtonStyle(activeMode === "signin")}
          >
            Вход
          </button>
          <button
            type="button"
            onClick={() => {
              setActiveMode("signup");
              setError(null);
            }}
            style={getAuthDialogModeButtonStyle(activeMode === "signup")}
          >
            Регистрация
          </button>
        </div>

        <button
          type="button"
          onClick={handleGoogleSignIn}
          style={authDialogStyles.googleButton}
        >
          <span aria-hidden="true" style={authDialogStyles.googleBadge}>
            G
          </span>
          <span>Войти через Google</span>
        </button>

        <form onSubmit={handleSubmit} style={authDialogStyles.form}>
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
                style={authDialogStyles.hiddenFileInput}
                type="file"
              />
              <label
                htmlFor="auth-avatar-upload"
                style={authDialogStyles.uploadLabel}
              >
                <span style={authDialogStyles.uploadTitle}>
                  {avatarFile ? "Фото профиля выбрано" : "Загрузить фото профиля"}
                </span>
                <span style={authDialogStyles.uploadMeta}>
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
            <div style={authDialogStyles.error}>
              {error}
            </div>
          ) : null}

          <div style={authDialogStyles.actions}>
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
