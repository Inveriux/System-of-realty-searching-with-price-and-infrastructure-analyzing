"use client";

import { deleteListing } from "@diploma/api/listings";
import { useListingsQuery } from "@diploma/features";
import type { ListingDto } from "@diploma/contracts/listings";
import { useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { AppShell } from "../components/AppShell";
import { useAuth } from "../providers";
import { ListingGrid } from "./components/ListingGrid";
import { ListingsMapOverview } from "./components/ListingsMapOverview";
import type { ListingCardItem } from "./components/listing-card.types";
import { listingsPageClasses } from "./styles/listings-page.styles";

function mapApiListingToCard(
  listing: ListingDto,
  options: {
    canDelete: boolean;
    deletingId: string | null;
    onDelete: (id: string) => Promise<void>;
  },
): ListingCardItem {
  const listingType = listing.listingType === "Rent" ? "Аренда" : "Продажа";
  const href = `/listings/${listing.id}`;

  switch (listing.listingDetailsType) {
    case "House":
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "house",
          ...listing.listingDetails,
        },
      };
    case "Land":
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "land",
          ...listing.listingDetails,
        },
      };
    case "Office":
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "office",
          ...listing.listingDetails,
        },
      };
    case "Warehouse":
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "warehouse",
          ...listing.listingDetails,
        },
      };
    case "Apartment":
    default:
      return {
        id: listing.id,
        title: listing.title,
        addressText: listing.addressText ?? "Адрес уточняется",
        priceAmount: listing.priceAmountByn ?? listing.priceAmount ?? undefined,
        priceAmountUsd: listing.priceAmountUsd ?? undefined,
        priceCurrency: "BYN",
        pricePeriod: listing.pricePeriod ?? undefined,
        areaTotal: listing.areaTotal,
        mainMedia: listing.mainMedia,
        listingType,
        buildingType: listing.buildingType,
        publishedDate: listing.publishedDate,
        mediaCount: listing.media?.length ?? (listing.mainMedia ? 1 : 0),
        canDelete: options.canDelete,
        isDeleting: options.deletingId === listing.id,
        onDelete: options.onDelete,
        href,
        details: {
          kind: "apartment",
          ...listing.listingDetails,
        },
      };
  }
}

function getPropertyTypeLabel(listing: ListingDto) {
  switch (listing.listingDetailsType) {
    case "House":
      return "Дом";
    case "Land":
      return "Участок";
    case "Office":
      return "Офис";
    case "Warehouse":
      return "Склад";
    case "Apartment":
    default:
      return "Квартира";
  }
}

export function ListingsClient() {
  const queryClient = useQueryClient();
  const { api, user } = useAuth();
  const { data, isLoading, error } = useListingsQuery(api);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [isMapOverviewOpen, setIsMapOverviewOpen] = useState(false);
  const isAdmin = user?.role?.toLowerCase() === "admin";

  async function handleDelete(id: string) {
    try {
      setDeletingId(id);
      await deleteListing(api, id);
      await queryClient.invalidateQueries({ queryKey: ["listings"] });
    } finally {
      setDeletingId(null);
    }
  }

  const items = (data?.items ?? []).map((listing) =>
    mapApiListingToCard(listing, {
      canDelete: isAdmin,
      deletingId,
      onDelete: handleDelete,
    }),
  );
  const mapListings = (data?.items ?? [])
    .filter((listing) => listing.latitude != null && listing.longitude != null)
    .map((listing) => ({
      id: listing.id,
      title: listing.title,
      href: `/listings/${listing.id}`,
      addressText: listing.addressText,
      propertyType: getPropertyTypeLabel(listing),
      latitude: listing.latitude as number,
      longitude: listing.longitude as number,
      priceText: `${new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 0 }).format(listing.priceAmountByn ?? listing.priceAmount ?? 0)} BYN`,
    }));

  return (
    <AppShell>
      <main className={listingsPageClasses.main}>
        <section className={listingsPageClasses.section}>
          <div className={listingsPageClasses.headingWrap}>
            <div>
              <h2 className="section-title" style={{ marginTop: 8 }}>
                Объявлений найдено: {data?.totalCount ?? 0}
              </h2>
            </div>
          </div>

          {error ? (
            <div className={listingsPageClasses.error}>
              Ошибка: {String(error.message)}
            </div>
          ) : null}

          <ListingGrid items={items} isLoading={isLoading && items.length === 0} />
        </section>
      </main>

      <button
        type="button"
        onClick={() => setIsMapOverviewOpen(true)}
        className="surface"
        style={{
          position: "fixed",
          right: 22,
          bottom: 22,
          zIndex: 24,
          borderRadius: 24,
          padding: "18px 16px",
          display: "grid",
          gap: 10,
          justifyItems: "center",
          minWidth: 96,
          border: "1px solid var(--border-color)",
          background: "color-mix(in srgb, var(--background-elevated) 94%, white)",
        }}
        title="Обзор объектов на карте"
      >
        <span style={{ fontSize: "1.9rem", lineHeight: 1 }}>⌖</span>
        <span style={{ fontSize: 13, fontWeight: 800, lineHeight: 1.2 }}>На карте</span>
      </button>

      <ListingsMapOverview
        open={isMapOverviewOpen}
        onClose={() => setIsMapOverviewOpen(false)}
        listings={mapListings}
      />
    </AppShell>
  );
}
