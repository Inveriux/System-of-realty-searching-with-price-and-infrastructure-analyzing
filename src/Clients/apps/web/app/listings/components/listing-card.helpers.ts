import type {
  DealType,
  ListingCardItem,
  ListingPrimarySpec,
  ListingSecondaryFeature,
  PriceCurrency,
  PricePeriod,
} from "./listing-card.types";
import { translateLookup } from "../../lib/lookup-translations";

const currencyMap: Record<string, string> = {
  RUB: "₽",
  RUR: "₽",
  USD: "$",
  EUR: "евро",
  BYN: "BYN",
};

const periodMap: Record<string, string> = {
  Monthly: "мес",
  Daily: "сут",
  Weekly: "нед",
  Yearly: "год",
  OneTime: "",
  Месяц: "мес",
  Сутки: "сут",
  Неделя: "нед",
  Год: "год",
};

function prefixedLabel(prefix: string, value?: string | null) {
  if (!value) {
    return null;
  }

  return `${prefix}: ${value}`;
}

function formatMoneyValue(value: number) {
  return new Intl.NumberFormat("ru-RU", {
    minimumFractionDigits: 0,
    maximumFractionDigits: Number.isInteger(value) ? 0 : 2,
  }).format(value);
}

export function formatPrice(
  amount?: number,
  currency: PriceCurrency = "RUB",
  period?: PricePeriod,
) {
  if (!amount) {
    return "Цена по запросу";
  }

  const value = formatMoneyValue(amount);
  const currencyLabel = currencyMap[currency] ?? currency;
  const periodLabel = period ? periodMap[period] ?? String(period).toLowerCase() : null;

  return `${value} ${currencyLabel}${periodLabel ? `/${periodLabel}` : ""}`.trim();
}

export function formatNumericPrice(amount?: number, period?: PricePeriod) {
  if (!amount) {
    return "Цена по запросу";
  }

  const value = formatMoneyValue(amount);
  const periodLabel = period ? periodMap[period] ?? String(period).toLowerCase() : null;

  return `${value}${periodLabel ? `/${periodLabel}` : ""}`.trim();
}

export function formatNumericAmount(amount?: number) {
  if (!amount) {
    return null;
  }

  return formatMoneyValue(amount);
}

export function formatPricePeriodSuffix(period?: PricePeriod) {
  if (!period) {
    return null;
  }

  return periodMap[period] ?? String(period).toLowerCase();
}

export function formatCurrencyValue(amount?: number, currency: PriceCurrency = "RUB") {
  if (!amount) {
    return null;
  }

  const value = formatMoneyValue(amount);
  const currencyLabel = currencyMap[currency] ?? currency;

  return `${value} ${currencyLabel}`;
}

export function formatPricePerSquareMeter(
  amount?: number,
  area?: number,
  currency: PriceCurrency = "RUB",
) {
  if (!amount || !area) {
    return null;
  }

  const pricePerSquareMeter = amount / area;

  return `${new Intl.NumberFormat("ru-RU", {
    maximumFractionDigits: 0,
  }).format(pricePerSquareMeter)} ${currencyMap[currency] ?? currency}/м²`;
}

export function formatNumericPricePerSquareMeter(amount?: number, area?: number) {
  if (!amount || !area) {
    return null;
  }

  return new Intl.NumberFormat("ru-RU", {
    maximumFractionDigits: 0,
  }).format(amount / area);
}

export function formatArea(value?: number, unit = "м²") {
  if (!value) {
    return "—";
  }

  return `${new Intl.NumberFormat("ru-RU", {
    maximumFractionDigits: value % 1 === 0 ? 0 : 1,
  }).format(value)} ${unit}`;
}

export function formatInfrastructureScore(value?: number | null) {
  if (value == null) {
    return null;
  }

  return new Intl.NumberFormat("ru-RU", {
    maximumFractionDigits: value % 1 === 0 ? 0 : 1,
  }).format(value);
}

export function formatMeters(value?: number | null) {
  if (value == null) {
    return null;
  }

  return `${new Intl.NumberFormat("ru-RU", {
    maximumFractionDigits: 0,
  }).format(value)} м`;
}

export function formatFloor(floor?: number, total?: number) {
  if (!floor && !total) {
    return "—";
  }

  if (floor && total) {
    return `${floor}/${total}`;
  }

  return floor ? `${floor}` : `из ${total}`;
}

export function formatBooleanFeature(
  value: boolean | undefined,
  positiveLabel: string,
  negativeLabel?: string,
) {
  if (value === undefined) {
    return null;
  }

  if (value) {
    return positiveLabel;
  }

  return negativeLabel ?? null;
}

