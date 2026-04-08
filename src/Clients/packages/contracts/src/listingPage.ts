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
  subcategoryCode: string | null;
  distanceMeters: number;
};

export type ListingPageGroupedPoisDto = {
  category: string;
  count: number;
  pois: ListingPagePoiDto[];
};

export type ListingPageInfrastructureNearbyCategoryDto = {
  categoryCode: string;
  subcategoryCode: string;
  countInPrimaryRadius: number;
  candidateCount: number;
  reachableWithin10MinCount: number;
  reachableWithin15MinCount: number;
  nearestDistanceM: number | null;
  routeDistanceMeters: number | null;
  travelDurationSeconds: number | null;
  walkingMinutes: number | null;
  nearestWalkMinutes: number | null;
  nearestDriveMinutes: number | null;
  nearestDriveMinutesEstimated: boolean;
  nearestAnyDistanceM: number | null;
  nearestAnyWalkMinutes: number | null;
  nearestAnyDriveMinutes: number | null;
  nearestAnyDriveMinutesEstimated: boolean;
  usedRouting: boolean;
  fallbackMode: string;
  foundInRadiusM: number | null;
  foundInPrimaryRadius: boolean;
};

export type ListingPageInfrastructureTransportDetailsDto = {
  nearestStopDistanceM: number | null;
  nearestStopWalkMinutes: number | null;
  stopsCountPrimary: number;
  stopGroupsCountPrimary: number;
  stopPlatformsCountPrimary: number;
  nearestStopGroupDistanceM: number | null;
  nearestStopGroupWalkMinutes: number | null;
  nearestStopGroupDriveMinutes: number | null;
  nearestStopGroupDriveMinutesEstimated: boolean;
  nearestStopGroupName: string | null;
  nearestStopGroupPlatformCount: number | null;
  nearestStopGroupAnyDistanceM: number | null;
  nearestStopGroupAnyWalkMinutes: number | null;
  nearestStopGroupAnyDriveMinutes: number | null;
  nearestStopGroupAnyDriveMinutesEstimated: boolean;
  nearestStopGroupFoundRadiusM: number | null;
  stopGroups: ListingPageInfrastructureTransportStopGroupDto[];
  nearestMetroDistanceM: number | null;
  nearestMetroWalkMinutes: number | null;
  metroFoundInPrimaryRadius: boolean;
  nearestParkingDistanceM: number | null;
  nearestParkingWalkMinutes: number | null;
  nearestParkingDriveMinutes: number | null;
  nearestParkingDriveMinutesEstimated: boolean;
  parkingCountPrimary: number;
};

export type ListingPageInfrastructureTransportStopGroupDto = {
  groupId: string;
  normalizedName: string;
  displayName: string;
  centerLatitude: number;
  centerLongitude: number;
  maxDistanceWithinGroup: number;
  pointCount: number;
  platformCount: number;
  stopTypes: string[];
  sourcePoiIds: string[];
  nearestDistanceMeters: number;
  routeDistanceMeters: number | null;
  walkMinutes: number | null;
  usedRouting: boolean;
  fallbackMode: string;
};

export type ListingPageGroupedTransportSummaryDto = {
  nearestStopGroupName: string | null;
  nearestStopGroupDistanceM: number | null;
  nearestStopGroupWalkMinutes: number | null;
  nearestStopGroupDriveMinutes: number | null;
  nearestStopGroupDriveMinutesEstimated: boolean;
  nearestStopGroupAnyDistanceM: number | null;
  nearestStopGroupAnyWalkMinutes: number | null;
  nearestStopGroupAnyDriveMinutes: number | null;
  nearestStopGroupAnyDriveMinutesEstimated: boolean;
  stopGroupFoundInPrimaryRadius: boolean;
  nearestStopGroupFoundRadiusM: number | null;
  stopGroupsCountPrimary: number;
  stopPlatformsCountPrimary: number;
  nearestStopGroupPlatformCount: number | null;
  nearestMetroDistanceM: number | null;
  nearestMetroWalkMinutes: number | null;
  metroFoundInPrimaryRadius: boolean;
  nearestParkingDistanceM: number | null;
  nearestParkingWalkMinutes: number | null;
  nearestParkingDriveMinutes: number | null;
  nearestParkingDriveMinutesEstimated: boolean;
  nearestParkingAnyDistanceM: number | null;
  nearestParkingAnyWalkMinutes: number | null;
  nearestParkingAnyDriveMinutes: number | null;
  nearestParkingAnyDriveMinutesEstimated: boolean;
  parkingFoundInPrimaryRadius: boolean;
  nearestParkingFoundRadiusM: number | null;
};

export type ListingPageCompactInfrastructureItemDto = {
  key: string;
  label: string;
  iconSrc: string;
  entityName: string | null;
  primaryDistanceM: number | null;
  primaryWalkMinutes: number | null;
  primaryDriveMinutes: number | null;
  primaryDriveMinutesEstimated: boolean;
  fallbackDistanceM: number | null;
  fallbackWalkMinutes: number | null;
  fallbackDriveMinutes: number | null;
  fallbackDriveMinutesEstimated: boolean;
  foundInPrimaryRadius: boolean;
  foundInRadiusM: number | null;
  hint: string | null;
};

