"use client";

import { searchListings } from "@diploma/api/search";
import { deleteListing } from "@diploma/api/listings";
import { useSearchListingsQuery } from "@diploma/features";
import type {
  CommerceLevel,
  InfrastructureLevel,
  SearchListingsRequest,
  SearchSortBy,
  TransportLevel,
} from "@diploma/contracts/search";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useDeferredValue, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import { AppShell } from "../components/AppShell";
import { useAuth } from "../providers";
import { ListingGrid } from "./components/ListingGrid";
import { ListingsMapOverview } from "./components/ListingsMapOverview";
import { getSearchPropertyTypeLabel, mapSearchListingToCard } from "./search-listing-card.mapper";
import { listingsClientStyles } from "./styles/listings-client.styles";
import { listingsPageClasses } from "./styles/listings-page.styles";

const LISTINGS_PAGE_SIZE = 10;
const PAGINATION_WINDOW = 9;

type SearchFormState = {
  query: string;
  listingType: string;
  minPrice: string;
  maxPrice: string;
  minArea: string;
  maxArea: string;
  infrastructureLevel: InfrastructureLevel | "";
  transportLevel: TransportLevel;
  commerceLevel: CommerceLevel | "";
  needDailyCommerce: boolean;
  needStrongSurfaceTransport: boolean;
  needFamilyInfrastructure: boolean;
  needWalkableInfrastructure: boolean;
  minInfrastructureScore: string;
  minEducationCount: string;
  minTransportCount: string;
  minHealthcareCount: string;
  minCommerceCount: string;
  minLeisureCount: string;
  maxTransportNearestMeters: string;
  maxEducationNearestMeters: string;
  maxHealthcareNearestMeters: string;
  maxStoreWalkMinutes: string;
  maxPharmacyWalkMinutes: string;
  maxStopWalkMinutes: string;
  maxParkingWalkMinutes: string;
  maxSchoolWalkMinutes: string;
  maxKindergartenWalkMinutes: string;
  maxClinicDriveMinutes: string;
  maxHospitalDriveMinutes: string;
  maxMetroWalkMinutes: string;
  requireMetroInPrimaryRadius: boolean;
  sortBy: SearchSortBy;
  page: number;
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
  infrastructureLevel: "",
  transportLevel: "any",
  commerceLevel: "",
  needDailyCommerce: false,
  needStrongSurfaceTransport: false,
  needFamilyInfrastructure: false,
  needWalkableInfrastructure: false,
  minInfrastructureScore: "",
  minEducationCount: "",
  minTransportCount: "",
  minHealthcareCount: "",
  minCommerceCount: "",
  minLeisureCount: "",
  maxTransportNearestMeters: "",
  maxEducationNearestMeters: "",
  maxHealthcareNearestMeters: "",
  maxStoreWalkMinutes: "",
  maxPharmacyWalkMinutes: "",
  maxStopWalkMinutes: "",
  maxParkingWalkMinutes: "",
  maxSchoolWalkMinutes: "",
  maxKindergartenWalkMinutes: "",
  maxClinicDriveMinutes: "",
  maxHospitalDriveMinutes: "",
  maxMetroWalkMinutes: "",
  requireMetroInPrimaryRadius: false,
  sortBy: "published_desc",
  page: 0,
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

function getVisiblePageNumbers(currentPage: number, totalPages: number) {
  if (totalPages <= PAGINATION_WINDOW) {
    return Array.from({ length: totalPages }, (_, index) => index);
  }

  const halfWindow = Math.floor(PAGINATION_WINDOW / 2);
  let start = Math.max(0, currentPage - halfWindow);
  let end = start + PAGINATION_WINDOW - 1;

  if (end >= totalPages) {
    end = totalPages - 1;
    start = Math.max(0, end - PAGINATION_WINDOW + 1);
  }

  return Array.from({ length: end - start + 1 }, (_, index) => start + index);
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
    infrastructureLevel: filters.infrastructureLevel || null,
    transportLevel: filters.transportLevel === "any" ? null : filters.transportLevel,
    commerceLevel: filters.commerceLevel || null,
    needDailyCommerce: filters.needDailyCommerce ? true : null,
    needStrongSurfaceTransport: filters.needStrongSurfaceTransport ? true : null,
    needFamilyInfrastructure: filters.needFamilyInfrastructure ? true : null,
    needWalkableInfrastructure: filters.needWalkableInfrastructure ? true : null,
    minInfrastructureScore: parseOptionalNumber(filters.minInfrastructureScore),
    minEducationCount: parseOptionalNumber(filters.minEducationCount),
    minTransportCount: parseOptionalNumber(filters.minTransportCount),
    minHealthcareCount: parseOptionalNumber(filters.minHealthcareCount),
    minCommerceCount: parseOptionalNumber(filters.minCommerceCount),
    minLeisureCount: parseOptionalNumber(filters.minLeisureCount),
    maxTransportNearestMeters: parseOptionalNumber(filters.maxTransportNearestMeters),
    maxEducationNearestMeters: parseOptionalNumber(filters.maxEducationNearestMeters),
    maxHealthcareNearestMeters: parseOptionalNumber(filters.maxHealthcareNearestMeters),
    maxStoreWalkMinutes: parseOptionalNumber(filters.maxStoreWalkMinutes),
    maxPharmacyWalkMinutes: parseOptionalNumber(filters.maxPharmacyWalkMinutes),
    maxStopWalkMinutes: parseOptionalNumber(filters.maxStopWalkMinutes),
    maxParkingWalkMinutes: parseOptionalNumber(filters.maxParkingWalkMinutes),
    maxSchoolWalkMinutes: parseOptionalNumber(filters.maxSchoolWalkMinutes),
    maxKindergartenWalkMinutes: parseOptionalNumber(filters.maxKindergartenWalkMinutes),
    maxClinicDriveMinutes: parseOptionalNumber(filters.maxClinicDriveMinutes),
    maxHospitalDriveMinutes: parseOptionalNumber(filters.maxHospitalDriveMinutes),
    maxMetroWalkMinutes: parseOptionalNumber(filters.maxMetroWalkMinutes),
    requireMetroInPrimaryRadius: filters.requireMetroInPrimaryRadius ? true : null,
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
    includeHighlights: true,
    includeRelevanceBreakdown: false,
    sort: filters.sortBy,
    sortBy: filters.sortBy,
    page: filters.page,
    pageSize: LISTINGS_PAGE_SIZE,
  }), [filters]);
  const deferredSearchRequest = useDeferredValue(searchRequest);
  const { data, isLoading, error } = useSearchListingsQuery(api, deferredSearchRequest);
  const mapSearchRequest = useMemo<SearchListingsRequest>(() => ({
    ...searchRequest,
    page: 0,
    pageSize: Math.max(data?.totalCount ?? LISTINGS_PAGE_SIZE, LISTINGS_PAGE_SIZE),
  }), [data?.totalCount, searchRequest]);
  const deferredMapSearchRequest = useDeferredValue(mapSearchRequest);
  const { data: mapData } = useQuery({
    queryKey: ["search-listings-map", deferredMapSearchRequest],
    queryFn: () => searchListings(api, deferredMapSearchRequest),
  });

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
  const mapItemsSource =
    filters.page === 0 && (data?.totalCount ?? 0) <= LISTINGS_PAGE_SIZE
      ? (data?.items ?? [])
      : (mapData?.items ?? data?.items ?? []);
  const mapListings = mapItemsSource
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
      publishedDate: listing.publishedDate,
    }));
  const listingsWithoutCoordinates = (data?.items ?? []).filter(
    (listing) => listing.latitude == null || listing.longitude == null,
  ).length;
  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / LISTINGS_PAGE_SIZE));
  const visiblePages = getVisiblePageNumbers(filters.page, totalPages);
  const activeFiltersCount = [
    filters.query.trim(),
    filters.listingType,
    filters.minPrice,
    filters.maxPrice,
    filters.minArea,
    filters.maxArea,
    filters.infrastructureLevel,
    filters.transportLevel !== "any" ? filters.transportLevel : "",
    filters.commerceLevel,
    filters.needDailyCommerce ? "daily-commerce" : "",
    filters.needStrongSurfaceTransport ? "surface-transport" : "",
    filters.needFamilyInfrastructure ? "family" : "",
    filters.needWalkableInfrastructure ? "walkable" : "",
    filters.minInfrastructureScore,
    filters.minEducationCount,
    filters.minTransportCount,
    filters.minHealthcareCount,
    filters.minCommerceCount,
    filters.minLeisureCount,
    filters.maxTransportNearestMeters,
    filters.maxEducationNearestMeters,
    filters.maxHealthcareNearestMeters,
    filters.maxStoreWalkMinutes,
    filters.maxPharmacyWalkMinutes,
    filters.maxStopWalkMinutes,
    filters.maxParkingWalkMinutes,
    filters.maxSchoolWalkMinutes,
    filters.maxKindergartenWalkMinutes,
    filters.maxClinicDriveMinutes,
    filters.maxHospitalDriveMinutes,
    filters.maxMetroWalkMinutes,
    filters.requireMetroInPrimaryRadius ? "metro" : "",
    filters.radiusMeters,
    filters.centerLatitude != null && filters.centerLongitude != null ? "point" : "",
  ].filter(Boolean).length;

  function updateFilter<K extends keyof SearchFormState>(key: K, value: SearchFormState[K]) {
    setFilters((current) => ({
      ...current,
      [key]: value,
      page: key === "page" ? (value as SearchFormState["page"]) : 0,
    }));
  }

  const paginationBlock = totalPages > 1 ? (
    <div style={listingsClientStyles.footerRow}>
      <div style={listingsClientStyles.footerMeta}>
        Страница {Math.min((data?.page ?? 0) + 1, totalPages)} из {totalPages}
      </div>

      <div style={listingsClientStyles.footerActions}>
        <button
          className="button-secondary"
          type="button"
          disabled={filters.page <= 0}
          onClick={() => updateFilter("page", Math.max(0, filters.page - 1))}
        >
          Назад
        </button>
        <div className="flex flex-wrap gap-2">
          {visiblePages.map((pageNumber) => {
            const isActive = pageNumber === filters.page;

            return (
              <button
                key={pageNumber}
                type="button"
                onClick={() => updateFilter("page", pageNumber)}
                className={isActive ? "button-primary" : "button-secondary"}
                aria-current={isActive ? "page" : undefined}
              >
                {pageNumber + 1}
              </button>
            );
          })}
        </div>
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
  ) : null;

  return (
    <AppShell>
      <main className={listingsPageClasses.main}>
        <section className="surface grid gap-5 rounded-[30px] p-6">
          <div className="grid gap-3">
            <h1 className="section-title" style={listingsClientStyles.pageTitle}>
              Каталог и поиск объявлений
            </h1>
          </div>

          <div style={listingsClientStyles.heroGrid}>
            <div style={listingsClientStyles.heroPanel}>
              <div style={listingsClientStyles.heroCopy}>
                <div style={listingsClientStyles.heroTitle}>
                  Подберите объявления по нужным параметрам
                </div>
                <p style={listingsClientStyles.mutedParagraph}>
                  Основные параметры поиска и карта доступны в отдельной панели.
                </p>
              </div>

              <div style={listingsClientStyles.heroActions}>
                <button
                  className="button-primary"
                  type="button"
                  onClick={(event) => {
                    event.stopPropagation();
                    window.setTimeout(() => {
                      setIsFiltersModalOpen(true);
                    }, 0);
                  }}
                >
                  Параметры поиска
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
              style={listingsClientStyles.summaryPanel}
            >
              <div style={listingsClientStyles.summaryTags}>
                {activeFiltersCount > 0 ? (
                  <>
                    {filters.query.trim() ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Запрос: {filters.query.trim()}
                      </span>
                    ) : null}
                    {filters.listingType ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Сделка: {filters.listingType === "Sale" ? "Продажа" : "Аренда"}
                      </span>
                    ) : null}
                    {filters.minPrice || filters.maxPrice ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Цена: {filters.minPrice || "0"} - {filters.maxPrice || "∞"}
                      </span>
                    ) : null}
                    {filters.minArea || filters.maxArea ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Площадь: {filters.minArea || "0"} - {filters.maxArea || "∞"} м²
                      </span>
                    ) : null}
                    {filters.infrastructureLevel ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Инфраструктура: {filters.infrastructureLevel}
                      </span>
                    ) : null}
                    {filters.transportLevel !== "any" ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Транспорт: {filters.transportLevel}
                      </span>
                    ) : null}
                    {filters.commerceLevel ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Коммерция: {filters.commerceLevel}
                      </span>
                    ) : null}
                    {filters.needFamilyInfrastructure ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Для семьи
                      </span>
                    ) : null}
                    {filters.needWalkableInfrastructure ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Всё рядом пешком
                      </span>
                    ) : null}
                    {filters.needStrongSurfaceTransport ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Сильный наземный транспорт
                      </span>
                    ) : null}
                    {filters.needDailyCommerce ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Повседневная коммерция
                      </span>
                    ) : null}
                    {filters.centerLatitude != null && filters.centerLongitude != null ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Точка на карте
                      </span>
                    ) : null}
                    {filters.radiusMeters && filters.centerLatitude != null && filters.centerLongitude != null ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Радиус: {filters.radiusMeters} м
                      </span>
                    ) : null}
                  </>
                ) : (
                  <p style={listingsClientStyles.mutedParagraph}>
                    Дополнительные параметры не выбраны.
                  </p>
                )}
              </div>
            </div>
          </div>
        </section>

        <section className={listingsPageClasses.section}>
          <div className={listingsPageClasses.headingWrap}>
            <div className="grid gap-2">
              <h2 className="section-title" style={listingsClientStyles.headingTitle}>
                Объявлений найдено: {data?.totalCount ?? 0}
              </h2>
              <p style={listingsClientStyles.metaText}>
                Страница {Math.min((data?.page ?? 0) + 1, totalPages)} из {totalPages}
              </p>
              {listingsWithoutCoordinates > 0 ? (
                <p style={listingsClientStyles.metaText}>
                  Без координат на этой странице: {listingsWithoutCoordinates}
                </p>
              ) : null}
            </div>
          </div>

          {error ? (
            <div className={listingsPageClasses.error}>
              Ошибка: {String(error.message)}
            </div>
          ) : null}

          <div style={listingsClientStyles.footerRow}>
            <div style={listingsClientStyles.footerMeta}>
              Показано {(data?.items?.length ?? 0)} из {data?.totalCount ?? 0}
            </div>
            <div style={listingsClientStyles.footerActions}>
              <select
                className="field"
                value={filters.sortBy}
                onChange={(event) => updateFilter("sortBy", event.target.value as SearchSortBy)}
                style={{ minWidth: 220 }}
              >
                <option value="relevance">По релевантности</option>
                <option value="published_desc">Сначала новые</option>
                <option value="area_desc">Площадь больше</option>
                <option value="price_asc">Цена по возрастанию</option>
                <option value="price_desc">Цена по убыванию</option>
                <option value="infrastructure_desc">Лучшая инфраструктура</option>
              </select>
            </div>
          </div>

          {paginationBlock}

          <ListingGrid items={items} isLoading={isLoading && items.length === 0} />

          <div style={listingsClientStyles.footerRow}>
            <div style={listingsClientStyles.footerMeta}>
              Показано {(data?.items?.length ?? 0)} из {data?.totalCount ?? 0}
            </div>
          </div>

          {paginationBlock}
        </section>
      </main>

      <button
        type="button"
        onClick={() => setIsMapOverviewOpen(true)}
        className="surface"
        style={listingsClientStyles.floatingMapButton}
        title="Обзор объектов на карте"
      >
        <span style={listingsClientStyles.floatingMapIcon}>⌖</span>
        <span style={listingsClientStyles.floatingMapLabel}>На карте</span>
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

      {isFiltersModalOpen && typeof document !== "undefined"
        ? createPortal(
            <div
              className="listing-create-modal-backdrop"
              onClick={() => setIsFiltersModalOpen(false)}
            >
              <div
                className="listing-create-modal surface"
                onClick={(event) => event.stopPropagation()}
                style={listingsClientStyles.modalPanel}
              >
                <div style={listingsClientStyles.modalHeader}>
                  <div className="grid gap-2">
                    <h2 className="section-title" style={listingsClientStyles.modalHeading}>
                      Параметры поиска
                    </h2>
                    <p style={listingsClientStyles.mutedParagraph}>
                      Здесь можно уточнить цену, площадь, инфраструктуру и поиск вокруг выбранной точки на карте.
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
                  <select
                    className="field"
                    value={filters.infrastructureLevel}
                    onChange={(event) => updateFilter("infrastructureLevel", event.target.value as InfrastructureLevel | "")}
                  >
                    <option value="">Минимальная общая инфраструктура</option>
                    <option value="basic">Базовая</option>
                    <option value="good">Хорошая</option>
                    <option value="high">Высокая</option>
                    <option value="premium">Премиальная</option>
                  </select>
                  <select
                    className="field"
                    value={filters.transportLevel}
                    onChange={(event) => updateFilter("transportLevel", event.target.value as TransportLevel)}
                  >
                    <option value="any">Транспорт: любой</option>
                    <option value="basic">Транспорт: базовый</option>
                    <option value="good">Транспорт: хороший</option>
                    <option value="excellent">Транспорт: отличный</option>
                  </select>
                  <select
                    className="field"
                    value={filters.commerceLevel}
                    onChange={(event) => updateFilter("commerceLevel", event.target.value as CommerceLevel | "")}
                  >
                    <option value="">Коммерция: любая</option>
                    <option value="basic">Коммерция: базовая</option>
                    <option value="good">Коммерция: хорошая</option>
                    <option value="rich">Коммерция: насыщенная</option>
                  </select>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input
                      type="checkbox"
                      checked={filters.needFamilyInfrastructure}
                      onChange={(event) => updateFilter("needFamilyInfrastructure", event.target.checked)}
                    />
                    Для семьи
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input
                      type="checkbox"
                      checked={filters.needWalkableInfrastructure}
                      onChange={(event) => updateFilter("needWalkableInfrastructure", event.target.checked)}
                    />
                    Всё рядом пешком
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input
                      type="checkbox"
                      checked={filters.needStrongSurfaceTransport}
                      onChange={(event) => updateFilter("needStrongSurfaceTransport", event.target.checked)}
                    />
                    Сильный наземный транспорт
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input
                      type="checkbox"
                      checked={filters.needDailyCommerce}
                      onChange={(event) => updateFilter("needDailyCommerce", event.target.checked)}
                    />
                    Нужна повседневная коммерция
                  </label>
                </div>

                <div className="grid gap-2">
                  <h3 className="section-title" style={{ fontSize: 18 }}>
                    Расширенные фильтры
                  </h3>
                  <p style={listingsClientStyles.mutedParagraph}>
                    Здесь остаются точные ограничения по метрам, минутам и количеству.
                  </p>
                </div>

                <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
                  <input
                    className="field"
                    value={filters.minInfrastructureScore}
                    onChange={(event) => updateFilter("minInfrastructureScore", event.target.value)}
                    placeholder="Инфраструктура от"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.minEducationCount}
                    onChange={(event) => updateFilter("minEducationCount", event.target.value)}
                    placeholder="Объекты образования, от"
                    inputMode="numeric"
                  />
                  <input
                    className="field"
                    value={filters.minTransportCount}
                    onChange={(event) => updateFilter("minTransportCount", event.target.value)}
                    placeholder="Транспорт рядом, от"
                    inputMode="numeric"
                  />
                  <input
                    className="field"
                    value={filters.minHealthcareCount}
                    onChange={(event) => updateFilter("minHealthcareCount", event.target.value)}
                    placeholder="Медицина рядом, от"
                    inputMode="numeric"
                  />
                  <input
                    className="field"
                    value={filters.minCommerceCount}
                    onChange={(event) => updateFilter("minCommerceCount", event.target.value)}
                    placeholder="Коммерция рядом, от"
                    inputMode="numeric"
                  />
                  <input
                    className="field"
                    value={filters.minLeisureCount}
                    onChange={(event) => updateFilter("minLeisureCount", event.target.value)}
                    placeholder="Досуг рядом, от"
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
                  <input
                    className="field"
                    value={filters.maxStoreWalkMinutes}
                    onChange={(event) => updateFilter("maxStoreWalkMinutes", event.target.value)}
                    placeholder="Магазин: максимум минут пешком"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.maxPharmacyWalkMinutes}
                    onChange={(event) => updateFilter("maxPharmacyWalkMinutes", event.target.value)}
                    placeholder="Аптека: максимум минут пешком"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.maxStopWalkMinutes}
                    onChange={(event) => updateFilter("maxStopWalkMinutes", event.target.value)}
                    placeholder="Остановка: максимум минут пешком"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.maxParkingWalkMinutes}
                    onChange={(event) => updateFilter("maxParkingWalkMinutes", event.target.value)}
                    placeholder="Парковка: максимум минут пешком"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.maxSchoolWalkMinutes}
                    onChange={(event) => updateFilter("maxSchoolWalkMinutes", event.target.value)}
                    placeholder="Школа: максимум минут пешком"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.maxKindergartenWalkMinutes}
                    onChange={(event) => updateFilter("maxKindergartenWalkMinutes", event.target.value)}
                    placeholder="Детский сад: максимум минут пешком"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.maxClinicDriveMinutes}
                    onChange={(event) => updateFilter("maxClinicDriveMinutes", event.target.value)}
                    placeholder="Поликлиника: максимум минут на машине"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.maxHospitalDriveMinutes}
                    onChange={(event) => updateFilter("maxHospitalDriveMinutes", event.target.value)}
                    placeholder="Больница: максимум минут на машине"
                    inputMode="decimal"
                  />
                  <input
                    className="field"
                    value={filters.maxMetroWalkMinutes}
                    onChange={(event) => updateFilter("maxMetroWalkMinutes", event.target.value)}
                    placeholder="Метро: максимум минут пешком"
                    inputMode="decimal"
                  />
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input
                      type="checkbox"
                      checked={filters.requireMetroInPrimaryRadius}
                      onChange={(event) => updateFilter("requireMetroInPrimaryRadius", event.target.checked)}
                    />
                    Требовать метро в primary radius
                  </label>
                </div>

                <div style={listingsClientStyles.mapControls}>
                  <input
                    className="field"
                    value={filters.radiusMeters}
                    onChange={(event) => updateFilter("radiusMeters", event.target.value)}
                    placeholder="Радиус вокруг точки, м"
                    inputMode="numeric"
                    style={listingsClientStyles.radiusInput}
                  />
                  <button
                    className="button-secondary"
                    type="button"
                    onClick={() => setIsMapOverviewOpen(true)}
                  >
                    Выбрать точку на карте
                  </button>
                  {filters.centerLatitude != null && filters.centerLongitude != null ? (
                    <div style={listingsClientStyles.footerMeta}>
                      Точка: {filters.centerLatitude.toFixed(5)}, {filters.centerLongitude.toFixed(5)}
                    </div>
                  ) : (
                    <div style={listingsClientStyles.footerMeta}>
                      Точка не выбрана.
                    </div>
                  )}
                </div>

                <div style={listingsClientStyles.modalFooter}>
                  <div style={listingsClientStyles.footerMeta}>
                    Активных параметров: {activeFiltersCount}
                  </div>
                  <div style={listingsClientStyles.modalFooterActions}>
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
            </div>,
            document.body,
          )
        : null}
    </AppShell>
  );
}