export function getDealBadgeTone(type: DealType) {
  return type === "Продажа"
    ? "border border-[color:color-mix(in_srgb,var(--foreground)_10%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--background-elevated)_92%,var(--background-muted))] text-[color:var(--foreground)]"
    : "border border-[color:color-mix(in_srgb,#0f766e_22%,var(--border-color))] bg-[color:color-mix(in_srgb,#ccfbf1_72%,var(--background-elevated))] text-[#115e59]";
}

export function getPropertyBadgeTone(item: ListingCardItem) {
  switch (item.details.kind) {
    case "apartment":
      return "border border-[color:color-mix(in_srgb,#2563eb_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#dbeafe_82%,var(--background-elevated))] text-[#1d4ed8]";
    case "house":
      return "border border-[color:color-mix(in_srgb,#b45309_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#fef3c7_84%,var(--background-elevated))] text-[#b45309]";
    case "land":
      return "border border-[color:color-mix(in_srgb,#15803d_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#dcfce7_82%,var(--background-elevated))] text-[#166534]";
    case "office":
      return "border border-[color:color-mix(in_srgb,var(--color-primary)_18%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_24%,var(--background-elevated))] text-[color:var(--color-primary-strong)]";
    case "warehouse":
      return "border border-[color:color-mix(in_srgb,#475569_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#e2e8f0_82%,var(--background-elevated))] text-[#334155]";
  }
}

export function getPrimarySpecs(item: ListingCardItem): ListingPrimarySpec[] {
  const specs = (() => {
    switch (item.details.kind) {
      case "apartment":
        return [
          { label: "Площадь", value: formatArea(item.areaTotal) },
          { label: "Тип", value: getBuildingTypeLabel(item.buildingType) ?? "—" },
          { label: "Комнаты", value: item.details.rooms === 0 ? "Студия" : item.details.rooms ? `${item.details.rooms}` : "—" },
          { label: "Спальни", value: item.details.bedrooms ? `${item.details.bedrooms}` : "—" },
          { label: "Этаж", value: formatFloor(item.details.floor, item.details.floorsTotal) },
          { label: "Жилая", value: formatArea(item.details.areaLiving) },
          { label: "Кухня", value: formatArea(item.details.areaKitchen) },
          { label: "Потолки", value: item.details.ceilingHeight ? `${item.details.ceilingHeight} м` : "—" },
        ];
      case "house":
        return [
          { label: "Дом", value: formatArea(item.details.houseArea ?? item.areaTotal) },
          { label: "Участок", value: formatArea(item.details.plotArea) },
          { label: "Тип", value: getBuildingTypeLabel(item.buildingType) ?? "—" },
          { label: "Комнаты", value: item.details.rooms ? `${item.details.rooms}` : "—" },
          { label: "Спальни", value: item.details.bedrooms ? `${item.details.bedrooms}` : "—" },
          { label: "Этажей", value: item.details.floors ? `${item.details.floors}` : "—" },
        ];
      case "land":
        return [
          { label: "Участок", value: formatArea(item.details.plotArea ?? item.areaTotal) },
          { label: "Тип", value: getBuildingTypeLabel(item.buildingType) ?? "—" },
          { label: "Категория", value: item.details.landCategory ? translateLookup("landCategory", item.details.landCategory) : "—" },
          { label: "Назначение", value: item.details.allowedUse ?? "—" },
          { label: "Подъезд", value: item.details.roadAccessType ? translateLookup("roadAccessType", item.details.roadAccessType) : "—" },
        ];
      case "office":
        return [
          { label: "Площадь", value: formatArea(item.areaTotal) },
          { label: "Тип", value: getBuildingTypeLabel(item.buildingType) ?? "—" },
          { label: "Этаж", value: formatFloor(item.details.floor, item.details.floorsTotal) },
          { label: "Уровней", value: item.details.levels ? `${item.details.levels}` : "—" },
          { label: "Класс", value: item.details.officeClass ?? "—" },
          { label: "Парковка", value: item.details.parkingSpaces ? `${item.details.parkingSpaces} м.` : item.details.parkingType ?? "—" },
        ];
      case "warehouse":
        return [
          { label: "Площадь", value: formatArea(item.areaTotal) },
          { label: "Участок", value: formatArea(item.details.plotArea) },
          { label: "Тип", value: getBuildingTypeLabel(item.buildingType) ?? "—" },
          { label: "Потолки", value: item.details.ceilingHeight ? `${item.details.ceilingHeight} м` : "—" },
          { label: "Класс", value: item.details.classType ?? "—" },
          { label: "Ворота", value: item.details.gateCount ? `${item.details.gateCount}` : "—" },
          { label: "Нагрузка", value: item.details.floorLoadTonsPerSquareMeter ? `${item.details.floorLoadTonsPerSquareMeter} т/м²` : "—" },
        ];
    }
  })();

  return specs.filter((spec) => spec.value !== "—");
}

