"use client";

import { deleteListing } from "@diploma/api/listings";
import { useSearchListingsQuery } from "@diploma/features";
import type { SearchListingsRequest, SearchSortBy } from "@diploma/contracts/search";
import { useQueryClient } from "@tanstack/react-query";
import { useDeferredValue, useMemo, useState } from "react";
import { AppShell } from "../components/AppShell";
import { useAuth } from "../providers";
import { ListingGrid } from "./components/ListingGrid";
import { ListingsMapOverview } from "./components/ListingsMapOverview";
import { getSearchPropertyTypeLabel, mapSearchListingToCard } from "./search-listing-card.mapper";
import { listingsPageClasses } from "./styles/listings-page.styles";

type SearchFormState = {
  query: string;
  listingType: string;
  minPrice: string;
  maxPrice: string;
  minArea: string;
  maxArea: string;
  minInfrastructureScore: string;
  minEducationCount: string;
  minTransportCount: string;
  minHealthcareCount: string;
  minCommerceCount: string;
  minLeisureCount: string;
  maxTransportNearestMeters: string;
  maxEducationNearestMeters: string;
  maxHealthcareNearestMeters: string;
  sortBy: SearchSortBy;
  page: number;
  pageSize: number;
  searchInMapArea: boolean;
  boundsMinLatitude: number | null;
  boundsMinLongitude: number | null;
  boundsMaxLatitude: number | null;
  boundsMaxLongitude: number | null;
  centerLatitude: number | null;
  centerLongitude: number | null;
  radiusMeters: string;
};

const initialState: SearchFormState = {
  query: "",
  listingType: "",
  minPrice: "",
  maxPrice: "",
  minArea: "",
  maxArea: "",
  minInfrastructureScore: "",
  minEducationCount: "",
  minTransportCount: "",
  minHealthcareCount: "",
  minCommerceCount: "",
  minLeisureCount: "",
  maxTransportNearestMeters: "",
  maxEducationNearestMeters: "",
  maxHealthcareNearestMeters: "",
  sortBy: "published_desc",
  page: 0,
  pageSize: 12,
  searchInMapArea: false,
  boundsMinLatitude: null,
  boundsMinLongitude: null,
  boundsMaxLatitude: null,
  boundsMaxLongitude: null,
  centerLatitude: null,
  centerLongitude: null,
  radiusMeters: "",
};

function parseOptionalNumber(value: string) {
  if (!value.trim()) {
    return null;
  }

  const normalized = Number(value.replace(",", "."));
  return Number.isFinite(normalized) ? normalized : null;
}

