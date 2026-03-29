"use client";

import { searchListings } from "@diploma/api/search";
import { deleteListing } from "@diploma/api/listings";
import { useSearchListingsQuery } from "@diploma/features";
import type {
  CommerceLevel,
  InfrastructureLevel,
  LocationMode,
  SearchListingsRequest,
  SearchSortBy,
  TransportLevel,
} from "@diploma/contracts/search";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useDeferredValue, useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import { useSearchParams } from "next/navigation";
import { AppShell } from "../components/AppShell";
import { useAuth } from "../providers";
import { ListingGrid } from "./components/ListingGrid";
import { ListingsMapOverview } from "./components/ListingsMapOverview";
import { useFavoriteListings } from "./lib/listing-collections";
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
  locationMode: LocationMode | "";
  nearKindergarten: boolean;
  nearSchool: boolean;
  nearStore: boolean;
  nearPharmacy: boolean;
  nearStop: boolean;
  nearPark: boolean;
  nearClinic: boolean;
  nearParking: boolean;
  nearFitness: boolean;
  nearLeisure: boolean;
  nearMetro: boolean;
  familyFriendly: boolean;
  walkableArea: boolean;
  minInfrastructureScore: string;
  minEducationScore: string;
  minHealthcareScore: string;
  minTransportScore: string;
  minCommerceScore: string;
  minLeisureScore: string;
  minEducationCount: string;
  minTransportCount: string;
  minHealthcareCount: string;
  minCommerceCount: string;
  minLeisureCount: string;
  maxTransportNearestMeters: string;
  maxEducationNearestMeters: string;
  maxHealthcareNearestMeters: string;
  maxCommerceNearestMeters: string;
  maxLeisureNearestMeters: string;
  maxStoreWalkMinutes: string;
  maxPharmacyWalkMinutes: string;
  maxStopWalkMinutes: string;
  maxParkingWalkMinutes: string;
  maxSchoolWalkMinutes: string;
  maxKindergartenWalkMinutes: string;
  maxClinicDriveMinutes: string;
  maxHospitalDriveMinutes: string;
  maxParkWalkMinutes: string;
  maxFitnessWalkMinutes: string;
  maxMuseumDriveMinutes: string;
  maxTheaterDriveMinutes: string;
  maxCinemaDriveMinutes: string;
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
  locationMode: "",
  nearKindergarten: false,
  nearSchool: false,
  nearStore: false,
  nearPharmacy: false,
  nearStop: false,
  nearPark: false,
  nearClinic: false,
  nearParking: false,
  nearFitness: false,
  nearLeisure: false,
  nearMetro: false,
  familyFriendly: false,
  walkableArea: false,
  minInfrastructureScore: "",
  minEducationScore: "",
  minHealthcareScore: "",
  minTransportScore: "",
  minCommerceScore: "",
  minLeisureScore: "",
  minEducationCount: "",
  minTransportCount: "",
  minHealthcareCount: "",
  minCommerceCount: "",
  minLeisureCount: "",
  maxTransportNearestMeters: "",
  maxEducationNearestMeters: "",
  maxHealthcareNearestMeters: "",
  maxCommerceNearestMeters: "",
  maxLeisureNearestMeters: "",
  maxStoreWalkMinutes: "",
  maxPharmacyWalkMinutes: "",
  maxStopWalkMinutes: "",
  maxParkingWalkMinutes: "",
  maxSchoolWalkMinutes: "",
  maxKindergartenWalkMinutes: "",
  maxClinicDriveMinutes: "",
  maxHospitalDriveMinutes: "",
  maxParkWalkMinutes: "",
  maxFitnessWalkMinutes: "",
  maxMuseumDriveMinutes: "",
  maxTheaterDriveMinutes: "",
  maxCinemaDriveMinutes: "",
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

