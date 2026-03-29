import { z } from "zod";

export interface AuthSession {
  getAccessToken(): Promise<string | null>;
  refreshAccessToken?(): Promise<string | null>;
  clearAuth?(): Promise<void> | void;
}

export type ApiClientOptions = {
  baseUrl: string;
  authSession?: AuthSession;
};

type RequestOptions = {
  authMode?: "optional" | "none";
};

type ApiErrorPayload = {
  detail?: string | null;
  title?: string | null;
  errors?: Array<{
    message?: string | null;
    errorMessage?: string | null;
    propertyName?: string | null;
  }>;
};

const apiMessageTranslations: Array<[string, string]> = [
  ["Unsupported image extension. Allowed: .jpg, .jpeg, .png, .webp.", "Неподдерживаемый формат изображения. Разрешены только JPG, JPEG, PNG и WEBP."],
  ["Image file is required.", "Нужно выбрать изображение."],
  ["Validation failed.", "Проверьте заполнение формы и повторите попытку."],
  ["Validation failed", "Проверьте заполнение формы и повторите попытку."],
  ["Invalid email.", "Пользователь с такой почтой не найден."],
  ["User not found.", "Пользователь не найден."],
  ["Incorrect password.", "Неверный пароль."],
  ["Invalid password.", "Неверный пароль."],
  ["Email is not verified.", "Почта ещё не подтверждена."],
  ["Confirm your email before signing in.", "Сначала подтвердите почту, затем войдите в аккаунт."],
  ["Invalid verification code.", "Неверный код подтверждения."],
  ["Verification code is invalid or expired.", "Код подтверждения неверный или уже истёк."],
  ["Email is already verified.", "Почта уже подтверждена."],
  ["Email verification is not configured.", "Подтверждение почты сейчас недоступно."],
  ["SMTP sender is not configured.", "Сервис отправки писем сейчас не настроен."],
  ["Email is already in use.", "Пользователь с такой почтой уже существует."],
  ["Email is already in use by a verified account.", "Пользователь с такой почтой уже существует."],
  ["Current password is required.", "Введите текущий пароль."],
  ["Failed to change password.", "Не удалось изменить пароль."],
  ["Refresh token is expired sign up again.", "Сессия истекла. Войдите снова."],
  ["Invalid or expired refresh token.", "Сессия истекла. Войдите снова."],
  ["Invalid refresh token.", "Сессия недействительна. Войдите снова."],
  ["Password is required.", "Введите пароль."],
  ["Email is required.", "Введите email."],
  ["Email format is invalid.", "Укажите корректный email."],
  ["Name is required.", "Введите имя."],
  ["Sign in data must be provided.", "Не удалось выполнить вход. Проверьте данные и попробуйте ещё раз."],
  ["Sign up data must be provided.", "Не удалось выполнить регистрацию. Проверьте данные и попробуйте ещё раз."],
  ["Password change data must be provided.", "Не удалось изменить пароль. Проверьте данные и попробуйте ещё раз."],
  ["Phone number contains invalid characters.", "Номер телефона содержит недопустимые символы."],
  ["Max price must be greater than or equal to min price.", "Максимальная цена должна быть больше или равна минимальной."],
  ["Max area must be greater than or equal to min area.", "Максимальная площадь должна быть больше или равна минимальной."],
  ["Latitude/Longitude and RadiusMeters must be passed together.", "Для поиска по радиусу нужно указать точку и радиус."],
  ["All bounds coordinates must be passed together.", "Для поиска по области нужно указать все границы."],
  ["Unsupported sort mode.", "Выбран неподдерживаемый режим сортировки."],
];

const apiPropertyTranslations: Record<string, string> = {
  Email: "Почта",
  EmailConfirmed: "Подтверждение почты",
  Password: "Пароль",
  CurrentPassword: "Текущий пароль",
  NewPassword: "Новый пароль",
  Name: "Имя",
  PhoneNumber: "Телефон",
  Code: "Код подтверждения",
  SignUpUserDto: "Регистрация",
  ChangePasswordDto: "Смена пароля",
  User: "Пользователь",
  RefreshToken: "Сессия",
};

function humanizeApiMessage(message: string) {
  const normalized = message.trim();
  if (!normalized) {
    return "Произошла ошибка. Попробуйте ещё раз.";
  }

  for (const [source, target] of apiMessageTranslations) {
    if (normalized.includes(source)) {
      return target;
    }
  }

  if (normalized.startsWith("User with ") && normalized.includes(" email already exists.")) {
    return "Пользователь с такой почтой уже существует.";
  }

  if (normalized.startsWith("User with ") && normalized.includes(" email not found.")) {
    return "Пользователь с такой почтой не найден.";
  }

  if (normalized.startsWith("User with ") && normalized.includes(" id not found.")) {
    return "Пользователь не найден.";
  }

  if (normalized.startsWith("Failed to fetch API:")) {
    return "Не удалось выполнить запрос. Попробуйте ещё раз.";
  }

  if (normalized.startsWith("{") || normalized.startsWith("<!DOCTYPE") || normalized.startsWith("<html")) {
    return "Сервис временно недоступен. Попробуйте ещё раз позже.";
  }

  return normalized
    .replace(/\s+/g, " ")
    .replace(/\.$/, "");
}

function humanizePropertyName(propertyName: string | null | undefined) {
  if (!propertyName) {
    return null;
  }

  return apiPropertyTranslations[propertyName] ?? propertyName;
}

