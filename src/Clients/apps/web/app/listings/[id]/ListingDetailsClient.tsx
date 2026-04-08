"use client";

import { useListingPageQuery } from "@diploma/features";
import { useQueryClient } from "@tanstack/react-query";
import {
  ChartPie,
  ChevronDown,
  GraduationCap,
  HeartPulse,
  ShoppingBag,
  Trees,
  TramFront,
} from "lucide-react";
import type { CSSProperties, ReactNode } from "react";
import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { AppShell } from "../../components/AppShell";
import { useAuth } from "../../providers";
import { ListingGallery } from "./ListingGallery";
import { ListingLocationMap } from "./ListingLocationMap";
import { ListingPriceAnalytics } from "./components/ListingPriceAnalytics";
import { useViewedListings } from "../lib/listing-collections";

const fallbackImage =
  "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1200 760'%3E%3Crect width='1200' height='760' fill='%23efe8ff'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%235326d4' font-family='Arial' font-size='54'%3EListing%20Preview%3C/text%3E%3C/svg%3E";

function formatPrice(value?: number | null) {
  if (value == null) return "Цена по запросу";
  return new Intl.NumberFormat("ru-RU", {
    minimumFractionDigits: 0,
    maximumFractionDigits: Number.isInteger(value) ? 0 : 2,
  }).format(value);
}

function formatNumericPricePerSquareMeter(amount?: number | null, area?: number | null) {
  if (amount == null || area == null || area <= 0) {
    return null;
  }

  return new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 0 }).format(amount / area);
}

function formatDealType(value?: string | null) {
  if (!value) return "Сделка не указана";
  if (value === "Продажа" || value === "Аренда") return value;
  if (value === "Sale") return "Продажа";
  if (value === "Rent") return "Аренда";
  return value;
}

function formatPublishedDate(value?: string | null) {
  if (!value) {
    return null;
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return null;
  }

  return new Intl.DateTimeFormat("ru-BY", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    timeZone: "Europe/Minsk",
  }).format(parsed);
}

const detailLabelMap: Record<string, string> = {
  plotArea: "Площадь участка",
  allowedUse: "Назначение",
  hasElectricity: "Электричество",
  hasGas: "Газ",
  hasWater: "Вода",
  hasSewer: "Канализация",
  hasBuildings: "Постройки",
  landCategory: "Категория участка",
  roadAccessType: "Подъезд",
  rooms: "Комнаты",
  floor: "Этаж",
  floorsTotal: "Этажей",
  areaLiving: "Жилая площадь",
  areaKitchen: "Площадь кухни",
  ceilingHeight: "Высота потолков",
  bathroomType: "Санузел",
  renovationType: "Ремонт",
  buildingMaterial: "Материал",
  hasBalcony: "Балкон",
  hasLoggia: "Лоджия",
  hasElevator: "Лифт",
  houseArea: "Площадь дома",
  floors: "Этажность дома",
  wallMaterial: "Материал стен",
  heatingType: "Отопление",
  sewerType: "Канализация",
  gasType: "Газоснабжение",
  roofMaterial: "Кровля",
  waterSupplyType: "Водоснабжение",
  officeClass: "Класс офиса",
  layoutType: "Планировка",
  officeRenovationType: "Ремонт офиса",
  parkingType: "Парковка",
  parkingSpaces: "Мест на парковке",
  kitchenette: "Мини-кухня",
  warehouseCeilingHeight: "Высота потолков",
  floorLoadTonsPerSquareMeter: "Нагрузка на пол",
  gateCount: "Количество ворот",
  hasDockLevelers: "Доклевеллеры",
  dockCount: "Доков",
  hasRamps: "Рампы",
  powerKw: "Мощность",
  hasOffice: "Офисная часть",
  officeArea: "Площадь офиса",
  classType: "Класс",
  warehouseHeatingType: "Отопление склада",
};

const detailValueMap: Record<string, string> = {
  True: "Да",
  False: "Нет",
  true: "Да",
  false: "Нет",
  Rent: "Аренда",
  Sale: "Продажа",
  Apartment: "Квартира",
  House: "Дом",
  Land: "Участок",
  Office: "Офис",
  Warehouse: "Склад",
  Other: "Другое",
  Brick: "Кирпич",
  Panel: "Панель",
  Monolith: "Монолит",
  "Monolith-Brick": "Монолит-кирпич",
  Wood: "Дерево",
  "Block (foam/gas block)": "Блоки (пеноблок / газоблок)",
  "Metal frame": "Металлокаркас",
  Concrete: "Бетон",
  "Sandwich panels": "Сэндвич-панели",
  Metal: "Металл",
  Central: "Центральное",
  "Autonomous (building)": "Автономное (на здание)",
  "Individual gas boiler": "Индивидуальный газовый котел",
  Electric: "Электрическое",
  "Solid fuel": "Твердотопливное",
  Well: "Колодец",
  Borehole: "Скважина",
  "Delivered / tank": "Привозная вода / емкость",
  "Septic tank": "Септик",
  Cesspool: "Выгребная яма",
  "Central gas": "Центральный газ",
  "Gas holder / tank": "Газгольдер / резервуар",
  "Bottled gas": "Баллонный газ",
  "Residential development": "Жилая застройка",
  "Personal subsidiary farming": "Личное подсобное хозяйство",
  "Garden / dacha plot": "Садовый / дачный участок",
  Agricultural: "Сельскохозяйственная",
  "Industrial / commercial": "Промышленная / коммерческая",
  "Forestry / protected area": "Лесной фонд / охраняемая территория",
  Asphalt: "Асфальт",
  Gravel: "Гравий",
  "Dirt road": "Грунтовая дорога",
  "No direct access / off-road": "Нет прямого подъезда / бездорожье",
  "Open space": "Открытая планировка",
  "Cabinet / rooms": "Кабинеты / комнаты",
  Mixed: "Смешанная",
  Coworking: "Коворкинг",
  "Combined (shared)": "Совмещенный",
  Separate: "Раздельный",
  "Two or more": "Два и более",
  "Bathroom + WC (separate rooms)": "Ванная и туалет раздельно",
  "No renovation / requires renovation": "Без ремонта / требует ремонта",
  Cosmetic: "Косметический",
  "Major / Capital": "Капитальный",
  Designer: "Дизайнерский",
  "Shell & Core": "Под чистовую отделку",
  "Finished / Turnkey": "С отделкой / под ключ",
  "Apartment Building (Multi-family)": "Многоквартирный дом",
  "Detached House": "Отдельный дом",
  Townhouse: "Таунхаус",
  "Business Center": "Бизнес-центр",
  "Warehouse Complex": "Складской комплекс",
  "Industrial Building": "Промышленное здание",
  "Shopping Mall": "Торговый центр",
  "Mixed-use Building": "Многофункциональное здание",
};