export function getSecondaryFeatures(item: ListingCardItem): ListingSecondaryFeature[] {
  let labels: Array<string | null> = [];

  switch (item.details.kind) {
    case "apartment":
      labels = [
        prefixedLabel(
          "Ремонт",
          item.details.renovationType ? translateLookup("renovationType", item.details.renovationType) : item.details.renovationType,
        ),
        prefixedLabel(
          "Санузел",
          item.details.bathroomType ? translateLookup("bathroomType", item.details.bathroomType) : item.details.bathroomType,
        ),
        formatBooleanFeature(item.details.hasBalcony, "Балкон"),
        formatBooleanFeature(item.details.hasLoggia, "Лоджия"),
        formatBooleanFeature(item.details.hasElevator, "Лифт"),
      ];
      break;
    case "house":
      labels = [
        formatBooleanFeature(item.details.hasGarage, "Гараж", "Без гаража"),
        formatBooleanFeature(item.details.hasBasement, "Подвал"),
        formatBooleanFeature(item.details.hasFence, "Огорожен"),
        prefixedLabel(
          "Отопление",
          item.details.heatingType ? translateLookup("heatingType", item.details.heatingType) : item.details.heatingType,
        ),
        prefixedLabel(
          "Вода",
          item.details.waterSupplyType ? translateLookup("waterSupplyType", item.details.waterSupplyType) : item.details.waterSupplyType,
        ),
        prefixedLabel(
          "Газ",
          item.details.gasType ? translateLookup("gasType", item.details.gasType) : item.details.gasType,
        ),
        prefixedLabel(
          "Канализация",
          item.details.sewerType ? translateLookup("sewerType", item.details.sewerType) : item.details.sewerType,
        ),
        formatBooleanFeature(item.details.hasElectricity, "Электричество"),
        prefixedLabel("Крыша", item.details.roofMaterial),
      ];
      break;
    case "land":
      return [
        formatBooleanFeature(item.details.hasElectricity, "Электричество"),
        formatBooleanFeature(item.details.hasGas, "Газ"),
        formatBooleanFeature(item.details.hasWater, "Вода"),
        formatBooleanFeature(item.details.hasSewer, "Канализация"),
        formatBooleanFeature(item.details.hasBuildings, "Есть постройки"),
      ]
        .filter(Boolean)
        .slice(0, 6)
        .map((label, index) => ({ label: String(label), tone: index < 2 ? "accent" : "neutral" }));
    case "office":
      labels = [
        prefixedLabel(
          "Планировка",
          item.details.layoutType ? translateLookup("layoutType", item.details.layoutType) : item.details.layoutType,
        ),
        prefixedLabel(
          "Ремонт",
          item.details.renovationType ? translateLookup("renovationType", item.details.renovationType) : item.details.renovationType,
        ),
        formatBooleanFeature(item.details.kitchenette, "Мини-кухня"),
        prefixedLabel("Парковка", item.details.parkingType ? translateLookup("layoutType", item.details.parkingType) : item.details.parkingType),
        item.details.parkingSpaces ? `Мест: ${item.details.parkingSpaces}` : null,
      ];
      break;
    case "warehouse":
      labels = [
        item.details.dockCount ? `Доков: ${item.details.dockCount}` : null,
        formatBooleanFeature(item.details.hasRamps, "Рампы"),
        formatBooleanFeature(item.details.hasDockLevelers, "Доклевеллеры"),
        item.details.powerKw ? `Мощность: ${item.details.powerKw} кВт` : null,
        formatBooleanFeature(item.details.hasOffice, "Офисная зона"),
        item.details.officeArea ? `Офис: ${formatArea(item.details.officeArea)}` : null,
        prefixedLabel(
          "Отопление",
          item.details.heatingType ? translateLookup("heatingType", item.details.heatingType) : item.details.heatingType,
        ),
      ];
      break;
  }

  return labels
    .filter(Boolean)
    .slice(0, 6)
    .map((label) => ({ label: String(label) }));
}

export function getTypeLabel(item: ListingCardItem) {
  switch (item.details.kind) {
    case "apartment":
      return "Квартира";
    case "house":
      return "Дом";
    case "land":
      return "Участок";
    case "office":
      return "Офис";
    case "warehouse":
      return "Склад";
  }
}

export function getMaterialLabel(value?: string) {
  if (!value) {
    return null;
  }

  const buildingMaterial = translateLookup("buildingMaterial", value);
  if (buildingMaterial !== value) {
    return buildingMaterial;
  }

  return translateLookup("wallMaterial", value);
}

export function getBuildingTypeLabel(value?: string | null) {
  if (!value) {
    return "Недвижимость";
  }

  return translateLookup("buildingType", value);
}