function formatApiError(status: number, text: string) {
  if (!text) {
    return status >= 500
      ? "Сервис временно недоступен. Попробуйте ещё раз позже."
      : "Не удалось выполнить запрос. Проверьте данные и попробуйте ещё раз.";
  }

  try {
    const payload = JSON.parse(text) as ApiErrorPayload;
    const parts: string[] = [];

    if (payload.detail) {
      parts.push(payload.detail);
    } else if (payload.title) {
      parts.push(payload.title);
    }

    const fieldErrors = (payload.errors ?? [])
      .map((error) => {
        const message = error.message ?? error.errorMessage;
        if (!message) {
          return null;
        }

        const readableMessage = humanizeApiMessage(message);
        const readableProperty = humanizePropertyName(error.propertyName);

        return error.propertyName
          ? `${readableProperty}: ${readableMessage}`
          : readableMessage;
      })
      .filter((value): value is string => Boolean(value));

    if (fieldErrors.length > 0) {
      parts.push(fieldErrors.join("; "));
    }

    if (parts.length > 0) {
      return [...new Set(parts.map(humanizeApiMessage))].join(" ");
    }
  } catch {
    // Ignore parse errors and fall back to raw response text.
  }

  return humanizeApiMessage(text) || (
    status >= 500
      ? "Сервис временно недоступен. Попробуйте ещё раз позже."
      : "Не удалось выполнить запрос. Проверьте данные и попробуйте ещё раз."
  );
}

function parseJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const payload = token.split(".")[1];
    if (!payload) return null;

    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");
    const decoded = atob(padded);
    return JSON.parse(decoded) as Record<string, unknown>;
  } catch {
    return null;
  }
}

function isTokenExpired(token: string) {
  const payload = parseJwtPayload(token);
  const exp = typeof payload?.exp === "number" ? payload.exp : null;
  if (!exp) return false;

  const nowInSeconds = Math.floor(Date.now() / 1000);
  return exp <= nowInSeconds + 15;
}

export class ApiClient {
  constructor(private options: ApiClientOptions) {}

  async get<T>(path: string, schema: z.ZodType<T>, options?: RequestOptions): Promise<T> {
    return this.request("GET", path, schema, undefined, 0, options);
  }

  async post<TBody, TResponse>(
    path: string,
    body: TBody,
    schema: z.ZodType<TResponse>,
    options?: RequestOptions,
  ): Promise<TResponse> {
    return this.request("POST", path, schema, body, 0, options);
  }

  async postVoid<TBody>(path: string, body: TBody, options?: RequestOptions): Promise<void> {
    await this.request("POST", path, z.void(), body, 0, options);
  }

  async put<TBody, TResponse>(
    path: string,
    body: TBody,
    schema: z.ZodType<TResponse>,
    options?: RequestOptions,
  ): Promise<TResponse> {
    return this.request("PUT", path, schema, body, 0, options);
  }

  async patch<TBody, TResponse>(
    path: string,
    body: TBody,
    schema: z.ZodType<TResponse>,
    options?: RequestOptions,
  ): Promise<TResponse> {
    return this.request("PATCH", path, schema, body, 0, options);
  }

  async postFormVoid(path: string, body: FormData, options?: RequestOptions): Promise<void> {
    await this.request("POST", path, z.void(), body, 0, options);
  }

  async deleteVoid(path: string, options?: RequestOptions): Promise<void> {
    await this.request("DELETE", path, z.void(), undefined, 0, options);
  }

  async delete<T>(path: string, schema: z.ZodType<T>, options?: RequestOptions): Promise<T> {
    return this.request("DELETE", path, schema, undefined, 0, options);
  }

  private async getValidAccessToken() {
    const token = this.options.authSession
      ? await this.options.authSession.getAccessToken()
      : null;

    if (!token) {
      return null;
    }

    if (!isTokenExpired(token)) {
      return token;
    }

    if (!this.options.authSession?.refreshAccessToken) {
      return token;
    }

    return this.options.authSession.refreshAccessToken();
  }

  private async request<T>(
    method: string,
    path: string,
    schema: z.ZodType<T>,
    body?: unknown,
    attempt = 0,
    options?: RequestOptions,
  ): Promise<T> {
    const url = `${this.options.baseUrl}${path}`;
    const isFormData = typeof FormData !== "undefined" && body instanceof FormData;
    const token =
      options?.authMode === "none"
        ? null
        : await this.getValidAccessToken();

    const headers: Record<string, string> = {
      ...(!isFormData && body !== undefined ? { "Content-Type": "application/json" } : {}),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    };

    const res = await fetch(url, {
      method,
      headers,
      body:
        body === undefined
          ? undefined
          : isFormData
            ? body
            : JSON.stringify(body),
    });

    if (
      options?.authMode !== "none" &&
      res.status === 401 &&
      attempt === 0 &&
      this.options.authSession?.refreshAccessToken
    ) {
      const refreshedToken = await this.options.authSession.refreshAccessToken();
      if (refreshedToken) {
        return this.request(method, path, schema, body, attempt + 1, options);
      }

      await this.options.authSession.clearAuth?.();
    }

    if (!res.ok) {
      const text = await res.text().catch(() => "");
      throw new Error(formatApiError(res.status, text));
    }

    if (res.status === 204) {
      return schema.parse(undefined);
    }

    const text = await res.text();
    if (!text) {
      return schema.parse(undefined);
    }

    const json = JSON.parse(text) as unknown;
    return schema.parse(json);
  }
}