function normalizePoiName(name: string) {
  const trimmed = name.trim();
  if (!trimmed || trimmed === "Инфраструктурный объект" || trimmed === "Транспортный узел") {
    return null;
  }

  return trimmed;
}

function WalkIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <circle cx="12" cy="4.5" r="2.5" fill="currentColor" />
      <path d="M10 9.5L14 8L16.5 10" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M12 10L10.5 14L7.5 17" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M12.5 12L15 15L16.5 20" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M10 20L12 16" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function CarIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path d="M6.5 9.5L8 6.5C8.3 5.9 8.9 5.5 9.6 5.5H14.4C15.1 5.5 15.7 5.9 16 6.5L17.5 9.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
      <path d="M4.5 10.5H19.5V15.5C19.5 16.052 19.052 16.5 18.5 16.5H5.5C4.948 16.5 4.5 16.052 4.5 15.5V10.5Z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" />
      <circle cx="7.5" cy="16.5" r="1.5" fill="currentColor" />
      <circle cx="16.5" cy="16.5" r="1.5" fill="currentColor" />
    </svg>
  );
}

function getPoiSubcategoryLabel(value?: string | null) {
  switch (value) {
    case "supermarket":
      return "Супермаркет";
    case "hypermarket":
      return "Гипермаркет";
    case "discount_store":
      return "Дискаунтер";
    case "convenience_store":
      return "Магазин у дома";
    case "mall":
      return "Торговый центр";
    case "electronics":
      return "Электроника";
    case "furniture":
      return "Мебель";
    case "building_store":
      return "Стройтовары";
    case "beauty_service":
      return "Красота и уход";
    case "drogerie":
      return "Товары для дома";
    case "pharmacy":
      return "Аптека";
    case "clinic":
      return "Поликлиника";
    case "hospital":
      return "Больница";
    case "dentistry":
      return "Стоматология";
    case "laboratory":
      return "Лаборатория";
    case "bus_stop":
      return "Автобусная остановка";
    case "tram_stop":
      return "Трамвайная остановка";
    case "trolleybus_stop":
      return "Троллейбусная остановка";
    case "metro_station":
      return "Станция метро";
    case "railway_station":
      return "Железнодорожная станция";
    case "parking":
      return "Парковка";
    case "fuel_station":
      return "Заправка";
    case "service_station":
      return "СТО / автомойка";
    case "kindergarten":
      return "Детский сад";
    case "school":
      return "Школа";
    case "college":
      return "Колледж";
    case "university":
      return "Университет";
    case "park":
      return "Парк";
    case "square":
      return "Сквер";
    case "waterfront":
      return "Набережная";
    case "cinema":
      return "Кинотеатр";
    case "cafe":
      return "Кафе";
    case "restaurant":
      return "Ресторан";
    case "bar":
      return "Бар";
    case "fitness":
      return "Фитнес";
    case "gym":
      return "Спортзал";
    case "pool":
      return "Бассейн";
    case "stadium":
      return "Стадион";
    case "sports_center":
      return "Спортцентр";
    case "museum":
      return "Музей";
    case "theater":
      return "Театр";
    case "gallery":
      return "Галерея";
    case "exhibition_hall":
      return "Выставочный зал";
    case "library":
      return "Библиотека";
    case "landmark":
      return "Достопримечательность";
    case "recreation_zone":
      return "Зона отдыха";
    default:
      return null;
  }
}

function getPrimaryCategoryLabel(value: string) {
  switch (value) {
    case "store":
      return "магазины";
    case "pharmacy":
      return "аптеки";
    case "stop":
      return "остановки";
    case "parking":
      return "парковки";
    case "school":
      return "школы";
    case "kindergarten":
      return "детские сады";
    case "clinic":
      return "поликлиники";
    case "hospital":
      return "больницы";
    case "leisure":
      return "объекты досуга";
    case "metro_station":
      return "метро";
    default:
      return value;
  }
}

function formatScoreLabel(score?: number | null) {
  if (score == null) {
    return "—";
  }

  return score.toFixed(1);
}

function formatMinutesLabel(value?: number | null, estimated?: boolean) {
  if (value == null) {
    return "не найдено";
  }

  return `${estimated ? "~" : ""}${value.toFixed(0)} мин`;
}

function formatStopTypes(stopTypes: string[]) {
  const labels = stopTypes
    .map((stopType) => getPoiSubcategoryLabel(stopType))
    .filter((label): label is Exclude<typeof label, null> => Boolean(label));

  return labels.join(", ");
}

function getCategoryAccent(category: string) {
  switch (category) {
    case "Коммерция":
      return "#0f766e";
    case "Здоровье":
      return "#c2410c";
    case "Транспорт":
      return "#2563eb";
    case "Образование":
      return "#7c3aed";
    case "Досуг":
      return "#15803d";
    default:
      return "#475569";
  }
}

function ScoreDial({
  label,
  value,
  color,
  icon,
}: {
  label: string;
  value?: number | null;
  color: string;
  icon: ReactNode;
}) {
  const normalized = Math.max(0, Math.min(100, value ?? 0));
  const circumference = 2 * Math.PI * 32;
  const offset = circumference - (normalized / 100) * circumference;

  return (
    <div className="listing-details-score-card grid justify-items-center gap-3 rounded-[26px] border border-[var(--border-color)] bg-[var(--background-elevated)] px-5 py-5">
      <div className="listing-details-score-dial relative grid h-[92px] w-[92px] place-items-center">
        <svg viewBox="0 0 88 88" className="h-[92px] w-[92px] -rotate-90">
          <circle cx="44" cy="44" r="32" fill="none" stroke="color-mix(in srgb, var(--border-color) 72%, white)" strokeWidth="8" />
          <circle
            cx="44"
            cy="44"
            r="32"
            fill="none"
            stroke={color}
            strokeWidth="8"
            strokeLinecap="round"
            strokeDasharray={circumference}
            strokeDashoffset={offset}
          />
        </svg>
        <div className="absolute inset-0 grid place-items-center">
          <div className="listing-details-score-icon grid h-[56px] w-[56px] place-items-center rounded-full text-white shadow-sm" style={{ background: color }}>
            {icon}
          </div>
        </div>
      </div>
      <div className="listing-details-score-copy text-center">
        <div className="listing-details-score-value text-[1.55rem] font-black tracking-[-0.05em]" style={{ color }}>
          {formatScoreLabel(value)}
        </div>
        <div className="listing-details-score-label mt-1 text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
          {label}
        </div>
      </div>
    </div>
  );
}

