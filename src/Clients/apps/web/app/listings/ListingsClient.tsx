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
import { useDeferredValue, useEffect, useMemo, useRef, useState } from "react";
import type { CSSProperties } from "react";
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

const LISTINGS_PAGE_SIZE = 12;
const PAGINATION_WINDOW = 9;
const LISTINGS_FILTERS_STORAGE_KEY = "domio:listings:filters";
const SORT_OPTIONS: Array<{ value: SearchSortBy; label: string }> = [
  { value: "relevance", label: "По релевантности" },
  { value: "published_desc", label: "Сначала новые" },
  { value: "area_desc", label: "Площадь больше" },
  { value: "price_asc", label: "Цена по возрастанию" },
  { value: "price_desc", label: "Цена по убыванию" },
  { value: "infrastructure_desc", label: "Лучшая инфраструктура" },
];

const nearbySectionStyle: CSSProperties = {
  display: "grid",
  gap: 4,
  margin: 0,
  padding: 0,
};

const nearbySectionLabelStyle: CSSProperties = {
  display: "block",
  margin: 0,
  fontSize: 24,
  fontWeight: 700,
  lineHeight: 1.1,
  color: "var(--foreground-muted)",
};

const nearbyGridStyle: CSSProperties = {
  display: "flex",
  flexWrap: "wrap",
  gap: 12,
  margin: 0,
  paddingTop: 6,
  alignItems: "flex-start",
};

const nearbyCheckStyle: CSSProperties = {
  display: "flex",
  alignItems: "center",
  gap: 8,
  margin: 0,
  minHeight: 0,
  width: "auto",
  flex: "0 0 auto",
  padding: "14px 20px",
  border: "1px solid color-mix(in srgb, var(--border-color) 88%, transparent)",
  background: "color-mix(in srgb, var(--background-elevated) 96%, var(--background-muted))",
  borderRadius: 18,
  lineHeight: 1.1,
  boxShadow: "inset 0 0 0 1px color-mix(in srgb, white 12%, transparent)",
  fontSize: 20,
  whiteSpace: "nowrap",
};