export type ListingPageInfrastructureSearchFeaturesDto = {
  storeCountPrimary: number;
  nearestStoreWalkMinutes: number | null;
  nearestStoreDriveMinutes: number | null;
  nearestStoreDriveMinutesEstimated: boolean;
  nearestStoreAnyWalkMinutes: number | null;
  nearestStoreAnyDriveMinutes: number | null;
  nearestStoreAnyDriveMinutesEstimated: boolean;
  nearestStoreFoundRadiusM: number | null;
  pharmacyCountPrimary: number;
  nearestPharmacyWalkMinutes: number | null;
  nearestPharmacyDriveMinutes: number | null;
  nearestPharmacyDriveMinutesEstimated: boolean;
  nearestPharmacyAnyWalkMinutes: number | null;
  nearestPharmacyAnyDriveMinutes: number | null;
  nearestPharmacyAnyDriveMinutesEstimated: boolean;
  nearestPharmacyFoundRadiusM: number | null;
  stopsCountPrimary: number;
  nearestStopWalkMinutes: number | null;
  nearestStopDriveMinutes: number | null;
  nearestStopDriveMinutesEstimated: boolean;
  nearestStopAnyWalkMinutes: number | null;
  nearestStopAnyDriveMinutes: number | null;
  nearestStopAnyDriveMinutesEstimated: boolean;
  nearestStopFoundRadiusM: number | null;
  stopGroupsCountPrimary: number;
  stopPlatformsCountPrimary: number;
  nearestStopGroupWalkMinutes: number | null;
  nearestStopGroupDriveMinutes: number | null;
  nearestStopGroupDriveMinutesEstimated: boolean;
  nearestStopGroupName: string | null;
  nearestStopGroupPlatformCount: number | null;
  nearestStopGroupAnyWalkMinutes: number | null;
  nearestStopGroupAnyDriveMinutes: number | null;
  nearestStopGroupAnyDriveMinutesEstimated: boolean;
  nearestStopGroupFoundRadiusM: number | null;
  parkingCountPrimary: number;
  nearestParkingWalkMinutes: number | null;
  nearestParkingDriveMinutes: number | null;
  nearestParkingDriveMinutesEstimated: boolean;
  nearestParkingAnyWalkMinutes: number | null;
  nearestParkingAnyDriveMinutes: number | null;
  nearestParkingAnyDriveMinutesEstimated: boolean;
  nearestParkingFoundRadiusM: number | null;
  schoolCountPrimary: number;
  nearestSchoolWalkMinutes: number | null;
  nearestSchoolDriveMinutes: number | null;
  nearestSchoolDriveMinutesEstimated: boolean;
  nearestSchoolAnyWalkMinutes: number | null;
  nearestSchoolAnyDriveMinutes: number | null;
  nearestSchoolAnyDriveMinutesEstimated: boolean;
  nearestSchoolFoundRadiusM: number | null;
  kindergartenCountPrimary: number;
  nearestKindergartenWalkMinutes: number | null;
  nearestKindergartenDriveMinutes: number | null;
  nearestKindergartenDriveMinutesEstimated: boolean;
  nearestKindergartenAnyWalkMinutes: number | null;
  nearestKindergartenAnyDriveMinutes: number | null;
  nearestKindergartenAnyDriveMinutesEstimated: boolean;
  nearestKindergartenFoundRadiusM: number | null;
  clinicCountPrimary: number;
  nearestClinicWalkMinutes: number | null;
  nearestClinicDriveMinutes: number | null;
  nearestClinicDriveMinutesEstimated: boolean;
  nearestClinicAnyDriveMinutes: number | null;
  nearestClinicAnyDriveMinutesEstimated: boolean;
  nearestClinicFoundRadiusM: number | null;
  hospitalCountPrimary: number;
  nearestHospitalDriveMinutes: number | null;
  nearestHospitalDriveMinutesEstimated: boolean;
  nearestHospitalAnyDriveMinutes: number | null;
  nearestHospitalAnyDriveMinutesEstimated: boolean;
  nearestHospitalFoundRadiusM: number | null;
  leisureCountPrimary: number;
  nearestLeisureWalkMinutes: number | null;
  nearestLeisureAnyWalkMinutes: number | null;
  nearestLeisureFoundRadiusM: number | null;
  nearestMetroWalkMinutes: number | null;
  metroFoundInPrimaryRadius: boolean;
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
  rawMapPois: ListingPagePoiDto[];
  nearbyCategories: ListingPageInfrastructureNearbyCategoryDto[];
  transportDetails: ListingPageInfrastructureTransportDetailsDto;
  groupedTransportSummary: ListingPageGroupedTransportSummaryDto;
  compactInfrastructureSummary: ListingPageCompactInfrastructureItemDto[];
  searchFeatures: ListingPageInfrastructureSearchFeaturesDto;
  missingPrimaryCategories: string[];
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