export function ListingsClient() {
  const queryClient = useQueryClient();
  const { api, user } = useAuth();
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [isMapOverviewOpen, setIsMapOverviewOpen] = useState(false);
  const [isFiltersModalOpen, setIsFiltersModalOpen] = useState(false);
  const [filters, setFilters] = useState<SearchFormState>(initialState);
  const isAdmin = user?.role?.toLowerCase() === "admin";
  const searchRequest = useMemo<SearchListingsRequest>(() => ({
    query: filters.query.trim() || null,
    listingType: filters.listingType || null,
    minPrice: parseOptionalNumber(filters.minPrice),
    maxPrice: parseOptionalNumber(filters.maxPrice),
    minArea: parseOptionalNumber(filters.minArea),
    maxArea: parseOptionalNumber(filters.maxArea),
    minInfrastructureScore: parseOptionalNumber(filters.minInfrastructureScore),
    minEducationCount: parseOptionalNumber(filters.minEducationCount),
    minTransportCount: parseOptionalNumber(filters.minTransportCount),
    minHealthcareCount: parseOptionalNumber(filters.minHealthcareCount),
    minCommerceCount: parseOptionalNumber(filters.minCommerceCount),
    minLeisureCount: parseOptionalNumber(filters.minLeisureCount),
    maxTransportNearestMeters: parseOptionalNumber(filters.maxTransportNearestMeters),
    maxEducationNearestMeters: parseOptionalNumber(filters.maxEducationNearestMeters),
    maxHealthcareNearestMeters: parseOptionalNumber(filters.maxHealthcareNearestMeters),
    boundsMinLatitude: null,
    boundsMinLongitude: null,
    boundsMaxLatitude: null,
    boundsMaxLongitude: null,
    centerLatitude:
      filters.centerLatitude != null && parseOptionalNumber(filters.radiusMeters) != null
        ? filters.centerLatitude
        : null,
    centerLongitude:
      filters.centerLongitude != null && parseOptionalNumber(filters.radiusMeters) != null
        ? filters.centerLongitude
        : null,
    radiusMeters:
      filters.centerLatitude != null && filters.centerLongitude != null
        ? parseOptionalNumber(filters.radiusMeters)
        : null,
    sortBy: filters.sortBy,
    page: filters.page,
    pageSize: filters.pageSize,
  }), [filters]);
  const deferredSearchRequest = useDeferredValue(searchRequest);
  const { data, isLoading, error } = useSearchListingsQuery(api, deferredSearchRequest);

  async function handleDelete(id: string) {
    try {
      setDeletingId(id);
      await deleteListing(api, id);
      await queryClient.invalidateQueries({ queryKey: ["search-listings"] });
      await queryClient.invalidateQueries({ queryKey: ["listings"] });
    } finally {
      setDeletingId(null);
    }
  }

  const items = (data?.items ?? []).map((listing) =>
    mapSearchListingToCard(listing, {
      canDelete: isAdmin || listing.agentId === user?.id,
      deletingId,
      onDelete: handleDelete,
    }),
  );
  const mapListings = (data?.items ?? [])
    .filter((listing) => listing.latitude != null && listing.longitude != null)
    .map((listing) => ({
      id: listing.listingId,
      title: listing.title,
      href: `/listings/${listing.listingId}`,
      addressText: listing.addressText,
      propertyType: getSearchPropertyTypeLabel(listing),
      latitude: listing.latitude as number,
      longitude: listing.longitude as number,
      priceText: `${new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 0 }).format(listing.priceAmount ?? 0)} ${listing.priceCurrency ?? "BYN"}`,
    }));
  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / filters.pageSize));
  const activeFiltersCount = [
    filters.query.trim(),
    filters.listingType,
    filters.minPrice,
    filters.maxPrice,
    filters.minArea,
    filters.maxArea,
    filters.minInfrastructureScore,
    filters.minEducationCount,
    filters.minTransportCount,
    filters.minHealthcareCount,
    filters.minCommerceCount,
    filters.minLeisureCount,
    filters.maxTransportNearestMeters,
    filters.maxEducationNearestMeters,
    filters.maxHealthcareNearestMeters,
    filters.radiusMeters,
    filters.centerLatitude != null && filters.centerLongitude != null ? "point" : "",
    filters.sortBy !== initialState.sortBy ? filters.sortBy : "",
  ].filter(Boolean).length;

  function updateFilter<K extends keyof SearchFormState>(key: K, value: SearchFormState[K]) {
    setFilters((current) => ({
      ...current,
      [key]: value,
      page: key === "page" ? (value as SearchFormState["page"]) : 0,
    }));
  }

  return (
    <AppShell>
      <main className={listingsPageClasses.main}>
        <section className="surface grid gap-5 rounded-[30px] p-6">
          <div className="grid gap-3">
            <h1 className="section-title" style={{ marginTop: 0 }}>
              Каталог и поиск объявлений
            </h1>
          </div>

          <div
            style={{
              display: "grid",
              gap: 16,
              gridTemplateColumns: "minmax(0, 1.35fr) minmax(280px, 0.9fr)",
              alignItems: "stretch",
            }}
          >
            <div
              style={{
                borderRadius: 26,
                padding: 22,
                background:
                  "linear-gradient(135deg, color-mix(in srgb, var(--color-primary-soft) 68%, white), color-mix(in srgb, var(--background-elevated) 92%, white))",
                border: "1px solid color-mix(in srgb, var(--color-primary) 20%, var(--border-color))",
                display: "grid",
                gap: 14,
              }}
            >
              <div style={{ display: "grid", gap: 8 }}>
                <div style={{ fontSize: "clamp(1.2rem, 2vw, 1.8rem)", fontWeight: 900, lineHeight: 1.05 }}>
                  Настрой поиск в отдельном окне и вводи параметры там
                </div>
              </div>

              <div style={{ display: "flex", gap: 12, flexWrap: "wrap", alignItems: "center" }}>
                <button
                  className="button-primary"
                  type="button"
                  onClick={() => setIsFiltersModalOpen(true)}
                >
                  Открыть фильтр
                  {activeFiltersCount > 0 ? ` · ${activeFiltersCount}` : ""}
                </button>
                <button
                  className="button-secondary"
                  type="button"
                  onClick={() => setFilters(initialState)}
                >
                  Сбросить
                </button>
                <button
                  className="button-secondary"
                  type="button"
                  onClick={() => setIsMapOverviewOpen(true)}
                >
                  Карта объектов
                </button>
              </div>
            </div>

            <div
              className="surface"
              style={{
                borderRadius: 26,
                padding: 20,
                display: "grid",
                gap: 12,
                alignContent: "start",
              }}
            >
              <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                {activeFiltersCount > 0 ? (
                  <>
                    {filters.query.trim() ? (
                      <span className="surface" style={{ borderRadius: 999, padding: "8px 12px", fontSize: 13 }}>
                        Запрос: {filters.query.trim()}
                      </span>
                    ) : null}
                    {filters.listingType ? (
                      <span className="surface" style={{ borderRadius: 999, padding: "8px 12px", fontSize: 13 }}>
                        Сделка: {filters.listingType === "Sale" ? "Продажа" : "Аренда"}
                      </span>
                    ) : null}
                    {filters.minPrice || filters.maxPrice ? (
                      <span className="surface" style={{ borderRadius: 999, padding: "8px 12px", fontSize: 13 }}>
                        Цена: {filters.minPrice || "0"} - {filters.maxPrice || "∞"}
                      </span>
                    ) : null}
                    {filters.minArea || filters.maxArea ? (
                      <span className="surface" style={{ borderRadius: 999, padding: "8px 12px", fontSize: 13 }}>
                        Площадь: {filters.minArea || "0"} - {filters.maxArea || "∞"} м²
                      </span>
                    ) : null}
                    {filters.centerLatitude != null && filters.centerLongitude != null ? (
                      <span className="surface" style={{ borderRadius: 999, padding: "8px 12px", fontSize: 13 }}>
                        Точка на карте
                      </span>
                    ) : null}
                    {filters.radiusMeters && filters.centerLatitude != null && filters.centerLongitude != null ? (
                      <span className="surface" style={{ borderRadius: 999, padding: "8px 12px", fontSize: 13 }}>
                        Радиус: {filters.radiusMeters} м
                      </span>
                    ) : null}
                    {filters.sortBy !== initialState.sortBy ? (
                      <span className="surface" style={{ borderRadius: 999, padding: "8px 12px", fontSize: 13 }}>
                        Сортировка: {filters.sortBy}
                      </span>
                    ) : null}
                  </>
                ) : (
                  <p style={{ margin: 0, color: "var(--foreground-muted)", lineHeight: 1.6 }}>
                    Фильтры не заданы. Открой модальное окно и введи нужные параметры поиска.
                  </p>
                )}
              </div>
            </div>
          </div>
        </section>

        <section className={listingsPageClasses.section}>
          <div className={listingsPageClasses.headingWrap}>
            <div className="grid gap-2">
              <h2 className="section-title" style={{ marginTop: 8 }}>
                Объявлений найдено: {data?.totalCount ?? 0}
              </h2>
              <p style={{ margin: 0, color: "var(--foreground-muted)" }}>
                Страница {Math.min((data?.page ?? 0) + 1, totalPages)} из {totalPages}
              </p>
            </div>
          </div>

          {error ? (
            <div className={listingsPageClasses.error}>
              Ошибка: {String(error.message)}
            </div>
          ) : null}

          <ListingGrid items={items} isLoading={isLoading && items.length === 0} />

          <div style={{ display: "flex", gap: 12, alignItems: "center", justifyContent: "space-between", flexWrap: "wrap" }}>
            <div style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
              Показано {(data?.items?.length ?? 0)} из {data?.totalCount ?? 0}
            </div>

            <div style={{ display: "flex", gap: 10, alignItems: "center", flexWrap: "wrap" }}>
              <select
                className="field"
                value={String(filters.pageSize)}
                onChange={(event) => updateFilter("pageSize", Number(event.target.value))}
                style={{ width: 120 }}
              >
                <option value="12">12 / стр.</option>
                <option value="24">24 / стр.</option>
                <option value="36">36 / стр.</option>
              </select>
              <button
                className="button-secondary"
                type="button"
                disabled={filters.page <= 0}
                onClick={() => updateFilter("page", Math.max(0, filters.page - 1))}
              >
                Назад
              </button>
              <button
                className="button-secondary"
                type="button"
                disabled={filters.page + 1 >= totalPages}
                onClick={() => updateFilter("page", filters.page + 1)}
              >
                Дальше
              </button>
            </div>
          </div>
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
        selectedCenter={
          filters.centerLatitude != null && filters.centerLongitude != null
            ? {
                latitude: filters.centerLatitude,
                longitude: filters.centerLongitude,
              }
            : null
        }
        radiusMeters={parseOptionalNumber(filters.radiusMeters)}
        onSelectCenter={(point) =>
          setFilters((current) => ({
            ...current,
            page: 0,
            searchInMapArea: false,
            boundsMinLatitude: null,
            boundsMinLongitude: null,
            boundsMaxLatitude: null,
            boundsMaxLongitude: null,
            centerLatitude: point.latitude,
            centerLongitude: point.longitude,
            radiusMeters: current.radiusMeters || "1500",
          }))
        }
        onClearCenter={() =>
          setFilters((current) => ({
            ...current,
            page: 0,
            searchInMapArea: false,
            boundsMinLatitude: null,
            boundsMinLongitude: null,
            boundsMaxLatitude: null,
            boundsMaxLongitude: null,
            centerLatitude: null,
            centerLongitude: null,
          }))
        }
      />

      {isFiltersModalOpen ? (
        <div
          className="listing-create-modal-backdrop"
          onClick={() => setIsFiltersModalOpen(false)}
        >
          <div
            className="listing-create-modal surface"
            onClick={(event) => event.stopPropagation()}
            style={{ width: "min(1180px, 100%)", maxHeight: "calc(100vh - 48px)", overflow: "auto" }}
          >
            <div style={{ display: "flex", gap: 16, justifyContent: "space-between", alignItems: "start", flexWrap: "wrap" }}>
              <div className="grid gap-2">
                <h2 className="section-title" style={{ marginTop: 0, marginBottom: 0 }}>
                  Параметры подбора объявлений
                </h2>
                <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 760 }}>
                  Ввод всех параметров перенесён сюда: цена, площадь, инфраструктура, сортировка и поиск вокруг выбранной точки на карте.
                </p>
              </div>
              <button
                className="button-secondary"
                type="button"
                onClick={() => setIsFiltersModalOpen(false)}
              >
                Закрыть
              </button>
            </div>

            <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
              <input
                className="field"
                value={filters.query}
                onChange={(event) => updateFilter("query", event.target.value)}
                placeholder="Поиск по названию, адресу, описанию"
              />
              <select
                className="field"
                value={filters.listingType}
                onChange={(event) => updateFilter("listingType", event.target.value)}
              >
                <option value="">Все типы сделки</option>
                <option value="Sale">Продажа</option>
                <option value="Rent">Аренда</option>
              </select>
              <input
                className="field"
                value={filters.minPrice}
                onChange={(event) => updateFilter("minPrice", event.target.value)}
                placeholder="Цена от"
                inputMode="decimal"
              />
              <input
                className="field"
                value={filters.maxPrice}
                onChange={(event) => updateFilter("maxPrice", event.target.value)}
                placeholder="Цена до"
                inputMode="decimal"
              />
              <input
                className="field"
                value={filters.minArea}
                onChange={(event) => updateFilter("minArea", event.target.value)}
                placeholder="Площадь от, м²"
                inputMode="decimal"
              />
              <input
                className="field"
                value={filters.maxArea}
                onChange={(event) => updateFilter("maxArea", event.target.value)}
                placeholder="Площадь до, м²"
                inputMode="decimal"
              />
              <input
                className="field"
                value={filters.minInfrastructureScore}
                onChange={(event) => updateFilter("minInfrastructureScore", event.target.value)}
                placeholder="Инфраструктура от"
                inputMode="decimal"
              />
              <select
                className="field"
                value={filters.sortBy}
                onChange={(event) => updateFilter("sortBy", event.target.value as SearchSortBy)}
              >
                <option value="published_desc">Сначала новые</option>
                <option value="price_asc">Цена по возрастанию</option>
                <option value="price_desc">Цена по убыванию</option>
                <option value="infrastructure_desc">Сильная инфраструктура</option>
              </select>
              <input
                className="field"
                value={filters.minEducationCount}
                onChange={(event) => updateFilter("minEducationCount", event.target.value)}
                placeholder="Мин. объектов образования"
                inputMode="numeric"
              />
              <input
                className="field"
                value={filters.minTransportCount}
                onChange={(event) => updateFilter("minTransportCount", event.target.value)}
                placeholder="Мин. транспорта рядом"
                inputMode="numeric"
              />
              <input
                className="field"
                value={filters.minHealthcareCount}
                onChange={(event) => updateFilter("minHealthcareCount", event.target.value)}
                placeholder="Мин. медицины рядом"
                inputMode="numeric"
              />
              <input
                className="field"
                value={filters.minCommerceCount}
                onChange={(event) => updateFilter("minCommerceCount", event.target.value)}
                placeholder="Мин. коммерции рядом"
                inputMode="numeric"
              />
              <input
                className="field"
                value={filters.minLeisureCount}
                onChange={(event) => updateFilter("minLeisureCount", event.target.value)}
                placeholder="Мин. досуга рядом"
                inputMode="numeric"
              />
              <input
                className="field"
                value={filters.maxTransportNearestMeters}
                onChange={(event) => updateFilter("maxTransportNearestMeters", event.target.value)}
                placeholder="Транспорт не дальше, м"
                inputMode="numeric"
              />
              <input
                className="field"
                value={filters.maxEducationNearestMeters}
                onChange={(event) => updateFilter("maxEducationNearestMeters", event.target.value)}
                placeholder="Образование не дальше, м"
                inputMode="numeric"
              />
              <input
                className="field"
                value={filters.maxHealthcareNearestMeters}
                onChange={(event) => updateFilter("maxHealthcareNearestMeters", event.target.value)}
                placeholder="Медицина не дальше, м"
                inputMode="numeric"
              />
            </div>

            <div style={{ display: "flex", gap: 12, flexWrap: "wrap", alignItems: "center" }}>
              <input
                className="field"
                value={filters.radiusMeters}
                onChange={(event) => updateFilter("radiusMeters", event.target.value)}
                placeholder="Радиус вокруг точки, м"
                inputMode="numeric"
                style={{ maxWidth: 180 }}
              />
              <button
                className="button-secondary"
                type="button"
                onClick={() => setIsMapOverviewOpen(true)}
              >
                Выбрать точку на карте
              </button>
              {filters.centerLatitude != null && filters.centerLongitude != null ? (
                <div style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
                  Точка: {filters.centerLatitude.toFixed(5)}, {filters.centerLongitude.toFixed(5)}
                </div>
              ) : (
                <div style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
                  Точка не выбрана. Кликни по карте, чтобы искать рядом с меткой.
                </div>
              )}
            </div>

            <div style={{ display: "flex", gap: 12, justifyContent: "space-between", flexWrap: "wrap" }}>
              <div style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
                Активных параметров: {activeFiltersCount}
              </div>
              <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
                <button
                  className="button-secondary"
                  type="button"
                  onClick={() => setFilters(initialState)}
                >
                  Сбросить
                </button>
                <button
                  className="button-primary"
                  type="button"
                  onClick={() => setIsFiltersModalOpen(false)}
                >
                  Показать результаты
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : null}
    </AppShell>
  );
}