function getSellerInitials(name?: string | null) {
  if (!name) {
    return "R";
  }

  const initials = name
    .split(" ")
    .map((part) => part.trim()[0])
    .filter(Boolean)
    .slice(0, 2)
    .join("")
    .toUpperCase();

  return initials || "R";
}

function formatDetailValue(value: unknown) {
  if (typeof value === "boolean") {
    return value ? "Да" : "Нет";
  }

  if (typeof value === "number") {
    return new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 2 }).format(value);
  }

  if (typeof value === "string") {
    return detailValueMap[value] ?? value;
  }

  return String(value);
}

function normalizeDetailKey(key: string) {
  switch (key) {
    case "PlotArea":
      return "plotArea";
    case "Floor":
      return "floor";
    case "FloorsTotal":
      return "floorsTotal";
    case "Levels":
      return "levels";
    case "ParkingSpaces":
      return "parkingSpaces";
    case "Kitchenette":
      return "kitchenette";
    case "OfficeClass":
      return "officeClass";
    case "LayoutType":
      return "layoutType";
    case "RenovationType":
      return "renovationType";
    case "ParkingType":
      return "parkingType";
    default:
      return key.length > 1
        ? `${key.charAt(0).toLowerCase()}${key.slice(1)}`
        : key.toLowerCase();
  }
}

function getDetailEntries(details: Record<string, unknown> | null | undefined) {
  if (!details) {
    return [];
  }

  return Object.entries(details)
    .map(([key, value]) => [normalizeDetailKey(key), value] as const)
    .filter(([, value]) => value !== null && value !== undefined && value !== "")
    .map(([key, value]) => ({
      key,
      label: detailLabelMap[key] ?? key,
      value: formatDetailValue(value),
    }));
}

function buildUniqueListKey(
  baseKey: string,
  usedKeys: Set<string>,
  fallbackParts: Array<string | number | null | undefined>,
) {
  if (!usedKeys.has(baseKey)) {
    usedKeys.add(baseKey);
    return baseKey;
  }

  const fallbackKey = `${baseKey}-${fallbackParts
    .filter((part): part is string | number => part !== null && part !== undefined && part !== "")
    .join("-")}`;

  if (!usedKeys.has(fallbackKey)) {
    usedKeys.add(fallbackKey);
    return fallbackKey;
  }

  let suffix = 2;
  let candidate = `${fallbackKey}-${suffix}`;
  while (usedKeys.has(candidate)) {
    suffix += 1;
    candidate = `${fallbackKey}-${suffix}`;
  }

  usedKeys.add(candidate);
  return candidate;
}

type InfrastructureListItem = {
  key: string;
  title: string;
  subtitle: string | null;
  distanceMeters: number;
  addressText: string | null | undefined;
  subcategoryKey: string;
};

type InfrastructureSubcategoryGroup = {
  key: string;
  label: string;
  count: number;
  items: InfrastructureListItem[];
};

type InfrastructureCategoryGroup = {
  category: string;
  count: number;
  subcategories: InfrastructureSubcategoryGroup[];
};

const bynIconStyle: CSSProperties = {
  WebkitMaskImage: "url('/byn.svg')",
  maskImage: "url('/byn.svg')",
  WebkitMaskRepeat: "no-repeat",
  maskRepeat: "no-repeat",
  WebkitMaskPosition: "center",
  maskPosition: "center",
  WebkitMaskSize: "contain",
  maskSize: "contain",
};

const dollarIconStyle: CSSProperties = {
  WebkitMaskImage: "url('/dollar.svg')",
  maskImage: "url('/dollar.svg')",
  WebkitMaskRepeat: "no-repeat",
  maskRepeat: "no-repeat",
  WebkitMaskPosition: "center",
  maskPosition: "center",
  WebkitMaskSize: "108% 108%",
  maskSize: "108% 108%",
};

type Props = {
  listingId: string;
};

