"use client";

import type { SignInDto, SignUpDto } from "@diploma/contracts/auth";
import { FormEvent, useEffect, useState } from "react";
import { useAuth } from "../providers";
import { authDialogStyles, getAuthDialogModeButtonStyle } from "./auth-dialog.styles";

type AuthDialogProps = {
  mode: "signin" | "signup" | null;
  onClose(): void;
};

type PendingVerificationState = {
  email: string;
  password: string;
};

type AuthFieldName = "name" | "email" | "password" | "verificationCode";

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
  const { signIn, signUp, verifySignUpEmail, resendVerificationCode } = useAuth();
  const [activeMode, setActiveMode] = useState<"signin" | "signup">("signin");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Partial<Record<AuthFieldName, string>>>({});
  const [signInValues, setSignInValues] = useState(initialSignInState);
  const [signUpValues, setSignUpValues] = useState(initialSignUpState);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [verificationCode, setVerificationCode] = useState("");
  const [pendingVerification, setPendingVerification] = useState<PendingVerificationState | null>(null);

  useEffect(() => {
    if (mode) {
      setActiveMode(mode);
      setError(null);
      setFieldErrors({});
      setPendingVerification(null);
      setVerificationCode("");
    }
  }, [mode]);

  if (!mode) {
    return null;
  }

  function getFieldStyle(fieldName: AuthFieldName) {
    if (!fieldErrors[fieldName]) {
      return undefined;
    }

    return {
      borderColor: "var(--color-danger)",
      boxShadow: "0 0 0 1px color-mix(in srgb, var(--color-danger) 55%, transparent)",
      background: "color-mix(in srgb, var(--color-danger) 7%, var(--background-elevated))",
    };
  }

  function renderFieldError(fieldName: AuthFieldName) {
    const message = fieldErrors[fieldName];
    if (!message) {
      return null;
    }

    return (
      <div style={authDialogStyles.fieldErrorText}>
        {message}
      </div>
    );
  }

  function validateFields() {
    const nextErrors: Partial<Record<AuthFieldName, string>> = {};

    if (pendingVerification) {
      if (verificationCode.trim().length !== 6) {
        nextErrors.verificationCode = "Введите 6-значный код из письма.";
      }
      return nextErrors;
    }

    const email = (activeMode === "signin" ? signInValues.email : signUpValues.email).trim();
    const password = activeMode === "signin" ? signInValues.password : signUpValues.password;

    if (activeMode === "signup" && !signUpValues.name.trim()) {
      nextErrors.name = "Введите имя.";
    }

    if (!email) {
      nextErrors.email = "Введите email.";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      nextErrors.email = "Введите корректный email.";
    }

    if (!password.trim()) {
      nextErrors.password = "Введите пароль.";
    } else if (password.length < 8) {
      nextErrors.password = "Пароль должен быть не короче 8 символов.";
    }

    return nextErrors;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError(null);
    const nextFieldErrors = validateFields();
    setFieldErrors(nextFieldErrors);

    if (Object.keys(nextFieldErrors).length > 0) {
      setIsSubmitting(false);
      return;
    }

    try {
      if (pendingVerification) {
        await verifySignUpEmail(
          pendingVerification.email,
          pendingVerification.password,
          verificationCode,
        );
      } else if (activeMode === "signin") {
        await signIn(signInValues);
      } else {
        await signUp({
          ...signUpValues,
          accountImageUrl: signUpValues.accountImageUrl?.trim() || null,
        }, avatarFile);
        setPendingVerification({
          email: signUpValues.email,
          password: signUpValues.password,
        });
        setVerificationCode("");
        return;
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

  const title = pendingVerification
    ? "Подтверждение почты"
    : activeMode === "signin"
      ? "Вход в профиль"
      : "Создание аккаунта";

  function handleGoogleSignIn() {
    const form = document.createElement("form");
    form.method = "POST";
    form.action = `${window.location.origin}/api/users/google/signup`;
    form.style.display = "none";

    const fields = {
      returnUrl: "/listings",
      frontendBaseUrl: window.location.origin,
      prompt: "select_account",
    };

    for (const [name, value] of Object.entries(fields)) {
      const input = document.createElement("input");
      input.type = "hidden";
      input.name = name;
      input.value = value;
      form.appendChild(input);
    }

    document.body.appendChild(form);
    form.submit();
  }

  return (
    <div
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

        <form noValidate onSubmit={handleSubmit} style={authDialogStyles.form}>
          {pendingVerification ? (
            <>
              <div style={authDialogStyles.uploadMeta}>
                На {pendingVerification.email} отправлен 6-значный код подтверждения.
              </div>
              <div style={authDialogStyles.fieldGroup}>
                <input
                  key="verification-code"
                  className="field"
                  inputMode="numeric"
                  maxLength={6}
                  onChange={(event) => {
                    setVerificationCode(event.target.value.replace(/\D/g, "").slice(0, 6));
                    setFieldErrors((current) => ({ ...current, verificationCode: undefined }));
                  }}
                  placeholder="Код из письма"
                  required
                  style={getFieldStyle("verificationCode")}
                  value={verificationCode}
                />
                {renderFieldError("verificationCode")}
              </div>
              <button
                className="button-secondary"
                onClick={async () => {
                  setIsSubmitting(true);
                  setError(null);

                  try {
                    await resendVerificationCode(pendingVerification.email);
                  } catch (submissionError) {
                    setError(
                      submissionError instanceof Error
                        ? submissionError.message
                        : "Не удалось отправить код повторно.",
                    );
                  } finally {
                    setIsSubmitting(false);
                  }
                }}
                type="button"
              >
                Отправить код ещё раз
              </button>
            </>
          ) : activeMode === "signup" ? (
            <>
              <div style={authDialogStyles.fieldGroup}>
                <input
                  key="signup-name"
                  className="field"
                  onChange={(event) => {
                    setSignUpValues((current) => ({ ...current, name: event.target.value }));
                    setFieldErrors((current) => ({ ...current, name: undefined }));
                  }}
                  placeholder="Имя"
                  required
                  style={getFieldStyle("name")}
                  value={signUpValues.name}
                />
                {renderFieldError("name")}
              </div>
              <input
                key="signup-avatar"
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

          {pendingVerification ? null : (
            <div style={authDialogStyles.fieldGroup}>
              <input
                key={`email-${activeMode}`}
                className="field"
                onChange={(event) => {
                  const email = event.target.value;
                  if (activeMode === "signin") {
                    setSignInValues((current) => ({ ...current, email }));
                  } else {
                    setSignUpValues((current) => ({ ...current, email }));
                  }
                  setFieldErrors((current) => ({ ...current, email: undefined }));
                }}
                placeholder="Email"
                required
                style={getFieldStyle("email")}
                type="email"
                value={activeMode === "signin" ? signInValues.email : signUpValues.email}
              />
              {renderFieldError("email")}
            </div>
          )}

          {pendingVerification ? null : (
            <div style={authDialogStyles.fieldGroup}>
              <input
                key={`password-${activeMode}`}
                className="field"
                minLength={8}
                onChange={(event) => {
                  const password = event.target.value;
                  if (activeMode === "signin") {
                    setSignInValues((current) => ({ ...current, password }));
                  } else {
                    setSignUpValues((current) => ({ ...current, password }));
                  }
                  setFieldErrors((current) => ({ ...current, password: undefined }));
                }}
                placeholder="Пароль"
                required
                style={getFieldStyle("password")}
                type="password"
                value={activeMode === "signin" ? signInValues.password : signUpValues.password}
              />
              {renderFieldError("password")}
            </div>
          )}

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
              {isSubmitting
                ? "Отправка..."
                : pendingVerification
                  ? "Подтвердить почту"
                  : activeMode === "signin"
                    ? "Войти"
                    : "Создать аккаунт"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
