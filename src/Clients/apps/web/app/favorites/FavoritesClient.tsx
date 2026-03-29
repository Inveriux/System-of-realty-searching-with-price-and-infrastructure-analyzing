"use client";

import { useEffect, useState } from "react";
import { AppShell } from "../components/AppShell";
import { ListingsClient } from "../listings/ListingsClient";
import { useFavoriteListings } from "../listings/lib/listing-collections";
import { useAuth } from "../providers";

export function FavoritesClient() {
  const { status, user } = useAuth();
  const favorites = useFavoriteListings();
  const [isMounted, setIsMounted] = useState(false);

  useEffect(() => {
    setIsMounted(true);
  }, []);

  if (!isMounted) {
    return (
      <AppShell>
        <section className="surface grid gap-4 rounded-[28px] p-7">
          <div className="eyebrow">Избранное</div>
          <h1 className="section-title">Загрузка...</h1>
        </section>
      </AppShell>
    );
  }

  if (status !== "authenticated" || !user) {
    return (
      <AppShell>
        <section className="surface grid gap-4 rounded-[28px] p-7">
          <div className="eyebrow">Избранное</div>
          <h1 className="section-title">Войдите в аккаунт</h1>
          <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 680 }}>
            Сохраненные объекты доступны только после входа.
          </p>
        </section>
      </AppShell>
    );
  }

  return (
    <ListingsClient
      title="Избранное"
      eyebrow="Избранное"
      subtitle="Используются те же фильтры, что и в основном каталоге, но только по сохраненным объектам."
      forcedListingIds={favorites.favoriteIds}
      emptyTitle="В избранном пока пусто"
      emptyDescription="Добавьте объявления в избранное через сердечко на карточке, и они появятся здесь."
    />
  );
}