export function ListingDetailsClient({ listingId }: Props) {
  const { api } = useAuth();
  const viewed = useViewedListings();
  const { pushViewed } = viewed;
  const queryClient = useQueryClient();
  const { data, isLoading, error } = useListingPageQuery(api, listingId);
  const [activeInfrastructureCategory, setActiveInfrastructureCategory] = useState<string | null>(null);
  const [activeInfrastructureSubcategory, setActiveInfrastructureSubcategory] = useState<string | null>(null);
  const [isInfrastructurePanelCollapsed, setIsInfrastructurePanelCollapsed] = useState(false);
  const isMissingListingError = error instanceof Error && error.message.includes("404");

  useEffect(() => {
    if (!isMissingListingError) {
      return;
    }

    void queryClient.invalidateQueries({ queryKey: ["search-listings"] });
    void queryClient.invalidateQueries({ queryKey: ["listings"] });
  }, [isMissingListingError, queryClient]);

  useEffect(() => {
    if (!listingId) {
      return;
    }

    pushViewed(listingId);
  }, [listingId, pushViewed]);

  const infrastructure = data?.infrastructure;
  const groupedInfrastructurePois = useMemo(() => infrastructure?.groupedPois ?? [], [infrastructure?.groupedPois]);
  const groupedInfrastructureCategories = useMemo<InfrastructureCategoryGroup[]>(() => {
    if (!infrastructure) {
      return [];
    }

    return groupedInfrastructurePois.map((group) => {
      if (group.category !== "Транспорт") {
        const groupedItems = group.pois.reduce<Record<string, InfrastructureSubcategoryGroup>>((accumulator, poi) => {
          const subcategoryKey = poi.subcategoryCode ?? "other";
          const bucket = accumulator[subcategoryKey] ?? {
            key: subcategoryKey,
            label: getPoiSubcategoryLabel(poi.subcategoryCode) ?? "Прочее",
            count: 0,
            items: [],
          };

          bucket.items.push({
            key: poi.id,
            title: normalizePoiName(poi.name) ?? "Объект инфраструктуры",
            subtitle: poi.subcategoryCode
              ? (getPoiSubcategoryLabel(poi.subcategoryCode) ?? "Инфраструктурный объект")
              : null,
            distanceMeters: poi.distanceMeters,
            addressText: poi.addressText,
            subcategoryKey,
          });
          bucket.count += 1;
          accumulator[subcategoryKey] = bucket;
          return accumulator;
        }, {});

        return {
          category: group.category,
          count: group.count,
          subcategories: Object.values(groupedItems)
            .map((entry) => ({
              ...entry,
              items: entry.items.sort((left, right) => left.distanceMeters - right.distanceMeters),
            }))
            .sort((left, right) => left.items[0]!.distanceMeters - right.items[0]!.distanceMeters),
        };
      }

      const usedTransportKeys = new Set<string>();
      const stopGroupItems: InfrastructureListItem[] = infrastructure.transportDetails.stopGroups
        .slice()
        .sort((left, right) => left.nearestDistanceMeters - right.nearestDistanceMeters)
        .map((groupItem) => ({
          key: buildUniqueListKey(
            `stop-group-${groupItem.groupId}`,
            usedTransportKeys,
            [
              groupItem.normalizedName,
              groupItem.nearestDistanceMeters,
              groupItem.platformCount,
              groupItem.pointCount,
              groupItem.sourcePoiIds.slice().sort().join("-"),
            ],
          ),
          title: groupItem.displayName,
          subtitle: [
            formatStopTypes(groupItem.stopTypes),
            groupItem.platformCount > 1 ? `${groupItem.platformCount} платформы` : null,
          ].filter((value): value is string => Boolean(value)).join(" • ") || "Остановочная группа",
          distanceMeters: groupItem.nearestDistanceMeters,
          addressText: groupItem.pointCount > 1
            ? `Объединено точек остановки: ${groupItem.pointCount}`
            : null,
          subcategoryKey: "stop_group",
        }));

      const nonGroupedTransportItems: InfrastructureListItem[] = (infrastructure.rawMapPois ?? infrastructure.nearbyPois)
        .filter((poi) => poi.category === "Транспорт")
        .filter((poi) => !["bus_stop", "tram_stop", "trolleybus_stop"].includes(poi.subcategoryCode ?? ""))
        .sort((left, right) => left.distanceMeters - right.distanceMeters)
        .map((poi) => ({
          key: buildUniqueListKey(`poi-${poi.id}`, usedTransportKeys, [poi.name, poi.distanceMeters, poi.addressText]),
          title: normalizePoiName(poi.name) ?? "Транспортный объект",
          subtitle: poi.subcategoryCode
            ? (getPoiSubcategoryLabel(poi.subcategoryCode) ?? "Транспортный объект")
            : "Транспортный объект",
          distanceMeters: poi.distanceMeters,
          addressText: poi.addressText,
          subcategoryKey: poi.subcategoryCode ?? "other",
        }));

      const transportSubcategories = [...stopGroupItems, ...nonGroupedTransportItems]
        .reduce<Record<string, InfrastructureSubcategoryGroup>>((accumulator, item) => {
          const bucket = accumulator[item.subcategoryKey] ?? {
            key: item.subcategoryKey,
            label: item.subcategoryKey === "stop_group"
              ? "Остановки"
              : (getPoiSubcategoryLabel(item.subcategoryKey) ?? "Транспорт"),
            count: 0,
            items: [],
          };

          bucket.items.push(item);
          bucket.count += 1;
          accumulator[item.subcategoryKey] = bucket;
          return accumulator;
        }, {});

      return {
        category: group.category,
        count: stopGroupItems.length + nonGroupedTransportItems.length,
        subcategories: Object.values(transportSubcategories)
          .map((entry) => ({
            ...entry,
            items: entry.items.sort((left, right) => left.distanceMeters - right.distanceMeters),
          }))
          .sort((left, right) => left.items[0]!.distanceMeters - right.items[0]!.distanceMeters),
      };
    });
  }, [groupedInfrastructurePois, infrastructure]);

  const selectedInfrastructureCategory = groupedInfrastructureCategories.find((group) => group.category === activeInfrastructureCategory)
    ?? groupedInfrastructureCategories[0]
    ?? null;
  const selectedInfrastructureSubcategory = selectedInfrastructureCategory?.subcategories.find(
    (subcategory) => subcategory.key === activeInfrastructureSubcategory,
  ) ?? selectedInfrastructureCategory?.subcategories[0] ?? null;

  useEffect(() => {
    if (!selectedInfrastructureCategory) {
      if (activeInfrastructureSubcategory !== null) {
        setActiveInfrastructureSubcategory(null);
      }
      return;
    }

    const hasActiveSubcategory = selectedInfrastructureCategory.subcategories.some(
      (subcategory) => subcategory.key === activeInfrastructureSubcategory,
    );

    if (!hasActiveSubcategory) {
      setActiveInfrastructureSubcategory(selectedInfrastructureCategory.subcategories[0]?.key ?? null);
    }
  }, [activeInfrastructureSubcategory, selectedInfrastructureCategory]);

  if (isLoading) {
    return (
      <AppShell>
        <main className="grid gap-6 pb-10">
          <section className="surface rounded-[28px] p-8">Загрузка объявления...</section>
        </main>
      </AppShell>
    );
  }

  if (error || !data) {
    return (
      <AppShell>
        <main className="grid gap-6 pb-10">
          <section className="surface grid gap-4 rounded-[28px] p-8 text-[#d04b73]">
            <div>
              {isMissingListingError
                ? "Объявление удалено или больше не существует."
                : "Не удалось загрузить страницу объявления."}
            </div>
            {isMissingListingError ? (
              <div>
                <Link className="button-secondary" href="/listings">
                  Вернуться в каталог
                </Link>
              </div>
            ) : null}
          </section>
        </main>
      </AppShell>
    );
  }

  const { listing, seller, derived } = data;
  const galleryMedia = [
    listing.mainMedia,
    ...listing.media.map((item) => item.url),
  ]
    .filter((item): item is string => Boolean(item))
    .filter((item, index, array) => array.indexOf(item) === index);
  const media = galleryMedia.length > 0 ? galleryMedia : [fallbackImage];
  const detailEntries = getDetailEntries(listing.listingDetails);
  const publishedDateLabel = formatPublishedDate(listing.publishedDate);
  const bynPricePerSquareMeter = formatNumericPricePerSquareMeter(
    listing.priceAmountByn ?? listing.priceAmount,
    listing.areaTotal,
  );
  const usdPricePerSquareMeter = formatNumericPricePerSquareMeter(
    listing.priceAmountUsd,
    listing.areaTotal,
  );
  const infrastructureFacts = infrastructure?.compactInfrastructureSummary ?? [];
  const groupedTransportSummary = infrastructure?.groupedTransportSummary ?? null;
  const infrastructureScoreCards = infrastructure ? [
    { key: "transport", label: "Транспорт", value: infrastructure.transportScore, color: "#2563eb", icon: <TramFront className="h-6 w-6" /> },
    { key: "commerce", label: "Коммерция", value: infrastructure.commerceScore, color: "#0f766e", icon: <ShoppingBag className="h-6 w-6" /> },
    { key: "education", label: "Образование", value: infrastructure.educationScore, color: "#7c3aed", icon: <GraduationCap className="h-6 w-6" /> },
    { key: "healthcare", label: "Здоровье", value: infrastructure.healthcareScore, color: "#c2410c", icon: <HeartPulse className="h-6 w-6" /> },
    { key: "leisure", label: "Досуг", value: infrastructure.leisureScore, color: "#15803d", icon: <Trees className="h-6 w-6" /> },
    { key: "total", label: "Общая оценка", value: infrastructure.totalScore, color: "#111827", icon: <ChartPie className="h-6 w-6" /> },
  ] : [];
  const infoCardClass = "rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4";

  return (
    <AppShell>
      <main className="listing-details-page grid gap-6 pb-10" style={{ minWidth: 0, width: "100%", maxWidth: "100%", overflowX: "clip" }}>
        <section
          className="listing-details-top grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]"
          style={{ minWidth: 0, width: "100%", maxWidth: "100%", overflowX: "clip" }}
        >
          <article className="surface overflow-hidden rounded-[32px]" style={{ minWidth: 0, width: "100%", maxWidth: "100%" }}>
            <ListingGallery title={listing.title} media={media} />
          </article>

          <aside
            className="listing-details-sidebar grid gap-5 self-start"
            style={{ minWidth: 0, width: "100%", maxWidth: "100%", overflowX: "clip" }}
          >
            <section className="surface rounded-[28px] p-6" style={{ minWidth: 0, width: "100%", maxWidth: "100%", overflowX: "clip" }}>
              <div className="eyebrow">Продавец</div>
              <div className="mt-4 flex items-start gap-4">
                <div
                  className="overflow-hidden rounded-[20px] border border-[var(--border-color)] bg-[var(--background-muted)]"
                  style={{ width: 72, height: 72, flexShrink: 0 }}
                >
                  {seller.avatarUrl ? (
                    <img
                      alt={seller.fullName ?? "Продавец"}
                      src={seller.avatarUrl}
                      style={{ width: "100%", height: "100%", objectFit: "cover" }}
                    />
                  ) : (
                    <div
                      className="grid h-full w-full place-items-center text-[1.15rem] font-black tracking-[-0.05em] text-[var(--color-primary-strong)]"
                    >
                      {getSellerInitials(seller.fullName)}
                    </div>
                  )}
                </div>

                <div className="min-w-0">
                  <div className="text-[1.35rem] font-extrabold tracking-[-0.03em]">
                    {seller.fullName ?? "Контакты продавца уточняются"}
                  </div>
                  {seller.agencyName ? (
                    <div className="mt-1 text-sm text-[var(--foreground-muted)]">{seller.agencyName}</div>
                  ) : null}
                  {seller.isVerified ? (
                    <div className="mt-2 inline-flex rounded-full bg-[var(--background-muted)] px-3 py-1 text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                      Проверенный профиль
                    </div>
                  ) : null}
                </div>
              </div>
              <div className="mt-4 grid gap-2 text-sm">
                {seller.phone ? (
                  <a
                    href={`tel:${seller.phone}`}
                    className={infoCardClass}
                    style={{ textDecoration: "none", color: "inherit" }}
                  >
                    Телефон: {seller.phone}
                  </a>
                ) : null}
                {seller.email ? <div>Электронная почта: {seller.email}</div> : null}
              </div>
            </section>

            <ListingPriceAnalytics
              listingId={listingId}
              currentPriceAmountByn={listing.priceAmountByn ?? listing.priceAmount ?? null}
              areaTotal={listing.areaTotal ?? 0}
            />

          </aside>
        </section>

        <section
          className="surface listing-details-summary rounded-[32px] p-8"
          style={{ minWidth: 0, width: "100%", maxWidth: "100%", overflowX: "clip" }}
        >
          <div className="grid gap-6">
            <div className="listing-details-summary-grid grid gap-4 xl:grid-cols-[minmax(0,1.25fr)_minmax(320px,0.75fr)] xl:items-start">
              <div className="grid gap-3">
                <span className="eyebrow">{listing.addressText ?? "Адрес уточняется"}</span>
                <h1 className="page-title listing-details-title" style={{ fontSize: "clamp(2.4rem, 4vw, 4.2rem)" }}>
                  {listing.title}
                </h1>
                <div className="flex flex-wrap gap-2 text-sm font-semibold text-[var(--foreground-muted)]">
                  <span className="rounded-full bg-[var(--background-muted)] px-3 py-1.5">
                    {formatDealType(derived.quickStats.find((item) => item.key === "dealType")?.value)}
                  </span>
                  {publishedDateLabel ? (
                    <span className="rounded-full bg-[var(--background-muted)] px-3 py-1.5">
                      Опубликовано: {publishedDateLabel}
                    </span>
                  ) : null}
                  {listing.buildYear ? (
                    <span className="rounded-full bg-[var(--background-muted)] px-3 py-1.5">
                      Год постройки: {listing.buildYear}
                    </span>
                  ) : null}
                </div>
              </div>

              <div className="listing-details-price-group flex flex-wrap items-start gap-3 xl:justify-end" style={{ minWidth: 0 }}>
                <div
                  className="listing-details-price-card inline-flex max-w-full items-center rounded-[24px] border border-[var(--border-color)] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_64%,var(--background-elevated))] px-5 py-4 font-black tracking-[-0.05em] text-[var(--color-primary-strong)]"
                  style={{ minWidth: 0, width: "100%", flexWrap: "wrap", overflowWrap: "anywhere", gap: 8 }}
                >
                  <span className="inline-flex items-center" style={{ minWidth: 0, maxWidth: "100%", flexWrap: "wrap", gap: 2 }}>
                    <span className="listing-details-price-main" style={{ lineHeight: 1 }}>
                      {formatPrice(listing.priceAmountByn ?? listing.priceAmount)}
                    </span>
                    <span
                      className="inline-block h-[0.92em] w-[0.74em] shrink-0 bg-current align-[-0.08em]"
                      style={bynIconStyle}
                      aria-hidden="true"
                    />
                  </span>
                  {listing.priceAmountUsd != null ? (
                    <>
                      <span className="listing-details-price-divider font-extrabold text-[var(--foreground-muted)]" style={{ marginInline: 2 }}>
                        /
                      </span>
                      <span className="inline-flex items-center" style={{ minWidth: 0, maxWidth: "100%", flexWrap: "wrap", gap: 2 }}>
                        <span className="listing-details-price-secondary" style={{ lineHeight: 1 }}>
                          {formatPrice(listing.priceAmountUsd)}
                        </span>
                        <span
                          className="inline-block h-[0.92em] w-[0.82em] shrink-0 bg-current align-[-0.08em]"
                          style={dollarIconStyle}
                          aria-hidden="true"
                        />
                      </span>
                    </>
                  ) : null}
                </div>

                {(bynPricePerSquareMeter || usdPricePerSquareMeter) ? (
                  <div
                    className="grid gap-1.5 rounded-[22px] border border-[var(--border-color)] bg-[var(--background-elevated)] px-4 py-3 font-bold tracking-[-0.03em] text-[var(--foreground)]"
                    style={{ minWidth: 0, width: "100%", maxWidth: "100%", overflowWrap: "anywhere", fontSize: "0.72rem" }}
                  >
                    {bynPricePerSquareMeter ? (
                      <span className="inline-flex items-center gap-1" style={{ minWidth: 0, maxWidth: "100%", flexWrap: "wrap" }}>
                        <span style={{ fontSize: "0.68rem", lineHeight: 1 }}>{bynPricePerSquareMeter}</span>
                        <span
                          className="inline-block h-[0.88em] w-[0.7em] shrink-0 bg-current align-[-0.08em]"
                          style={bynIconStyle}
                          aria-hidden="true"
                        />
                        <span className="text-[var(--foreground-muted)]" style={{ fontSize: "0.66rem", lineHeight: 1 }}>/м²</span>
                      </span>
                    ) : null}
                    {usdPricePerSquareMeter ? (
                      <span className="inline-flex items-center gap-1" style={{ minWidth: 0, maxWidth: "100%", flexWrap: "wrap" }}>
                        <span style={{ fontSize: "0.68rem", lineHeight: 1 }}>{usdPricePerSquareMeter}</span>
                        <span
                          className="inline-block h-[0.9em] w-[0.76em] shrink-0 bg-current align-[-0.08em]"
                          style={dollarIconStyle}
                          aria-hidden="true"
                        />
                        <span className="text-[var(--foreground-muted)]" style={{ fontSize: "0.66rem", lineHeight: 1 }}>/м²</span>
                      </span>
                    ) : null}
                  </div>
                ) : null}
              </div>
            </div>

            <div className="flex flex-wrap gap-2">
              {derived.featureTags.map((tag) => (
                <span
                  key={tag.key}
                  className="rounded-full border border-[var(--border-color)] bg-[var(--background-elevated)] px-3 py-1.5 text-[12px] font-semibold"
                >
                  {tag.label}
                </span>
              ))}
            </div>

            <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
              {derived.quickStats.map((stat) => (
                <div key={stat.key} className={infoCardClass}>
                  <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                    {stat.label}
                  </div>
                  <div className="mt-1 break-words text-[1.2rem] leading-tight font-extrabold tracking-[-0.03em] whitespace-normal">
                    {stat.value}{stat.unit ? ` ${stat.unit}` : ""}
                  </div>
                </div>
              ))}
            </div>

            <section className="grid gap-3">
              <h2 className="section-title">Описание</h2>
              <p style={{ margin: 0, color: "var(--foreground-muted)", lineHeight: 1.7 }}>
                {listing.description ?? "Описание пока не заполнено."}
              </p>
            </section>

            {detailEntries.length > 0 ? (
              <section className="grid gap-3">
                <h2 className="section-title">Характеристики</h2>
                <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
                  {detailEntries.map((entry) => (
                    <div key={entry.key} className={infoCardClass}>
                      <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                        {entry.label}
                      </div>
                      <div className="mt-1 text-[1rem] font-bold tracking-[-0.02em] text-[var(--foreground)]">
                        {entry.value}
                      </div>
                    </div>
                  ))}
                </div>
              </section>
            ) : null}

            {data.warnings.length > 0 ? (
              <section className="grid gap-2">
                <h2 className="section-title">Ограничения данных</h2>
                <div className="grid gap-2">
                  {data.warnings.map((warning) => (
                    <div key={warning.code} className="rounded-[18px] border border-[#f0c36d] bg-[#fff5df] px-4 py-3 text-sm text-[#8c5a00]">
                      {warning.message}
                    </div>
                  ))}
                </div>
              </section>
            ) : null}
          </div>
        </section>

        {infrastructure ? (
          <section className="surface rounded-[32px] p-6 sm:p-8">
            <section className="grid gap-5 rounded-[28px] border border-[var(--border-color)] bg-[linear-gradient(180deg,color-mix(in_srgb,var(--color-primary-soft)_45%,white)_0%,var(--background-elevated)_100%)] p-6">
              <div className="flex flex-wrap items-start justify-between gap-4">
                <div className="grid gap-2">
                  <div className="eyebrow">Инфраструктура рядом</div>
                  <h2 className="section-title">Оценка и ближайшие объекты</h2>
                  {infrastructure.summary ? (
                    <p style={{ margin: 0, color: "var(--foreground-muted)", lineHeight: 1.7 }}>
                      {infrastructure.summary}
                    </p>
                  ) : null}
                </div>

                <button
                  type="button"
                  onClick={() => setIsInfrastructurePanelCollapsed((current) => !current)}
                  className="inline-flex items-center gap-2 rounded-full border border-[var(--border-color)] bg-[var(--background-elevated)] px-4 py-2 text-sm font-semibold text-[var(--foreground)] transition hover:border-[color:color-mix(in_srgb,var(--color-primary)_28%,var(--border-color))]"
                >
                  <span>{isInfrastructurePanelCollapsed ? "Показать" : "Скрыть"}</span>
                  <ChevronDown className={`h-4 w-4 transition ${isInfrastructurePanelCollapsed ? "rotate-180" : ""}`} />
                </button>
              </div>

              <div
                className={`overflow-hidden transition-all duration-300 ease-out ${
                  isInfrastructurePanelCollapsed
                    ? "pointer-events-none max-h-0 translate-y-6 opacity-0"
                    : "max-h-[5000px] translate-y-0 opacity-100"
                }`}
              >
                <div className="grid gap-5 pt-1">
                  <div className="listing-details-score-grid grid gap-3 sm:grid-cols-2 xl:grid-cols-6">
                    {infrastructureScoreCards.map((card) => (
                      <ScoreDial
                        key={card.key}
                        label={card.label}
                        value={card.value}
                        color={card.color}
                        icon={card.icon}
                      />
                    ))}
                  </div>

                  {groupedInfrastructureCategories.length > 0 ? (
                    <div className="listing-details-category-grid grid gap-4 xl:grid-cols-[280px_minmax(0,1fr)]">
                      <div className="grid gap-2">
                        {groupedInfrastructureCategories.map((group) => {
                          const isActive = selectedInfrastructureCategory?.category === group.category;
                          const accent = getCategoryAccent(group.category);

                          return (
                            <button
                              key={group.category}
                              type="button"
                              onClick={() => setActiveInfrastructureCategory(group.category)}
                              className="flex items-center justify-between rounded-[22px] border px-4 py-3 text-left transition"
                              style={{
                                borderColor: isActive ? accent : "var(--border-color)",
                                background: isActive
                                  ? `color-mix(in srgb, ${accent} 10%, var(--background-elevated))`
                                  : "var(--background-elevated)",
                              }}
                            >
                              <div>
                                <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                                  Категория
                                </div>
                                <div className="mt-1 text-[1rem] font-extrabold tracking-[-0.02em] text-[var(--foreground)]">
                                  {group.category}
                                </div>
                              </div>
                              <div className="flex items-center gap-3">
                                <span
                                  className="grid h-9 min-w-9 place-items-center rounded-full px-2 text-[12px] font-black text-white"
                                  style={{ background: accent }}
                                >
                                  {group.count}
                                </span>
                                <ChevronDown
                                  className={`h-4 w-4 transition ${isActive ? "rotate-180" : ""}`}
                                  style={{ color: accent }}
                                />
                              </div>
                            </button>
                          );
                        })}
                      </div>

                      <div className="rounded-[24px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-5">
                        {selectedInfrastructureCategory ? (
                          <div className="grid gap-3">
                            <div>
                              <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                                Объекты категории
                              </div>
                              <h3 className="mt-1 text-[1.45rem] font-black tracking-[-0.04em] text-[var(--foreground)]">
                                {selectedInfrastructureCategory.category}
                              </h3>
                            </div>
                            {selectedInfrastructureCategory.subcategories.length > 1 ? (
                              <div className="flex flex-wrap gap-2">
                                {selectedInfrastructureCategory.subcategories.map((subcategory) => {
                                  const isActive = selectedInfrastructureSubcategory?.key === subcategory.key;
                                  return (
                                    <button
                                      key={subcategory.key}
                                      type="button"
                                      onClick={() => setActiveInfrastructureSubcategory(subcategory.key)}
                                      className="rounded-full border px-3 py-1.5 text-sm font-semibold transition"
                                      style={{
                                        borderColor: isActive ? "var(--color-primary)" : "var(--border-color)",
                                        background: isActive
                                          ? "color-mix(in srgb, var(--color-primary-soft) 58%, var(--background-elevated))"
                                          : "var(--background-elevated)",
                                        color: "var(--foreground)",
                                      }}
                                    >
                                      {subcategory.label} · {subcategory.count}
                                    </button>
                                  );
                                })}
                              </div>
                            ) : null}
                            <div
                              className="grid gap-2"
                              style={{
                                maxHeight: "560px",
                                overflowY: "auto",
                                paddingRight: 4,
                              }}
                            >
                              {(selectedInfrastructureSubcategory?.items ?? []).map((item) => (
                                <div key={item.key} className={infoCardClass}>
                                  <div className="flex flex-wrap items-start justify-between gap-3">
                                    <div>
                                      <div className="font-semibold text-[var(--foreground)]">
                                        {item.title}
                                      </div>
                                      {item.subtitle ? (
                                        <div className="mt-1 text-sm text-[var(--foreground-muted)]">
                                          {item.subtitle}
                                        </div>
                                      ) : null}
                                    </div>
                                    <div className="rounded-full bg-[var(--background-muted)] px-3 py-1 text-[12px] font-bold text-[var(--foreground)]">
                                      {item.distanceMeters.toFixed(0)} м
                                    </div>
                                  </div>
                                  {item.addressText ? (
                                    <div className="mt-2 text-sm text-[var(--foreground-muted)]">
                                      {item.addressText}
                                    </div>
                                  ) : null}
                                </div>
                              ))}
                            </div>
                          </div>
                        ) : (
                          <div className="text-sm text-[var(--foreground-muted)]">
                            Для этой локации нет распределённых по категориям объектов.
                          </div>
                        )}
                      </div>
                    </div>
                  ) : null}

                  <div className="listing-details-infra-grid grid gap-3 xl:grid-cols-[minmax(0,1.2fr)_minmax(320px,0.8fr)]">
                    <div className="grid gap-3">
                      <div className="grid gap-2 sm:grid-cols-2">
                        {infrastructureFacts.map((item) => (
                          <div key={item.key} className={infoCardClass}>
                            <div className="flex items-center gap-3">
                              <span className="flex h-11 w-11 items-center justify-center rounded-[14px] bg-[var(--background-muted)]">
                                <img
                                  src={item.iconSrc}
                                  alt=""
                                  className="h-8 w-8"
                                  aria-hidden="true"
                                />
                              </span>
                              <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                                {item.label}
                              </div>
                            </div>
                            {item.entityName ? (
                              <div className="mt-2 text-sm font-semibold text-[var(--foreground)]">
                                {item.entityName}
                              </div>
                            ) : null}
                            <div className="mt-3 grid gap-2">
                              <div className="flex items-center gap-2 text-sm text-[var(--foreground)]">
                                <span className="text-[var(--foreground-muted)]">
                                  <WalkIcon />
                                </span>
                                <span className="font-semibold">
                                  {item.primaryWalkMinutes != null
                                    ? formatMinutesLabel(item.primaryWalkMinutes)
                                    : item.fallbackWalkMinutes != null
                                      ? formatMinutesLabel(item.fallbackWalkMinutes)
                                      : "не найдено"}
                                </span>
                              </div>
                              <div className="flex items-center gap-2 text-sm text-[var(--foreground)]">
                                <span className="text-[var(--foreground-muted)]">
                                  <CarIcon />
                                </span>
                                <span className="font-semibold">
                                  {item.primaryDriveMinutes != null
                                    ? formatMinutesLabel(item.primaryDriveMinutes, item.primaryDriveMinutesEstimated)
                                    : item.fallbackDriveMinutes != null
                                      ? formatMinutesLabel(item.fallbackDriveMinutes, item.fallbackDriveMinutesEstimated)
                                      : "не найдено"}
                                </span>
                              </div>
                            </div>
                            {item.hint ? (
                              <div className="mt-2 text-sm text-[var(--foreground-muted)]">
                                {item.hint}
                              </div>
                            ) : null}
                          </div>
                        ))}
                      </div>
                    </div>

                    <div className="grid gap-3">
                      {groupedTransportSummary ? (
                        <div className={infoCardClass}>
                          <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                            Транспорт рядом
                          </div>
                          <div className="mt-3 grid gap-2 text-sm text-[var(--foreground)]">
                            <div>
                              {groupedTransportSummary.nearestStopGroupName
                                ? `Ближайшая остановка «${groupedTransportSummary.nearestStopGroupName}»`
                                : "Ближайшая остановка не определена"}
                              {groupedTransportSummary.nearestStopGroupWalkMinutes != null
                                ? ` — ${formatMinutesLabel(groupedTransportSummary.nearestStopGroupWalkMinutes)} пешком`
                                : groupedTransportSummary.nearestStopGroupAnyDriveMinutes != null
                                  ? ` — ${formatMinutesLabel(groupedTransportSummary.nearestStopGroupAnyDriveMinutes, groupedTransportSummary.nearestStopGroupAnyDriveMinutesEstimated)} на машине`
                                  : ""}
                            </div>
                            <div>Остановочных групп рядом: {groupedTransportSummary.stopGroupsCountPrimary}</div>
                            {groupedTransportSummary.nearestStopGroupPlatformCount ? (
                              <div>Платформ в ближайшей группе: {groupedTransportSummary.nearestStopGroupPlatformCount}</div>
                            ) : null}
                            <div>
                              {groupedTransportSummary.metroFoundInPrimaryRadius && groupedTransportSummary.nearestMetroWalkMinutes != null
                                ? `Метро — ${formatMinutesLabel(groupedTransportSummary.nearestMetroWalkMinutes)} пешком`
                                : groupedTransportSummary.nearestMetroWalkMinutes != null
                                  ? `Ближайшее метро вне основной зоны — ${formatMinutesLabel(groupedTransportSummary.nearestMetroWalkMinutes)} пешком`
                                  : "Станции метро в пределах заданного радиуса не обнаружены."}
                            </div>
                            <div>
                              {groupedTransportSummary.parkingFoundInPrimaryRadius && groupedTransportSummary.nearestParkingWalkMinutes != null
                                ? `Парковка — ${formatMinutesLabel(groupedTransportSummary.nearestParkingWalkMinutes)} пешком`
                                : groupedTransportSummary.nearestParkingAnyDriveMinutes != null
                                  ? `Ближайшая парковка — ${formatMinutesLabel(groupedTransportSummary.nearestParkingAnyDriveMinutes, groupedTransportSummary.nearestParkingAnyDriveMinutesEstimated)} на машине`
                                  : "Парковка в основной зоне доступности не найдена"}
                            </div>
                          </div>
                        </div>
                      ) : null}
                      {infrastructure.highlights.length > 0 ? (
                        <div className="grid gap-2">
                          {infrastructure.highlights.map((item) => (
                            <div key={item} className={infoCardClass}>
                              {item}
                            </div>
                          ))}
                        </div>
                      ) : null}
                      {infrastructure.missingPrimaryCategories.length > 0 ? (
                        <div className={infoCardClass}>
                          <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                            Не найдено в основной зоне
                          </div>
                          <div className="mt-1 text-sm font-semibold text-[var(--foreground)]">
                            {infrastructure.missingPrimaryCategories.map(getPrimaryCategoryLabel).join(", ")}
                          </div>
                        </div>
                      ) : null}
                    </div>
                  </div>
                </div>
              </div>
            </section>
          </section>
        ) : null}

        {listing.latitude != null && listing.longitude != null ? (
          <section className="surface rounded-[32px] p-6 sm:p-8">
            <div className="grid gap-4 xl:min-h-[680px] xl:grid-rows-[auto_minmax(0,1fr)]">
              <div>
                <div className="eyebrow">Локация</div>
                <h2 className="section-title mt-2">Карта и окружение</h2>
                <p style={{ margin: "0.5rem 0 0", color: "var(--foreground-muted)", lineHeight: 1.6 }}>
                  {listing.addressText ?? "Точная локация уточняется"}
                </p>
              </div>
              <ListingLocationMap
                title={listing.title}
                addressText={listing.addressText}
                latitude={listing.latitude}
                longitude={listing.longitude}
                nearbyPois={infrastructure?.rawMapPois ?? infrastructure?.nearbyPois ?? []}
              />
            </div>
          </section>
        ) : null}

      </main>
    </AppShell>
  );
}
