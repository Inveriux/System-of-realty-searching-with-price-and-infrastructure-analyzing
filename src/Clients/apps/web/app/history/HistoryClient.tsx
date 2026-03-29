"use client";

import { useEffect, useState } from "react";
import { AppShell } from "../components/AppShell";
import { ListingsClient } from "../listings/ListingsClient";
import { useViewedListings } from "../listings/lib/listing-collections";
import { useAuth } from "../providers";

export function HistoryClient() {
  const { status, user } = useAuth();
  const viewed = useViewedListings();
  const [isMounted, setIsMounted] = useState(false);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  if (!isMounted) {
    return (
      <AppShell>
        <section className="surface grid gap-4 rounded-[28px] p-7">
          <div className="eyebrow">История</div>
          <h1 className="section-title">Загрузка...</h1>
        </section>
      </AppShell>
    );
  }

  if (status !== "authenticated" || !user) {
    return (
      <AppShell>
        <section className="surface grid gap-4 rounded-[28px] p-7">
          <div className="eyebrow">История</div>
          <h1 className="section-title">Войдите в аккаунт</h1>
          <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 680 }}>
            История просмотра доступна только после входа.
          </p>
        </section>
      </AppShell>
    );
  }

  return (
    <ListingsClient
      title="История просмотра"
      eyebrow="История"
      subtitle="Здесь собраны объявления, которые вы уже открывали. Фильтры и сортировка совпадают с главным каталогом."
      forcedListingIds={viewed.viewedIds}
      emptyTitle="История просмотра пока пустая"
      emptyDescription="Откройте несколько объявлений, и они появятся здесь автоматически."
    />
  );
}
