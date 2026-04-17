"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useEffect } from "react";

const ACCESS_TOKEN_COOKIE = "diploma_access_token";
const REFRESH_TOKEN_COOKIE = "diploma_refresh_token";

function writeCookie(name: string, value: string, days: number) {
  const expires = new Date(Date.now() + days * 24 * 60 * 60 * 1000).toUTCString();
  document.cookie = `${name}=${encodeURIComponent(value)}; path=/; expires=${expires}; SameSite=Lax`;
}

export default function GoogleAuthCallbackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();

  useEffect(() => {
    const accessToken = searchParams.get("accessToken");
    const refreshToken = searchParams.get("refreshToken");
    const error = searchParams.get("error");
    const returnUrl = searchParams.get("returnUrl") || "/listings";

    if (error) {
      router.replace(`/listings?googleAuthError=${encodeURIComponent(error)}`);
      return;
    }

    if (!accessToken || !refreshToken) {
      router.replace("/listings?googleAuthError=missing_tokens");
      return;
    }

    writeCookie(ACCESS_TOKEN_COOKIE, accessToken, 1);
    writeCookie(REFRESH_TOKEN_COOKIE, refreshToken, 7);
    window.location.replace(returnUrl);
  }, [router, searchParams]);

  return (
    <main className="grid gap-4 pb-10">
      <section className="surface rounded-[28px] p-8">
        Завершаем вход через Google...
      </section>
    </main>
  );
}
