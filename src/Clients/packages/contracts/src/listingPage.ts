export type ListingPageMediaDto = {
  url: string;
};

export type ListingPagePriceHistoryDto = {
  changedAt: string;
  priceAmount: number | null;
  priceCurrency: string | null;
  priceAmountByn: number | null;
  priceAmountUsd: number | null;
};

export type ListingPageListingDto = {
  id: string;
  agentId: string;
  locationId: string | null;
  title: string;
  description: string | null;
  addressText: string | null;
  latitude: number | null;
  longitude: number | null;
  priceAmount: number | null;
  priceCurrency: string | null;
  priceAmountByn: number | null;
  priceAmountUsd: number | null;
  pricePeriod: string | null;
  areaTotal: number;
  buildYear: number | null;
  publishedDate: string | null;
  buildingType: string | null;
  listingType: string;
  mainMedia: string | null;
  media: ListingPageMediaDto[];
  listingDetailsType: string | null;
  listingDetails: Record<string, unknown> | null;
  priceHistory: ListingPagePriceHistoryDto[];
};

export type ListingPagePoiDto = {
  id: string;
  externalId: string | null;
  name: string;
  addressText: string | null;
  latitude: number;
  longitude: number;
  category: string;
  distanceMeters: number;
};

export type ListingPageGroupedPoisDto = {
  category: string;
  count: number;
  pois: ListingPagePoiDto[];
};

export type ListingPageInfrastructureDto = {
  score: number | null;
  totalScore: number | null;
  educationScore: number | null;
  healthcareScore: number | null;
  transportScore: number | null;
  commerceScore: number | null;
  leisureScore: number | null;
  poiCount: number;
  summary: string | null;
  groupedPois: ListingPageGroupedPoisDto[];
  nearbyPois: ListingPagePoiDto[];
  highlights: string[];
  calculatedAtUtc: string | null;
};

export type ListingPageSellerDto = {
  id: string;
  fullName: string | null;
  avatarUrl: string | null;
  phone: string | null;
  email: string | null;
  rating: number | null;
  reviewsCount: number | null;
  isVerified: boolean | null;
  agencyName: string | null;
  isFallback: boolean;
};

export type ListingQuickStatDto = {
  key: string;
  label: string;
  value: string;
  unit: string | null;
};

export type ListingFeatureTagDto = {
  key: string;
  label: string;
  tone: string | null;
};

export type ListingPageWarningDto = {
  code: string;
  message: string;
  severity: string;
  source: string | null;
};

export type ListingPageDerivedDto = {
  quickStats: ListingQuickStatDto[];
  featureTags: ListingFeatureTagDto[];
  warnings: string[];
  lastUpdatedUtc: string;
};

export type ListingPageAvailableBlocksDto = {
  listing: boolean;
  infrastructure: boolean;
  seller: boolean;
  priceHistory: boolean;
  map: boolean;
};

export type ListingPageDto = {
  listing: ListingPageListingDto;
  infrastructure: ListingPageInfrastructureDto | null;
  seller: ListingPageSellerDto;
  derived: ListingPageDerivedDto;
  availableBlocks: ListingPageAvailableBlocksDto;
  warnings: ListingPageWarningDto[];
};
