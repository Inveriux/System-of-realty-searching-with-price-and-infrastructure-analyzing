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

  async postFormVoid(path: string, body: FormData, options?: RequestOptions): Promise<void> {
    await this.request("POST", path, z.void(), body, 0, options);
  }

  async deleteVoid(path: string, options?: RequestOptions): Promise<void> {
    await this.request("DELETE", path, z.void(), undefined, 0, options);
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
      throw new Error(`Failed to fetch API: ${res.status} ${text}`);
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