function parseOptionalWalkMinutesFromMeters(value: string) {
  const meters = parseOptionalNumber(value);
  if (meters == null) {
    return null;
  }

  return meters / 80;
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

function getInfrastructureLevelLabel(level: InfrastructureLevel | "") {
  switch (level) {
    case "poor":
      return "Базовый район";
    case "medium":
      return "Средний район";
    case "good":
      return "Хороший район";
    case "excellent":
      return "Отличный район";
    default:
      return "";
  }
}

function getTransportLevelLabel(level: TransportLevel) {
  switch (level) {
    case "medium":
      return "Транспорт: средний";
    case "good":
      return "Транспорт: хороший";
    case "excellent":
      return "Транспорт: отличный";
    default:
      return "";
  }
}

function getCommerceLevelLabel(level: CommerceLevel | "") {
  switch (level) {
    case "basic":
      return "Коммерция: базовая";
    case "good":
      return "Коммерция: хорошая";
    case "rich":
      return "Коммерция: насыщенная";
    default:
      return "";
  }
}

function getLocationModeLabel(mode: LocationMode | "") {
  switch (mode) {
    case "in_city":
      return "В городе";
    case "near_city":
      return "Рядом с городом";
    case "outside_city":
      return "За городом";
    default:
      return "";
  }
}

type ListingsClientProps = {
  title?: string;
  subtitle?: string;
  eyebrow?: string;
  forcedListingIds?: string[] | null;
  emptyTitle?: string;
  emptyDescription?: string;
};

export function ListingsClient({
  title = "Каталог и поиск объявлений",
  subtitle = "Основные параметры поиска и карта доступны в отдельной панели.",
  eyebrow = "Каталог",
  forcedListingIds = null,
  emptyTitle = "Ничего не найдено",
  emptyDescription = "Попробуйте изменить фильтры или сбросить ограничения, чтобы расширить выборку.",
}: ListingsClientProps = {}) {
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const { api, user } = useAuth();
  const favorites = useFavoriteListings();
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [isMapOverviewOpen, setIsMapOverviewOpen] = useState(false);
  const [isFiltersModalOpen, setIsFiltersModalOpen] = useState(false);
  const [filters, setFilters] = useState<SearchFormState>(initialState);
  const [appliedFilters, setAppliedFilters] = useState<SearchFormState>(initialState);
  useEffect(() => {
    const nextQuery = searchParams.get("q")?.trim() ?? "";
    setFilters((current) => (current.query === nextQuery ? current : {
      ...current,
      query: nextQuery,
      page: 0,
    }));
    setAppliedFilters((current) => (current.query === nextQuery ? current : {
      ...current,
      query: nextQuery,
      page: 0,
    }));
  }, [searchParams]);
  const isAdmin = user?.role?.toLowerCase() === "admin";
  const searchRequest = useMemo<SearchListingsRequest>(() => ({
    listingIds: forcedListingIds ?? null,
    query: appliedFilters.query.trim() || null,
    listingType: appliedFilters.listingType || null,
    minPrice: parseOptionalNumber(appliedFilters.minPrice),
    maxPrice: parseOptionalNumber(appliedFilters.maxPrice),
    minArea: parseOptionalNumber(appliedFilters.minArea),
    maxArea: parseOptionalNumber(appliedFilters.maxArea),
    infrastructureLevel: appliedFilters.infrastructureLevel || null,
    transportLevel: appliedFilters.transportLevel === "any" ? null : appliedFilters.transportLevel,
    commerceLevel: appliedFilters.commerceLevel || null,
    locationMode: appliedFilters.locationMode || null,
    nearKindergarten: appliedFilters.nearKindergarten ? true : null,
    nearSchool: appliedFilters.nearSchool ? true : null,
    nearStore: appliedFilters.nearStore ? true : null,
    nearPharmacy: appliedFilters.nearPharmacy ? true : null,
    nearStop: appliedFilters.nearStop ? true : null,
    nearPark: appliedFilters.nearPark ? true : null,
    nearClinic: appliedFilters.nearClinic ? true : null,
    nearParking: appliedFilters.nearParking ? true : null,
    nearFitness: appliedFilters.nearFitness ? true : null,
    nearLeisure: appliedFilters.nearLeisure ? true : null,
    nearMetro: appliedFilters.nearMetro ? true : null,
    familyFriendly: appliedFilters.familyFriendly ? true : null,
    walkableArea: appliedFilters.walkableArea ? true : null,
    minInfrastructureScore: parseOptionalNumber(appliedFilters.minInfrastructureScore),
    minEducationScore: parseOptionalNumber(appliedFilters.minEducationScore),
    minHealthcareScore: parseOptionalNumber(appliedFilters.minHealthcareScore),
    minTransportScore: parseOptionalNumber(appliedFilters.minTransportScore),
    minCommerceScore: parseOptionalNumber(appliedFilters.minCommerceScore),
    minLeisureScore: parseOptionalNumber(appliedFilters.minLeisureScore),
    minEducationCount: parseOptionalNumber(appliedFilters.minEducationCount),
    minTransportCount: parseOptionalNumber(appliedFilters.minTransportCount),
    minHealthcareCount: parseOptionalNumber(appliedFilters.minHealthcareCount),
    minCommerceCount: parseOptionalNumber(appliedFilters.minCommerceCount),
    minLeisureCount: parseOptionalNumber(appliedFilters.minLeisureCount),
    maxTransportNearestMeters: parseOptionalNumber(appliedFilters.maxTransportNearestMeters),
    maxEducationNearestMeters: parseOptionalNumber(appliedFilters.maxEducationNearestMeters),
    maxHealthcareNearestMeters: parseOptionalNumber(appliedFilters.maxHealthcareNearestMeters),
    maxCommerceNearestMeters: parseOptionalNumber(appliedFilters.maxCommerceNearestMeters),
    maxLeisureNearestMeters: parseOptionalNumber(appliedFilters.maxLeisureNearestMeters),
    maxStoreWalkMinutes: parseOptionalNumber(appliedFilters.maxStoreWalkMinutes),
    maxPharmacyWalkMinutes: parseOptionalNumber(appliedFilters.maxPharmacyWalkMinutes),
    maxStopWalkMinutes: parseOptionalNumber(appliedFilters.maxStopWalkMinutes),
    maxParkingWalkMinutes: parseOptionalNumber(appliedFilters.maxParkingWalkMinutes),
    maxSchoolWalkMinutes: parseOptionalNumber(appliedFilters.maxSchoolWalkMinutes),
    maxKindergartenWalkMinutes: parseOptionalNumber(appliedFilters.maxKindergartenWalkMinutes),
    maxClinicDriveMinutes: parseOptionalNumber(appliedFilters.maxClinicDriveMinutes),
    maxHospitalDriveMinutes: parseOptionalNumber(appliedFilters.maxHospitalDriveMinutes),
    maxParkWalkMinutes: parseOptionalNumber(appliedFilters.maxParkWalkMinutes),
    maxFitnessWalkMinutes: parseOptionalNumber(appliedFilters.maxFitnessWalkMinutes),
    maxMuseumDriveMinutes: parseOptionalNumber(appliedFilters.maxMuseumDriveMinutes),
    maxTheaterDriveMinutes: parseOptionalNumber(appliedFilters.maxTheaterDriveMinutes),
    maxCinemaDriveMinutes: parseOptionalNumber(appliedFilters.maxCinemaDriveMinutes),
    maxMetroWalkMinutes: parseOptionalWalkMinutesFromMeters(appliedFilters.maxMetroWalkMinutes),
    requireMetroInPrimaryRadius: appliedFilters.requireMetroInPrimaryRadius ? true : null,
    boundsMinLatitude: null,
    boundsMinLongitude: null,
    boundsMaxLatitude: null,
    boundsMaxLongitude: null,
    centerLatitude:
      appliedFilters.centerLatitude != null && parseOptionalNumber(appliedFilters.radiusMeters) != null
        ? appliedFilters.centerLatitude
        : null,
    centerLongitude:
      appliedFilters.centerLongitude != null && parseOptionalNumber(appliedFilters.radiusMeters) != null
        ? appliedFilters.centerLongitude
        : null,
    radiusMeters:
      appliedFilters.centerLatitude != null && appliedFilters.centerLongitude != null
        ? parseOptionalNumber(appliedFilters.radiusMeters)
        : null,
    includeHighlights: true,
    includeRelevanceBreakdown: false,
    sort: appliedFilters.sortBy,
    sortBy: appliedFilters.sortBy,
    page: appliedFilters.page,
    pageSize: LISTINGS_PAGE_SIZE,
  }), [appliedFilters, forcedListingIds]);
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
      isFavorite: favorites.isFavorite(listing.listingId),
      onToggleFavorite: favorites.toggleFavorite,
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
  const visiblePages = getVisiblePageNumbers(appliedFilters.page, totalPages);
  const activeFiltersCount = [
    appliedFilters.query.trim(),
    appliedFilters.listingType,
    appliedFilters.minPrice,
    appliedFilters.maxPrice,
    appliedFilters.minArea,
    appliedFilters.maxArea,
    appliedFilters.infrastructureLevel,
    appliedFilters.transportLevel !== "any" ? appliedFilters.transportLevel : "",
    appliedFilters.commerceLevel,
    appliedFilters.locationMode,
    appliedFilters.nearKindergarten ? "near-kindergarten" : "",
    appliedFilters.nearSchool ? "near-school" : "",
    appliedFilters.nearStore ? "near-store" : "",
    appliedFilters.nearPharmacy ? "near-pharmacy" : "",
    appliedFilters.nearStop ? "near-stop" : "",
    appliedFilters.nearPark ? "near-park" : "",
    appliedFilters.nearClinic ? "near-clinic" : "",
    appliedFilters.nearParking ? "near-parking" : "",
    appliedFilters.nearFitness ? "near-fitness" : "",
    appliedFilters.nearLeisure ? "near-leisure" : "",
    appliedFilters.nearMetro ? "near-metro" : "",
    appliedFilters.familyFriendly ? "family-friendly" : "",
    appliedFilters.walkableArea ? "walkable-area" : "",
    appliedFilters.minInfrastructureScore,
    appliedFilters.minEducationScore,
    appliedFilters.minHealthcareScore,
    appliedFilters.minTransportScore,
    appliedFilters.minCommerceScore,
    appliedFilters.minLeisureScore,
    appliedFilters.minEducationCount,
    appliedFilters.minTransportCount,
    appliedFilters.minHealthcareCount,
    appliedFilters.minCommerceCount,
    appliedFilters.minLeisureCount,
    appliedFilters.maxTransportNearestMeters,
    appliedFilters.maxEducationNearestMeters,
    appliedFilters.maxHealthcareNearestMeters,
    appliedFilters.maxCommerceNearestMeters,
    appliedFilters.maxLeisureNearestMeters,
    appliedFilters.maxStoreWalkMinutes,
    appliedFilters.maxPharmacyWalkMinutes,
    appliedFilters.maxStopWalkMinutes,
    appliedFilters.maxParkingWalkMinutes,
    appliedFilters.maxSchoolWalkMinutes,
    appliedFilters.maxKindergartenWalkMinutes,
    appliedFilters.maxClinicDriveMinutes,
    appliedFilters.maxHospitalDriveMinutes,
    appliedFilters.maxParkWalkMinutes,
    appliedFilters.maxFitnessWalkMinutes,
    appliedFilters.maxMuseumDriveMinutes,
    appliedFilters.maxTheaterDriveMinutes,
    appliedFilters.maxCinemaDriveMinutes,
    appliedFilters.maxMetroWalkMinutes,
    appliedFilters.requireMetroInPrimaryRadius ? "metro" : "",
    appliedFilters.radiusMeters,
    appliedFilters.centerLatitude != null && appliedFilters.centerLongitude != null ? "point" : "",
  ].filter(Boolean).length;

  function updateFilter<K extends keyof SearchFormState>(key: K, value: SearchFormState[K]) {
    setFilters((current) => ({
      ...current,
      [key]: value,
      page: 0,
    }));
  }

  function updateAppliedFilter<K extends keyof SearchFormState>(key: K, value: SearchFormState[K]) {
    setFilters((current) => ({
      ...current,
      [key]: value,
      page: key === "page" ? (value as SearchFormState["page"]) : 0,
    }));
    setAppliedFilters((current) => ({
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
          disabled={appliedFilters.page <= 0}
          onClick={() => updateAppliedFilter("page", Math.max(0, appliedFilters.page - 1))}
        >
          Назад
        </button>
        <div className="flex flex-wrap gap-2">
          {visiblePages.map((pageNumber) => {
            const isActive = pageNumber === appliedFilters.page;

            return (
              <button
                key={pageNumber}
                type="button"
                onClick={() => updateAppliedFilter("page", pageNumber)}
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
          disabled={appliedFilters.page + 1 >= totalPages}
          onClick={() => updateAppliedFilter("page", appliedFilters.page + 1)}
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
              {title}
              </h1>
            </div>

          <div style={listingsClientStyles.heroGrid}>
            <div style={listingsClientStyles.heroPanel}>
              <div style={listingsClientStyles.heroCopy}>
                <div style={listingsClientStyles.heroTitle}>
                  Подберите объявления по нужным параметрам
                </div>
                <p style={listingsClientStyles.mutedParagraph}>
                  {subtitle}
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
                  onClick={() => {
                    setFilters(initialState);
                    setAppliedFilters(initialState);
                  }}
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
                        {getInfrastructureLevelLabel(filters.infrastructureLevel)}
                      </span>
                    ) : null}
                    {filters.transportLevel !== "any" ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        {getTransportLevelLabel(filters.transportLevel)}
                      </span>
                    ) : null}
                    {filters.commerceLevel ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        {getCommerceLevelLabel(filters.commerceLevel)}
                      </span>
                    ) : null}
                    {filters.locationMode ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        {getLocationModeLabel(filters.locationMode)}
                      </span>
                    ) : null}
                    {filters.familyFriendly ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Для семьи
                      </span>
                    ) : null}
                    {filters.walkableArea ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Всё рядом пешком
                      </span>
                    ) : null}
                    {filters.nearMetro ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Рядом с метро
                      </span>
                    ) : null}
                    {filters.nearStore ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Рядом с магазинами
                      </span>
                    ) : null}
                    {filters.nearStop ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Рядом с остановками
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

          {!isLoading && items.length === 0 ? (
            <section className="surface grid gap-4 rounded-[28px] p-7">
              <div className="eyebrow">{eyebrow}</div>
              <h2 className="section-title" style={{ marginTop: 0 }}>
                {emptyTitle}
              </h2>
              <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 620 }}>
                {emptyDescription}
              </p>
            </section>
          ) : null}

          <div style={listingsClientStyles.footerRow}>
            <div style={listingsClientStyles.footerMeta}>
              Показано {(data?.items?.length ?? 0)} из {data?.totalCount ?? 0}
            </div>
            <div style={listingsClientStyles.footerActions}>
              <select
                className="field"
                value={filters.sortBy}
                onChange={(event) => updateAppliedFilter("sortBy", event.target.value as SearchSortBy)}
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

          {items.length > 0 || isLoading ? (
            <ListingGrid items={items} isLoading={isLoading && items.length === 0} />
          ) : null}

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
                    style={{ gridColumn: "1 / -1", width: "100%" }}
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
                    <option value="">Уровень инфраструктуры</option>
                    <option value="poor">Базовый</option>
                    <option value="medium">Средний</option>
                    <option value="good">Хороший</option>
                    <option value="excellent">Отличный</option>
                  </select>
                  <select
                    className="field"
                    value={filters.transportLevel}
                    onChange={(event) => updateFilter("transportLevel", event.target.value as TransportLevel)}
                  >
                    <option value="any">Транспорт: любой</option>
                    <option value="medium">Транспорт: средний</option>
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
                  <select
                    className="field"
                    value={filters.locationMode}
                    onChange={(event) => updateFilter("locationMode", event.target.value as LocationMode | "")}
                  >
                    <option value="">Локация: не важно</option>
                    <option value="in_city">В городе</option>
                    <option value="near_city">Рядом с городом</option>
                    <option value="outside_city">За городом</option>
                  </select>
                </div>

                <div className="grid gap-2">
                  <h3 className="section-title" style={{ fontSize: 18 }}>
                    Рядом с
                  </h3>
                </div>

                <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearKindergarten} onChange={(event) => updateFilter("nearKindergarten", event.target.checked)} />
                    Детским садом
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearSchool} onChange={(event) => updateFilter("nearSchool", event.target.checked)} />
                    Школой
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearStore} onChange={(event) => updateFilter("nearStore", event.target.checked)} />
                    Магазинами
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearPharmacy} onChange={(event) => updateFilter("nearPharmacy", event.target.checked)} />
                    Аптекой
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearStop} onChange={(event) => updateFilter("nearStop", event.target.checked)} />
                    Остановками
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearPark} onChange={(event) => updateFilter("nearPark", event.target.checked)} />
                    Парком
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearClinic} onChange={(event) => updateFilter("nearClinic", event.target.checked)} />
                    Поликлиникой
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearParking} onChange={(event) => updateFilter("nearParking", event.target.checked)} />
                    Парковкой
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearFitness} onChange={(event) => updateFilter("nearFitness", event.target.checked)} />
                    Фитнесом
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input type="checkbox" checked={filters.nearLeisure} onChange={(event) => updateFilter("nearLeisure", event.target.checked)} />
                    Досугом
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input
                      type="checkbox"
                      checked={filters.nearMetro}
                      onChange={(event) => updateFilter("nearMetro", event.target.checked)}
                    />
                    Метро
                  </label>
                </div>

                <div className="grid gap-2">
                  <h3 className="section-title" style={{ fontSize: 18 }}>
                    Формат района
                  </h3>
                </div>

                <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input
                      type="checkbox"
                      checked={filters.familyFriendly}
                      onChange={(event) => updateFilter("familyFriendly", event.target.checked)}
                    />
                    Для семьи
                  </label>
                  <label className="field" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <input
                      type="checkbox"
                      checked={filters.walkableArea}
                      onChange={(event) => updateFilter("walkableArea", event.target.checked)}
                    />
                    Всё рядом пешком
                  </label>
                </div>

                <details className="surface rounded-[24px] border border-[var(--border-color)] p-4">
                  <summary style={{ cursor: "pointer", fontWeight: 700, color: "var(--text-color)" }}>
                    Расширенные фильтры
                  </summary>
                  <p style={{ ...listingsClientStyles.mutedParagraph, marginTop: 12, marginBottom: 12 }}>
                    Здесь остаются точные ограничения по метрам, минутам и количеству.
                  </p>
                  <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                    <input
                      className="field"
                      value={filters.minInfrastructureScore}
                      onChange={(event) => updateFilter("minInfrastructureScore", event.target.value)}
                      placeholder="Инфраструктура от"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.minEducationScore}
                      onChange={(event) => updateFilter("minEducationScore", event.target.value)}
                      placeholder="Образование: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.minHealthcareScore}
                      onChange={(event) => updateFilter("minHealthcareScore", event.target.value)}
                      placeholder="Здоровье: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.minTransportScore}
                      onChange={(event) => updateFilter("minTransportScore", event.target.value)}
                      placeholder="Транспорт: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.minCommerceScore}
                      onChange={(event) => updateFilter("minCommerceScore", event.target.value)}
                      placeholder="Коммерция: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.minLeisureScore}
                      onChange={(event) => updateFilter("minLeisureScore", event.target.value)}
                      placeholder="Досуг: минимальный score"
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
                      value={filters.maxCommerceNearestMeters}
                      onChange={(event) => updateFilter("maxCommerceNearestMeters", event.target.value)}
                      placeholder="Коммерция не дальше, м"
                      inputMode="numeric"
                    />
                    <input
                      className="field"
                      value={filters.maxLeisureNearestMeters}
                      onChange={(event) => updateFilter("maxLeisureNearestMeters", event.target.value)}
                      placeholder="Досуг не дальше, м"
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
                      value={filters.maxParkWalkMinutes}
                      onChange={(event) => updateFilter("maxParkWalkMinutes", event.target.value)}
                      placeholder="Парк: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.maxFitnessWalkMinutes}
                      onChange={(event) => updateFilter("maxFitnessWalkMinutes", event.target.value)}
                      placeholder="Фитнес: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.maxMuseumDriveMinutes}
                      onChange={(event) => updateFilter("maxMuseumDriveMinutes", event.target.value)}
                      placeholder="Музей: максимум минут на машине"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.maxTheaterDriveMinutes}
                      onChange={(event) => updateFilter("maxTheaterDriveMinutes", event.target.value)}
                      placeholder="Театр: максимум минут на машине"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.maxCinemaDriveMinutes}
                      onChange={(event) => updateFilter("maxCinemaDriveMinutes", event.target.value)}
                      placeholder="Кино: максимум минут на машине"
                      inputMode="decimal"
                    />
                    <input
                      className="field"
                      value={filters.maxMetroWalkMinutes}
                      onChange={(event) => updateFilter("maxMetroWalkMinutes", event.target.value)}
                      placeholder="До метро: максимум метров пешком"
                      inputMode="numeric"
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
                </details>

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
                      onClick={() => {
                        setFilters(initialState);
                        setAppliedFilters(initialState);
                      }}
                    >
                      Сбросить
                    </button>
                    <button
                      className="button-primary"
                      type="button"
                      onClick={() => {
                        setAppliedFilters({
                          ...filters,
                          page: 0,
                        });
                        setIsFiltersModalOpen(false);
                      }}
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
