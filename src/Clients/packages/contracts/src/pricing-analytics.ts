export type PricingAnalyticsListingType = "sale" | "rent";
export type PricingAnalyticsGeography = "country" | "minsk" | "city";
export type PricingAnalyticsTrendDirection = "up" | "down" | "flat";
export type PricingAnalyticsConfidenceLevel = "low" | "medium" | "high";
export type ListingRelativePosition = "below_average" | "near_average" | "above_average" | "no_data";

export type PricingAnalyticsQuery = {
  listingType?: PricingAnalyticsListingType | null;
  geography?: PricingAnalyticsGeography | null;
  city?: string | null;
  buildingType?: string | null;
  listingDetailsType?: string | null;
  months?: number | null;
};

export type PricingAnalyticsSummary = {
  averagePricePerSqm: number | null;
  medianPricePerSqm: number | null;
  minPricePerSqm: number | null;
  maxPricePerSqm: number | null;
  marketRangeLowPricePerSqm: number | null;
  marketRangeHighPricePerSqm: number | null;
  listingsCount: number;
  periodStart: string | null;
  periodEnd: string | null;
  updatedAt: string;
  confidenceLevel: PricingAnalyticsConfidenceLevel;
  hasEnoughData: boolean;
  trendDirection: PricingAnalyticsTrendDirection;
};

export type PricingAnalyticsPoint = {
  period: string;
  averagePricePerSqm: number | null;
  medianPricePerSqm: number | null;
  listingsCount: number;
  hasEnoughData: boolean;
  confidenceLevel: PricingAnalyticsConfidenceLevel;
};

export type PricingAnalyticsCategory = {
  categoryKey: string;
  categoryLabel: string;
  averagePricePerSqm: number | null;
  medianPricePerSqm: number | null;
  minPricePerSqm: number | null;
  maxPricePerSqm: number | null;
  marketRangeLowPricePerSqm: number | null;
  marketRangeHighPricePerSqm: number | null;
  listingsCount: number;
  trendDirection: PricingAnalyticsTrendDirection;
  confidenceLevel: PricingAnalyticsConfidenceLevel;
  hasEnoughData: boolean;
};

export type ListingPriceComparison = {
  currentListingPricePerSqm: number | null;
  localRadiusAveragePricePerSqm: number | null;
  localRadiusMedianPricePerSqm: number | null;
  localRadiusListingsCount: number;
  cityAveragePricePerSqm: number | null;
  cityMedianPricePerSqm: number | null;
  cityListingsCount: number;
  countryAveragePricePerSqm: number | null;
  countryMedianPricePerSqm: number | null;
  countryListingsCount: number;
  marketRangeLowPricePerSqm: number | null;
  marketRangeHighPricePerSqm: number | null;
  relativePosition: ListingRelativePosition;
  message: string;
  confidenceLevel: PricingAnalyticsConfidenceLevel;
  hasEnoughData: boolean;
};

export type PricingEstimateRequest = {
  listingType?: string | null;
  buildingType?: string | null;
  listingDetailsType?: string | null;
  priceAmountByn: number;
  areaTotal: number;
  latitude?: number | null;
  longitude?: number | null;
  cityName?: string | null;
  locationId?: string | null;
};

export type PricingEstimateResponse = {
  enteredPricePerSqm: number | null;
  localRadiusAveragePricePerSqm: number | null;
  cityAveragePricePerSqm: number | null;
  countryAveragePricePerSqm: number | null;
  localRadiusListingsCount: number;
  cityListingsCount: number;
  countryListingsCount: number;
  relativePosition: ListingRelativePosition;
  message: string;
  confidenceLevel: PricingAnalyticsConfidenceLevel;
  hasEnoughData: boolean;
};

export type ListingPriceHistoryPoint = {
  id: string;
  effectiveDateUtc: string;
  priceAmountByn: number | null;
  pricePerSqmByn: number | null;
  comment: string | null;
};

export type ListingPriceHistory = {
  listingId: string;
  currentPriceAmountByn: number | null;
  firstRecordedPriceAmountByn: number | null;
  lastRecordedPriceAmountByn: number | null;
  firstRecordedAt: string | null;
  lastRecordedAt: string | null;
  percentChangeFromFirst: number | null;
  history: ListingPriceHistoryPoint[];
};

export type UpdateListingPriceRequest = {
  priceAmountByn: number;
  effectiveDateUtc?: string | null;
  comment?: string | null;
};

export type CreateListingPriceHistoryEntryRequest = {
  priceAmountByn: number;
  effectiveDateUtc: string;
  comment?: string | null;
};

export type UpdateListingPriceHistoryEntryRequest = {
  priceAmountByn?: number | null;
  effectiveDateUtc?: string | null;
  comment?: string | null;
};
