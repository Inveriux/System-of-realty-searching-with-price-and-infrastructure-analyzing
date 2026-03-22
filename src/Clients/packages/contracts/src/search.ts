export type SearchSortBy =
  | "price_asc"
  | "price_desc"
  | "published_desc"
  | "infrastructure_desc";

export type SearchListingsRequest = {
  query?: string | null;
  locationId?: string | null;
  listingType?: string | null;
  buildingType?: string | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  minArea?: number | null;
  maxArea?: number | null;
  minInfrastructureScore?: number | null;
  minEducationScore?: number | null;
  minHealthcareScore?: number | null;
  minTransportScore?: number | null;
  minCommerceScore?: number | null;
  minLeisureScore?: number | null;
  minEducationCount?: number | null;
  minHealthcareCount?: number | null;
  minTransportCount?: number | null;
  minCommerceCount?: number | null;
  minLeisureCount?: number | null;
  maxEducationNearestMeters?: number | null;
  maxHealthcareNearestMeters?: number | null;
  maxTransportNearestMeters?: number | null;
  maxCommerceNearestMeters?: number | null;
  maxLeisureNearestMeters?: number | null;
  centerLatitude?: number | null;
  centerLongitude?: number | null;
  radiusMeters?: number | null;
  boundsMinLatitude?: number | null;
  boundsMinLongitude?: number | null;
  boundsMaxLatitude?: number | null;
  boundsMaxLongitude?: number | null;
  sortBy?: SearchSortBy | null;
  page: number;
  pageSize: number;
};

export type SearchListingDto = {
  listingId: string;
  agentId: string;
  title: string;
  description: string | null;
  addressText: string | null;
  locationId: string | null;
  latitude: number | null;
  longitude: number | null;
  listingType: string | null;
  buildingType: string | null;
  listingDetailsType: string | null;
  priceAmount: number | null;
  priceAmountByn: number | null;
  priceAmountUsd: number | null;
  priceCurrency: string | null;
  areaTotal: number;
  mainMediaUrl: string | null;
  publishedDate: string | null;
  infrastructureScore: number | null;
  educationScore: number | null;
  healthcareScore: number | null;
  transportScore: number | null;
  commerceScore: number | null;
  leisureScore: number | null;
  educationCount: number | null;
  healthcareCount: number | null;
  transportCount: number | null;
  commerceCount: number | null;
  leisureCount: number | null;
  educationNearestMeters: number | null;
  healthcareNearestMeters: number | null;
  transportNearestMeters: number | null;
  commerceNearestMeters: number | null;
  leisureNearestMeters: number | null;
  searchFeatures: Record<string, number>;
};

export type SearchListingsResponse = {
  items: SearchListingDto[];
  page: number;
  pageSize: number;
  totalCount: number;
};
