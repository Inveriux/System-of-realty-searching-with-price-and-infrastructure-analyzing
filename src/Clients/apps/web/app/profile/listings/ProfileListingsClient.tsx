"use client";

import { deleteListing } from "@diploma/api/listings";
import { useListingsQuery } from "@diploma/features";
import { useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useMemo, useState } from "react";
import { AppShell } from "../../components/AppShell";
import { useAuth } from "../../providers";
import { ListingGrid } from "../../listings/components/ListingGrid";
import { mapApiListingToCard } from "../../listings/listing-card.mapper";
import { listingsPageClasses } from "../../listings/styles/listings-page.styles";

export function ProfileListingsClient() {
  const queryClient = useQueryClient();
  const { api, status, user } = useAuth();
  const { data, isLoading, error } = useListingsQuery(api);
  const [deletingId, setDeletingId] = useState<string | null>(null);

  async function handleDelete(id: string) {
    try {
      setDeletingId(id);
      await deleteListing(api, id);
      await queryClient.invalidateQueries({ queryKey: ["listings"] });
      await queryClient.invalidateQueries({ queryKey: ["search-listings"] });
      await queryClient.invalidateQueries({ queryKey: ["search-listings-map"] });
    } finally {
      setDeletingId(null);
    }
  }

  const ownListings = useMemo(() => {
    if (!user) {
      return [];
    }

    return (data?.items ?? []).filter((listing) => listing.agentId === user.id);
  }, [data?.items, user]);

  const items = ownListings.map((listing) =>
    mapApiListingToCard(listing, {
      canDelete: true,
      deletingId,
      onDelete: handleDelete,
    }),
  );

  if (status !== "authenticated" || !user) {
    return (
      <AppShell>
        <section className="surface grid gap-4 rounded-[28px] p-7">
          <div className="eyebrow">Мои объявления</div>
          <h1 className="section-title">Войдите в аккаунт</h1>
          <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 680 }}>
            Каталог ваших объектов и удаление объявлений доступны только после входа.
          </p>
          <div>
            <Link className="button-primary" href="/listings">
              Перейти к каталогу
            </Link>
          </div>
        </section>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <main className={listingsPageClasses.main}>
        <section className={listingsPageClasses.section}>
          <div className={listingsPageClasses.headingWrap}>
            <div className="grid gap-3">
              <div className="eyebrow">Личный каталог</div>
              <h1 className="section-title" style={{ marginTop: 0 }}>
                Мои объявления
              </h1>
              <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 720 }}>
                Здесь собраны все ваши объекты. Их можно открыть, просмотреть как каталог и удалить через корзину.
              </p>
            </div>

            <Link className="button-secondary" href="/profile">
              К профилю
            </Link>
          </div>

          {error ? (
            <div className={listingsPageClasses.error}>
              Ошибка: {String(error.message)}
            </div>
          ) : null}

          {!isLoading && items.length === 0 ? (
            <section className="surface grid gap-4 rounded-[28px] p-7">
              <div className="eyebrow">Пусто</div>
              <h2 className="section-title" style={{ marginTop: 0 }}>
                У вас пока нет объявлений
              </h2>
              <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 620 }}>
                Создайте новый объект, и он появится здесь отдельной карточкой в вашем каталоге.
              </p>
              <div>
                <Link className="button-primary" href="/create-listing">
                  Добавить объявление
                </Link>
              </div>
            </section>
          ) : (
            <ListingGrid items={items} isLoading={isLoading && items.length === 0} />
          )}
        </section>
      </main>
    </AppShell>
  );
}