type SearchFormState = {
  query: string;
  listingType: string;
  pricePeriod: string;
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
  pricePeriod: "",
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
  subtitle = "",
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
  const [isSortMenuOpen, setIsSortMenuOpen] = useState(false);
  const [isMobileSortMenu, setIsMobileSortMenu] = useState(false);
  const [filters, setFilters] = useState<SearchFormState>(initialState);
  const [appliedFilters, setAppliedFilters] = useState<SearchFormState>(initialState);
  const [filtersHydrated, setFiltersHydrated] = useState(false);
  const sortMenuRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (typeof window === "undefined") {
      return;
    }

    try {
      const raw = window.sessionStorage.getItem(LISTINGS_FILTERS_STORAGE_KEY);
      if (!raw) {
        setFiltersHydrated(true);
        return;
      }

      const parsed = JSON.parse(raw) as Partial<SearchFormState>;
      const restored = { ...initialState, ...parsed };
      setFilters(restored);
      setAppliedFilters(restored);
    } catch {
      // Ignore broken session state and keep defaults.
    } finally {
      setFiltersHydrated(true);
    }
  }, []);

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

  useEffect(() => {
    if (!filtersHydrated || typeof window === "undefined") {
      return;
    }

    window.sessionStorage.setItem(LISTINGS_FILTERS_STORAGE_KEY, JSON.stringify(appliedFilters));
  }, [appliedFilters, filtersHydrated]);

  useEffect(() => {
    function syncMobileState() {
      setIsMobileSortMenu(window.innerWidth <= 1024);
    }

    syncMobileState();
    window.addEventListener("resize", syncMobileState);

    return () => {
      window.removeEventListener("resize", syncMobileState);
    };
  }, []);

  useEffect(() => {
    if (!isSortMenuOpen || !isMobileSortMenu) {
      return;
    }

    function handlePointerDown(event: MouseEvent) {
      if (!sortMenuRef.current?.contains(event.target as Node)) {
        setIsSortMenuOpen(false);
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setIsSortMenuOpen(false);
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("keydown", handleEscape);
    };
  }, [isMobileSortMenu, isSortMenuOpen]);
  const isAdmin = user?.role?.toLowerCase() === "admin";
  const searchRequest = useMemo<SearchListingsRequest>(() => ({
    listingIds: forcedListingIds ?? null,
    query: appliedFilters.query.trim() || null,
    listingType: appliedFilters.listingType || null,
    pricePeriod: appliedFilters.pricePeriod || null,
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
    walkableArea: null,
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
      isFavorite: user ? favorites.isFavorite(listing.listingId) : false,
      onToggleFavorite: user ? favorites.toggleFavorite : undefined,
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
      priceText: `${new Intl.NumberFormat("ru-RU", {
        minimumFractionDigits: 0,
        maximumFractionDigits: listing.priceAmount != null && Number.isInteger(listing.priceAmount) ? 0 : 2,
      }).format(listing.priceAmount ?? 0)} ${listing.priceCurrency ?? "BYN"}`,
      publishedDate: listing.publishedDate,
    }));
  const listingsWithoutCoordinates = (data?.items ?? []).filter(
    (listing) => listing.latitude == null || listing.longitude == null,
  ).length;
  const totalPages = Math.max(1, Math.ceil((data?.totalCount ?? 0) / LISTINGS_PAGE_SIZE));
  const visiblePages = getVisiblePageNumbers(appliedFilters.page, totalPages);
  const activeSortLabel = SORT_OPTIONS.find((option) => option.value === filters.sortBy)?.label ?? "Сортировка";
  const activeFiltersCount = [
    appliedFilters.query.trim(),
    appliedFilters.listingType,
    appliedFilters.pricePeriod,
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
    <div className="listings-pagination" style={listingsClientStyles.footerRow}>
      <div style={listingsClientStyles.footerMeta}>
        Страница {Math.min((data?.page ?? 0) + 1, totalPages)} из {totalPages}
      </div>

      <div className="listings-pagination-actions" style={listingsClientStyles.footerActions}>
        <button
          className="button-secondary listings-pagination-nav"
          type="button"
          disabled={appliedFilters.page <= 0}
          onClick={() => updateAppliedFilter("page", Math.max(0, appliedFilters.page - 1))}
        >
          Назад
        </button>
        <div className="listings-pagination-pages flex flex-wrap gap-2">
          {visiblePages.map((pageNumber) => {
            const isActive = pageNumber === appliedFilters.page;

            return (
              <button
                key={pageNumber}
                type="button"
                onClick={() => updateAppliedFilter("page", pageNumber)}
                className={isActive ? "button-primary listings-pagination-page" : "button-secondary listings-pagination-page"}
                aria-current={isActive ? "page" : undefined}
              >
                {pageNumber + 1}
              </button>
            );
          })}
        </div>
        <button
          className="button-secondary listings-pagination-nav"
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

          <div className="listings-hero-grid" style={listingsClientStyles.heroGrid}>
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
                  className="button-primary listings-filter-button listings-filter-button-primary"
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
                  className="button-secondary listings-filter-button"
                  type="button"
                  onClick={() => {
                    setFilters(initialState);
                    setAppliedFilters(initialState);
                  }}
                >
                  Сбросить
                </button>
                <button
                  className="button-secondary listings-filter-button"
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
                    {filters.pricePeriod ? (
                      <span className="surface" style={listingsClientStyles.filterTag}>
                        Оплата: {filters.pricePeriod === "Daily" ? "за сутки" : filters.pricePeriod === "Monthly" ? "за месяц" : filters.pricePeriod}
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

          <div className="listings-footer-row" style={listingsClientStyles.footerRow}>
            {isMobileSortMenu ? (
              <div
                ref={sortMenuRef}
                className="listings-sort-wrap"
                style={{
                  ...listingsClientStyles.footerActions,
                  position: "relative",
                  minWidth: 220,
                }}
              >
                <button
                  type="button"
                  className="field listings-sort-select"
                  aria-haspopup="listbox"
                  aria-expanded={isSortMenuOpen}
                  onClick={() => setIsSortMenuOpen((current) => !current)}
                  style={{
                    width: "100%",
                    minWidth: 220,
                    textAlign: "left",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                    gap: 12,
                    cursor: "pointer",
                  }}
                >
                  <span>{activeSortLabel}</span>
                  <span aria-hidden="true" style={{ fontSize: 12, opacity: 0.72 }}>
                    {isSortMenuOpen ? "▲" : "▼"}
                  </span>
                </button>

                {isSortMenuOpen ? (
                  <div
                    role="listbox"
                    aria-label="Сортировка объявлений"
                    className="surface listings-sort-menu"
                    style={{
                      position: "absolute",
                      top: "calc(100% + 8px)",
                      left: 0,
                      right: 0,
                      zIndex: 40,
                      display: "grid",
                      gap: 6,
                      padding: 8,
                      borderRadius: 18,
                      background: "color-mix(in srgb, var(--background-elevated) 96%, transparent)",
                      backdropFilter: "blur(18px)",
                      boxShadow: "0 18px 42px color-mix(in srgb, var(--shadow-color) 78%, transparent)",
                    }}
                  >
                    {SORT_OPTIONS.map((option) => {
                      const isActive = option.value === filters.sortBy;

                      return (
                        <button
                          key={option.value}
                          type="button"
                          role="option"
                          aria-selected={isActive}
                          className={isActive ? "button-primary" : "button-secondary"}
                          onClick={() => {
                            updateAppliedFilter("sortBy", option.value);
                            setIsSortMenuOpen(false);
                          }}
                          style={{
                            width: "100%",
                            justifyContent: "flex-start",
                            textAlign: "left",
                          }}
                        >
                          {option.label}
                        </button>
                      );
                    })}
                  </div>
                ) : null}
              </div>
            ) : (
              <div style={listingsClientStyles.footerActions}>
                <select
                  className="field listings-sort-select"
                  value={filters.sortBy}
                  onChange={(event) => updateAppliedFilter("sortBy", event.target.value as SearchSortBy)}
                  style={{ minWidth: 220 }}
                >
                  {SORT_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>

          {paginationBlock}

          {items.length > 0 || isLoading ? (
            <ListingGrid items={items} isLoading={isLoading && items.length === 0} />
          ) : null}

          <div style={listingsClientStyles.footerRow}>
          </div>

          {paginationBlock}
        </section>
      </main>

      <button
        type="button"
        onClick={() => setIsMapOverviewOpen(true)}
        className="surface listings-map-button"
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
        onSelectCenter={(point) => {
          setFilters((current) => ({
            ...current,
            centerLatitude: point.latitude,
            centerLongitude: point.longitude,
            radiusMeters: current.radiusMeters.trim() ? current.radiusMeters : "1000",
          }));
        }}
        onClearCenter={() => {
          setFilters((current) => ({
            ...current,
            centerLatitude: null,
            centerLongitude: null,
          }));
        }}
      />

      {isFiltersModalOpen && typeof document !== "undefined"
        ? createPortal(
            <div
              className="listing-create-modal-backdrop"
              onClick={() => setIsFiltersModalOpen(false)}
            >
              <div
                className="listing-create-modal surface listings-modal-panel"
                onClick={(event) => event.stopPropagation()}
                style={listingsClientStyles.modalPanel}
              >
                <div className="listings-modal-header" style={listingsClientStyles.modalHeader}>
                  <h2
                    style={{
                      ...listingsClientStyles.modalHeading,
                      margin: 0,
                      fontSize: "1.05rem",
                      fontWeight: 600,
                      letterSpacing: "-0.02em",
                    }}
                  >
                    Фильтры
                  </h2>
                  <button
                    className="listings-filter-close"
                    type="button"
                    aria-label="Закрыть фильтры"
                    onClick={() => setIsFiltersModalOpen(false)}
                    style={{
                      width: 34,
                      height: 34,
                      minWidth: 34,
                      minHeight: 34,
                      borderRadius: 10,
                      fontSize: 21,
                      lineHeight: 1,
                      padding: 0,
                    }}
                  >
                    ×
                  </button>
                </div>

                <div className="listings-filters-grid grid gap-3 md:grid-cols-2 xl:grid-cols-5">
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
                  <select
                    className="field"
                    value={filters.pricePeriod}
                    onChange={(event) => updateFilter("pricePeriod", event.target.value)}
                  >
                    <option value="">Оплата: любая</option>
                    <option value="Monthly">За месяц</option>
                    <option value="Daily">За сутки</option>
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
                <div className="listings-filter-section listings-filter-section-before-nearby grid gap-3" style={{ marginTop: 14, marginBottom: 0, paddingTop: 0, paddingBottom: 0 }}>
                  <div className="listings-quick-filter-row">
                    <button
                      type="button"
                      className={filters.familyFriendly ? "button-primary listings-filter-button listings-quick-filter-button" : "button-secondary listings-filter-button listings-quick-filter-button"}
                      onClick={() => updateFilter("familyFriendly", !filters.familyFriendly)}
                    >
                      Для семьи
                    </button>
                    <button
                      type="button"
                      className={filters.nearMetro ? "button-primary listings-filter-button listings-quick-filter-button" : "button-secondary listings-filter-button listings-quick-filter-button"}
                      onClick={() => updateFilter("nearMetro", !filters.nearMetro)}
                    >
                      Рядом с метро
                    </button>
                    <button
                      type="button"
                      className={filters.nearStore ? "button-primary listings-filter-button listings-quick-filter-button" : "button-secondary listings-filter-button listings-quick-filter-button"}
                      onClick={() => updateFilter("nearStore", !filters.nearStore)}
                    >
                      Магазины рядом
                    </button>
                    <button
                      type="button"
                      className={filters.nearStop ? "button-primary listings-filter-button listings-quick-filter-button" : "button-secondary listings-filter-button listings-quick-filter-button"}
                      onClick={() => updateFilter("nearStop", !filters.nearStop)}
                    >
                      Остановки рядом
                    </button>
                  </div>
                </div>

                <div style={{ ...nearbySectionStyle, marginTop: 0, marginBottom: 0, paddingTop: 0, paddingBottom: 0 }}>
                  <span style={nearbySectionLabelStyle}>Рядом с</span>
                  <div className="listings-filters-check-grid grid gap-3 md:grid-cols-2 xl:grid-cols-4" style={nearbyGridStyle}>
                  <label className="listings-filter-check" style={nearbyCheckStyle}>
                    <input type="checkbox" checked={filters.nearKindergarten} onChange={(event) => updateFilter("nearKindergarten", event.target.checked)} style={{ width: 13, height: 13, margin: 0 }} />
                    Детским садом
                  </label>
                  <label className="listings-filter-check" style={nearbyCheckStyle}>
                    <input type="checkbox" checked={filters.nearSchool} onChange={(event) => updateFilter("nearSchool", event.target.checked)} style={{ width: 13, height: 13, margin: 0 }} />
                    Школой
                  </label>
                  <label className="listings-filter-check" style={nearbyCheckStyle}>
                    <input type="checkbox" checked={filters.nearPharmacy} onChange={(event) => updateFilter("nearPharmacy", event.target.checked)} style={{ width: 13, height: 13, margin: 0 }} />
                    Аптекой
                  </label>
                  <label className="listings-filter-check" style={nearbyCheckStyle}>
                    <input type="checkbox" checked={filters.nearPark} onChange={(event) => updateFilter("nearPark", event.target.checked)} style={{ width: 13, height: 13, margin: 0 }} />
                    Парком
                  </label>
                  <label className="listings-filter-check" style={nearbyCheckStyle}>
                    <input type="checkbox" checked={filters.nearClinic} onChange={(event) => updateFilter("nearClinic", event.target.checked)} style={{ width: 13, height: 13, margin: 0 }} />
                    Поликлиникой
                  </label>
                  <label className="listings-filter-check" style={nearbyCheckStyle}>
                    <input type="checkbox" checked={filters.nearParking} onChange={(event) => updateFilter("nearParking", event.target.checked)} style={{ width: 13, height: 13, margin: 0 }} />
                    Парковкой
                  </label>
                  <label className="listings-filter-check" style={nearbyCheckStyle}>
                    <input type="checkbox" checked={filters.nearFitness} onChange={(event) => updateFilter("nearFitness", event.target.checked)} style={{ width: 13, height: 13, margin: 0 }} />
                    Фитнесом
                  </label>
                  <label className="listings-filter-check" style={nearbyCheckStyle}>
                    <input type="checkbox" checked={filters.nearLeisure} onChange={(event) => updateFilter("nearLeisure", event.target.checked)} style={{ width: 13, height: 13, margin: 0 }} />
                    Досугом
                  </label>
                </div>
                </div>

                <details className="surface listings-advanced-details rounded-[24px] border border-[var(--border-color)] p-4" style={{ marginTop: 18, paddingTop: 12 }}>
                  <summary
                    style={{
                      cursor: "pointer",
                      fontWeight: 700,
                      color: "var(--text-color)",
                      fontSize: 20,
                      lineHeight: 1.2,
                      paddingTop: 8,
                      paddingBottom: 8,
                    }}
                  >
                    Дополнительно
                  </summary>
                  <p style={{ ...listingsClientStyles.mutedParagraph, marginTop: 12, marginBottom: 12 }}>
                    Здесь остаются точные ограничения по метрам, минутам и количеству.
                  </p>
                  <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4 2xl:grid-cols-5">
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minInfrastructureScore}
                      onChange={(event) => updateFilter("minInfrastructureScore", event.target.value)}
                      placeholder="Инфраструктура от"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minEducationScore}
                      onChange={(event) => updateFilter("minEducationScore", event.target.value)}
                      placeholder="Образование: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minHealthcareScore}
                      onChange={(event) => updateFilter("minHealthcareScore", event.target.value)}
                      placeholder="Здоровье: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minTransportScore}
                      onChange={(event) => updateFilter("minTransportScore", event.target.value)}
                      placeholder="Транспорт: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minCommerceScore}
                      onChange={(event) => updateFilter("minCommerceScore", event.target.value)}
                      placeholder="Коммерция: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minLeisureScore}
                      onChange={(event) => updateFilter("minLeisureScore", event.target.value)}
                      placeholder="Досуг: минимальный score"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minEducationCount}
                      onChange={(event) => updateFilter("minEducationCount", event.target.value)}
                      placeholder="Объекты образования, от"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minTransportCount}
                      onChange={(event) => updateFilter("minTransportCount", event.target.value)}
                      placeholder="Транспорт рядом, от"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minHealthcareCount}
                      onChange={(event) => updateFilter("minHealthcareCount", event.target.value)}
                      placeholder="Медицина рядом, от"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minCommerceCount}
                      onChange={(event) => updateFilter("minCommerceCount", event.target.value)}
                      placeholder="Коммерция рядом, от"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.minLeisureCount}
                      onChange={(event) => updateFilter("minLeisureCount", event.target.value)}
                      placeholder="Досуг рядом, от"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxTransportNearestMeters}
                      onChange={(event) => updateFilter("maxTransportNearestMeters", event.target.value)}
                      placeholder="Транспорт не дальше, м"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxEducationNearestMeters}
                      onChange={(event) => updateFilter("maxEducationNearestMeters", event.target.value)}
                      placeholder="Образование не дальше, м"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxHealthcareNearestMeters}
                      onChange={(event) => updateFilter("maxHealthcareNearestMeters", event.target.value)}
                      placeholder="Медицина не дальше, м"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxCommerceNearestMeters}
                      onChange={(event) => updateFilter("maxCommerceNearestMeters", event.target.value)}
                      placeholder="Коммерция не дальше, м"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxLeisureNearestMeters}
                      onChange={(event) => updateFilter("maxLeisureNearestMeters", event.target.value)}
                      placeholder="Досуг не дальше, м"
                      inputMode="numeric"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxStoreWalkMinutes}
                      onChange={(event) => updateFilter("maxStoreWalkMinutes", event.target.value)}
                      placeholder="Магазин: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxPharmacyWalkMinutes}
                      onChange={(event) => updateFilter("maxPharmacyWalkMinutes", event.target.value)}
                      placeholder="Аптека: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxStopWalkMinutes}
                      onChange={(event) => updateFilter("maxStopWalkMinutes", event.target.value)}
                      placeholder="Остановка: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxParkingWalkMinutes}
                      onChange={(event) => updateFilter("maxParkingWalkMinutes", event.target.value)}
                      placeholder="Парковка: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxSchoolWalkMinutes}
                      onChange={(event) => updateFilter("maxSchoolWalkMinutes", event.target.value)}
                      placeholder="Школа: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxKindergartenWalkMinutes}
                      onChange={(event) => updateFilter("maxKindergartenWalkMinutes", event.target.value)}
                      placeholder="Детский сад: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxClinicDriveMinutes}
                      onChange={(event) => updateFilter("maxClinicDriveMinutes", event.target.value)}
                      placeholder="Поликлиника: максимум минут на машине"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxHospitalDriveMinutes}
                      onChange={(event) => updateFilter("maxHospitalDriveMinutes", event.target.value)}
                      placeholder="Больница: максимум минут на машине"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxParkWalkMinutes}
                      onChange={(event) => updateFilter("maxParkWalkMinutes", event.target.value)}
                      placeholder="Парк: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxFitnessWalkMinutes}
                      onChange={(event) => updateFilter("maxFitnessWalkMinutes", event.target.value)}
                      placeholder="Фитнес: максимум минут пешком"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxMuseumDriveMinutes}
                      onChange={(event) => updateFilter("maxMuseumDriveMinutes", event.target.value)}
                      placeholder="Музей: максимум минут на машине"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxTheaterDriveMinutes}
                      onChange={(event) => updateFilter("maxTheaterDriveMinutes", event.target.value)}
                      placeholder="Театр: максимум минут на машине"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxCinemaDriveMinutes}
                      onChange={(event) => updateFilter("maxCinemaDriveMinutes", event.target.value)}
                      placeholder="Кино: максимум минут на машине"
                      inputMode="decimal"
                    />
                    <input
                      className="field listings-filter-input-compact"
                      value={filters.maxMetroWalkMinutes}
                      onChange={(event) => updateFilter("maxMetroWalkMinutes", event.target.value)}
                      placeholder="До метро: максимум метров пешком"
                      inputMode="numeric"
                    />
                    <label className="listings-filter-check" style={{ display: "flex", alignItems: "center", gap: 12 }}>
                      <input
                        type="checkbox"
                        checked={filters.requireMetroInPrimaryRadius}
                        onChange={(event) => updateFilter("requireMetroInPrimaryRadius", event.target.checked)}
                      />
                      Требовать метро в основном радиусе
                    </label>
                  </div>
                </details>

                <div className="listings-filter-map-controls" style={listingsClientStyles.mapControls}>
                  <input
                    className="field"
                    value={filters.radiusMeters}
                    onChange={(event) => updateFilter("radiusMeters", event.target.value)}
                    placeholder="Радиус вокруг точки, м"
                    inputMode="numeric"
                    style={listingsClientStyles.radiusInput}
                  />
                  <button
                    className="button-secondary listings-filter-button"
                    type="button"
                    onClick={() => setIsMapOverviewOpen(true)}
                  >
                    Выбрать точку на карте
                  </button>
                  {filters.centerLatitude != null && filters.centerLongitude != null ? (
                    <div className="listings-filter-map-status" style={listingsClientStyles.footerMeta}>
                      Точка: {filters.centerLatitude.toFixed(5)}, {filters.centerLongitude.toFixed(5)}
                    </div>
                  ) : (
                    <div className="listings-filter-map-status" style={listingsClientStyles.footerMeta}>
                      Точка не выбрана.
                    </div>
                  )}
                </div>

                <div className="listings-filter-footer" style={listingsClientStyles.modalFooter}>
                  <div style={listingsClientStyles.footerMeta}>
                    Активных параметров: {activeFiltersCount}
                  </div>
                  <div className="listings-filter-footer-actions" style={listingsClientStyles.modalFooterActions}>
                    <button
                      className="button-secondary listings-filter-button"
                      type="button"
                      onClick={() => {
                        setFilters(initialState);
                        setAppliedFilters(initialState);
                      }}
                    >
                      Сбросить
                    </button>
                    <button
                      className="button-primary listings-filter-button listings-filter-button-primary"
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
