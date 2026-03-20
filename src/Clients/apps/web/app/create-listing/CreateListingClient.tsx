"use client";

import {
  createListingWithImages,
  getCreateListingLookups,
  type LookupOption,
} from "@diploma/api/listings";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ChangeEvent,
  FormEvent,
  useEffect,
  useEffectEvent,
  useMemo,
  useRef,
  useState,
} from "react";
import { useRouter } from "next/navigation";
import { AppShell } from "../components/AppShell";
import { useAuth } from "../providers";

type CreateListingState = {
  title: string;
  addressText: string;
  latitude: string;
  longitude: string;
  description: string;
  priceAmount: string;
  areaTotal: string;
  buildYear: string;
  buildingType: string;
  listingType: string;
  listingDetailsType: "Apartment" | "House" | "Land" | "Office" | "Warehouse";
  rooms: string;
  bedrooms: string;
  floorsTotal: string;
  levels: string;
  floor: string;
  areaLiving: string;
  areaKitchen: string;
  ceilingHeight: string;
  bathroomType: string;
  renovationType: string;
  buildingMaterial: string;
  hasBalcony: boolean;
  hasLoggia: boolean;
  hasElevator: boolean;
  houseFloors: string;
  houseArea: string;
  plotArea: string;
  hasElectricity: boolean;
  hasGarage: boolean;
  hasBasement: boolean;
  hasFence: boolean;
  wallMaterial: string;
  heatingType: string;
  sewerType: string;
  gasType: string;
  roofMaterial: string;
  waterSupplyType: string;
  allowedUse: string;
  hasGas: boolean;
  hasWater: boolean;
  hasSewer: boolean;
  hasBuildings: boolean;
  landCategory: string;
  roadAccessType: string;
  officeClass: string;
  layoutType: string;
  officeRenovationType: string;
  parkingType: string;
  parkingSpaces: string;
  kitchenette: boolean;
  warehouseCeilingHeight: string;
  floorLoadTonsPerSquareMeter: string;
  gateCount: string;
  hasDockLevelers: boolean;
  dockCount: string;
  hasRamps: boolean;
  powerKw: string;
  hasOffice: boolean;
  officeArea: string;
  classType: string;
  warehouseHeatingType: string;
};

type DraftLocation = {
  addressText: string;
  latitude: number;
  longitude: number;
};

type ListingDetailsType = CreateListingState["listingDetailsType"];

const YANDEX_MAPS_API_KEY =
  process.env.NEXT_PUBLIC_YANDEX_MAPS_API_KEY ??
  "3225bb0b-e2a7-455f-9c95-ce49d86acbcb";

const MAP_DEFAULT_CENTER: [number, number] = [53.902284, 27.561831];

const initialState: CreateListingState = {
  title: "",
  addressText: "",
  latitude: "",
  longitude: "",
  description: "",
  priceAmount: "",
  areaTotal: "",
  buildYear: "",
  buildingType: "",
  listingType: "",
  listingDetailsType: "Apartment",
  rooms: "",
  bedrooms: "",
  floorsTotal: "",
  levels: "",
  floor: "",
  areaLiving: "",
  areaKitchen: "",
  ceilingHeight: "",
  bathroomType: "",
  renovationType: "",
  buildingMaterial: "",
  hasBalcony: true,
  hasLoggia: false,
  hasElevator: true,
  houseFloors: "",
  houseArea: "",
  plotArea: "",
  hasElectricity: true,
  hasGarage: false,
  hasBasement: false,
  hasFence: false,
  wallMaterial: "",
  heatingType: "",
  sewerType: "",
  gasType: "",
  roofMaterial: "",
  waterSupplyType: "",
  allowedUse: "",
  hasGas: false,
  hasWater: false,
  hasSewer: false,
  hasBuildings: false,
  landCategory: "",
  roadAccessType: "",
  officeClass: "",
  layoutType: "",
  officeRenovationType: "",
  parkingType: "",
  parkingSpaces: "",
  kitchenette: false,
  warehouseCeilingHeight: "",
  floorLoadTonsPerSquareMeter: "",
  gateCount: "",
  hasDockLevelers: false,
  dockCount: "",
  hasRamps: false,
  powerKw: "",
  hasOffice: false,
  officeArea: "",
  classType: "",
  warehouseHeatingType: "",
};

const lookupTranslations: Record<string, Record<string, string>> = {
  listingType: {
    Rent: "Аренда",
    Sale: "Продажа",
  },
  buildingType: {
    "Apartment Building (Multi-family)": "Многоквартирный дом",
    "Detached House": "Отдельный дом",
    Townhouse: "Таунхаус",
    "Business Center": "Бизнес-центр",
    "Warehouse Complex": "Складской комплекс",
    "Industrial Building": "Промышленное здание",
    "Shopping Mall": "Торговый центр",
    "Mixed-use Building": "Многофункциональное здание",
    Other: "Другое",
  },
  bathroomType: {
    "Combined (shared)": "Совмещенный",
    Separate: "Раздельный",
    "Two bathrooms": "Два санузла",
    "Three or more bathrooms": "Три и более санузла",
  },
  renovationType: {
    Designer: "Дизайнерский",
    Modern: "Современный",
    Cosmetic: "Косметический",
    None: "Без ремонта",
    Euro: "Евроремонт",
  },
  buildingMaterial: {
    Brick: "Кирпич",
    Panel: "Панель",
    Monolith: "Монолит",
    Block: "Блок",
    Wood: "Дерево",
  },
  wallMaterial: {
    Brick: "Кирпич",
    Panel: "Панель",
    Monolith: "Монолит",
    Block: "Блок",
    Wood: "Дерево",
  },
  heatingType: {
    Central: "Центральное",
    "Gas boiler": "Газовый котел",
    "Individual gas boiler": "Индивидуальный газовый котел",
    Electric: "Электрическое",
    "Solid fuel": "Твердотопливное",
    None: "Нет",
  },
  waterSupplyType: {
    Central: "Центральное",
    Well: "Колодец",
    Borehole: "Скважина",
    "Delivered / tank": "Привозная / емкость",
    None: "Нет",
  },
  sewerType: {
    Central: "Центральная",
    "Septic tank": "Септик",
    Cesspool: "Выгребная яма",
    None: "Нет",
  },
  gasType: {
    "Central gas": "Центральный газ",
    "Gas holder / tank": "Газгольдер / резервуар",
    "Bottled gas": "Баллонный газ",
    None: "Нет",
  },
  landCategory: {
    "Residential (IЖС)": "Жилая застройка (ИЖС)",
    "Personal subsidiary plot (ЛПХ)": "Личное подсобное хозяйство (ЛПХ)",
    "Garden / dacha (СНТ)": "Садовый / дачный участок (СНТ)",
    Agricultural: "Сельскохозяйственная",
    "Industrial / commercial": "Промышленная / коммерческая",
    "Forestry / protected": "Лесной фонд / охраняемая",
    Other: "Другое",
  },
  roadAccessType: {
    Asphalt: "Асфальт",
    Gravel: "Гравий",
    "Dirt road": "Грунтовая дорога",
    "No direct access / off-road": "Нет прямого подъезда / бездорожье",
  },
  classType: {
    A: "Класс A",
    B: "Класс B",
    C: "Класс C",
    D: "Класс D",
  },
  layoutType: {
    "Open space": "Открытая планировка",
    "Cabinet / rooms": "Кабинетная",
    Mixed: "Смешанная",
    Coworking: "Коворкинг",
  },
  officeClass: {
    A: "Класс A",
    "B+": "Класс B+",
    B: "Класс B",
    C: "Класс C",
  },
};

declare global {
  interface Window {
    ymaps?: YandexMapsApi;
    __diplomaYandexMapsPromise?: Promise<YandexMapsApi>;
  }
}

type YandexMapsApi = {
  ready(callback: () => void): void;
  Map: new (container: HTMLElement, state: unknown, options?: unknown) => YandexMapInstance;
  Placemark: new (geometry: [number, number], properties?: unknown, options?: unknown) => YandexPlacemark;
  geocode(request: string | number[]): Promise<YandexGeocodeResult>;
};

type YandexMapInstance = {
  destroy(): void;
  setCenter(center: [number, number], zoom?: number, options?: unknown): void;
  geoObjects: {
    add(object: unknown): void;
    remove(object: unknown): void;
  };
  events: {
    add(eventName: string, handler: (event: { get(name: string): [number, number] }) => void): void;
  };
};

type YandexPlacemark = {
  geometry: {
    setCoordinates(coords: [number, number]): void;
  };
};

type YandexGeocodeResult = {
  geoObjects: {
    get(index: number): {
      getAddressLine?: () => string;
      properties: {
        get(name: string): string | undefined;
      };
    } | null;
  };
};

function toNumber(value: string) {
  return value ? Number(value) : null;
}

function toInteger(value: string) {
  return value ? Number.parseInt(value, 10) : null;
}

function translateLookup(kind: keyof typeof lookupTranslations, value: string) {
  return lookupTranslations[kind][value] ?? value;
}

function getListingDetailsTypeLabel(value: ListingDetailsType) {
  switch (value) {
    case "Apartment":
      return "Квартира";
    case "House":
      return "Дом";
    case "Land":
      return "Участок";
    case "Office":
      return "Офис";
    case "Warehouse":
      return "Склад";
    default:
      return value;
  }
}

function getListingSectionLabel(value: ListingDetailsType) {
  switch (value) {
    case "Apartment":
      return "Параметры квартиры";
    case "House":
      return "Параметры дома";
    case "Land":
      return "Параметры участка";
    case "Office":
      return "Параметры офиса";
    case "Warehouse":
      return "Параметры склада";
    default:
      return value;
  }
}

function loadYandexMaps() {
  if (typeof window === "undefined") {
    return Promise.reject(new Error("Yandex Maps доступны только в браузере."));
  }

  if (window.ymaps) {
    return Promise.resolve(window.ymaps);
  }

  if (window.__diplomaYandexMapsPromise) {
    return window.__diplomaYandexMapsPromise;
  }

  window.__diplomaYandexMapsPromise = new Promise<YandexMapsApi>((resolve, reject) => {
    const script = document.createElement("script");
    script.src = `https://api-maps.yandex.ru/2.1/?apikey=${encodeURIComponent(
      YANDEX_MAPS_API_KEY,
    )}&lang=ru_RU`;
    script.async = true;
    script.onload = () => {
      if (window.ymaps) {
        resolve(window.ymaps);
        return;
      }

      reject(new Error("Yandex Maps API не инициализировался."));
    };
    script.onerror = () => reject(new Error("Не удалось загрузить Yandex Maps API."));
    document.head.appendChild(script);
  });

  return window.__diplomaYandexMapsPromise;
}

export function CreateListingClient() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { api, status } = useAuth();
  const [values, setValues] = useState(initialState);
  const [files, setFiles] = useState<File[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isMapOpen, setIsMapOpen] = useState(false);
  const [mapError, setMapError] = useState<string | null>(null);
  const [isResolvingAddress, setIsResolvingAddress] = useState(false);
  const [draftLocation, setDraftLocation] = useState<DraftLocation | null>(null);

  const mapContainerRef = useRef<HTMLDivElement | null>(null);
  const mapRef = useRef<YandexMapInstance | null>(null);
  const placemarkRef = useRef<YandexPlacemark | null>(null);

  const lookupsQuery = useQuery({
    queryKey: ["listing-create-lookups"],
    queryFn: () => getCreateListingLookups(api),
  });

  const filePreviews = useMemo(
    () =>
      files.map((file) => ({
        file,
        url: URL.createObjectURL(file),
      })),
    [files],
  );

  useEffect(() => {
    return () => {
      for (const preview of filePreviews) {
        URL.revokeObjectURL(preview.url);
      }
    };
  }, [filePreviews]);

  useEffect(() => {
    if (!lookupsQuery.data) {
      return;
    }

    setValues((current) => ({
      ...current,
      listingType: current.listingType || lookupsQuery.data.listingTypes[0]?.name || "",
      buildingType: current.buildingType || lookupsQuery.data.buildingTypes[0]?.name || "",
      bathroomType: current.bathroomType || lookupsQuery.data.bathroomTypes[0]?.name || "",
      renovationType: current.renovationType || lookupsQuery.data.renovationTypes[0]?.name || "",
      buildingMaterial:
        current.buildingMaterial || lookupsQuery.data.buildingMaterials[0]?.name || "",
      classType: current.classType || lookupsQuery.data.classTypes[0]?.name || "",
      gasType: current.gasType || lookupsQuery.data.gasTypes[0]?.name || "",
      heatingType: current.heatingType || lookupsQuery.data.heatingTypes[0]?.name || "",
      landCategory: current.landCategory || lookupsQuery.data.landCategories[0]?.name || "",
      layoutType: current.layoutType || lookupsQuery.data.layoutTypes[0]?.name || "",
      officeClass: current.officeClass || lookupsQuery.data.officeClasses[0]?.name || "",
      officeRenovationType:
        current.officeRenovationType || lookupsQuery.data.renovationTypes[0]?.name || "",
      parkingType: current.parkingType || lookupsQuery.data.layoutTypes[0]?.name || "",
      roadAccessType: current.roadAccessType || lookupsQuery.data.roadAccessTypes[0]?.name || "",
      sewerType: current.sewerType || lookupsQuery.data.sewerTypes[0]?.name || "",
      wallMaterial: current.wallMaterial || lookupsQuery.data.wallMaterials[0]?.name || "",
      roofMaterial: current.roofMaterial || lookupsQuery.data.wallMaterials[0]?.name || "",
      waterSupplyType:
        current.waterSupplyType || lookupsQuery.data.waterSupplyTypes[0]?.name || "",
      warehouseHeatingType:
        current.warehouseHeatingType || lookupsQuery.data.heatingTypes[0]?.name || "",
    }));
  }, [lookupsQuery.data]);

  const updateDraftLocation = useEffectEvent(async (ymaps: YandexMapsApi, coords: [number, number]) => {
    setIsResolvingAddress(true);
    setMapError(null);

    try {
      updatePlacemark(coords, ymaps);

      const result = await ymaps.geocode(coords);
      const firstGeoObject = result.geoObjects.get(0);
      const addressText =
        firstGeoObject?.getAddressLine?.() ??
        firstGeoObject?.properties.get("text") ??
        `${coords[0].toFixed(6)}, ${coords[1].toFixed(6)}`;

      setDraftLocation({
        addressText,
        latitude: coords[0],
        longitude: coords[1],
      });
    } catch {
      setMapError("Не удалось определить адрес по точке на карте.");
    } finally {
      setIsResolvingAddress(false);
    }
  });

  useEffect(() => {
    if (!isMapOpen || !mapContainerRef.current) {
      return;
    }

    let cancelled = false;

    async function initMap() {
      try {
        const ymaps = await loadYandexMaps();
        if (cancelled || !mapContainerRef.current) {
          return;
        }

        ymaps.ready(() => {
          if (cancelled || !mapContainerRef.current) {
            return;
          }

          if (!mapRef.current) {
            const center: [number, number] =
              draftLocation
                ? [draftLocation.latitude, draftLocation.longitude]
                : values.latitude && values.longitude
                  ? [Number(values.latitude), Number(values.longitude)]
                  : MAP_DEFAULT_CENTER;

            const map = new ymaps.Map(
              mapContainerRef.current,
              {
                center,
                zoom: 14,
                controls: ["zoomControl", "fullscreenControl"],
              },
              {
                suppressMapOpenBlock: true,
              },
            );

            map.events.add("click", async (event) => {
              const coords = event.get("coords");
              await updateDraftLocation(ymaps, coords);
            });

            mapRef.current = map;
          }

          if (draftLocation) {
            updatePlacemark([draftLocation.latitude, draftLocation.longitude], ymaps);
            mapRef.current.setCenter([draftLocation.latitude, draftLocation.longitude]);
          }
        });
      } catch (loadError) {
        setMapError(
          loadError instanceof Error
            ? loadError.message
            : "Не удалось открыть карту.",
        );
      }
    }

    void initMap();

    return () => {
      cancelled = true;
    };
  }, [draftLocation, isMapOpen, values.latitude, values.longitude]);

  useEffect(() => {
    return () => {
      mapRef.current?.destroy();
      mapRef.current = null;
      placemarkRef.current = null;
    };
  }, []);

  function updateField<K extends keyof CreateListingState>(key: K, value: CreateListingState[K]) {
    setValues((current) => ({ ...current, [key]: value }));
  }

  function handleFilesChange(event: ChangeEvent<HTMLInputElement>) {
    const nextFiles = Array.from(event.target.files ?? []);

    if (nextFiles.length === 0) {
      return;
    }

    setFiles((current) => {
      const seen = new Set(
        current.map((file) => `${file.name}:${file.size}:${file.lastModified}`),
      );
      const uniqueNext = nextFiles.filter((file) => {
        const key = `${file.name}:${file.size}:${file.lastModified}`;
        if (seen.has(key)) {
          return false;
        }

        seen.add(key);
        return true;
      });

      return [...current, ...uniqueNext];
    });

    event.target.value = "";
  }

  function removeFile(fileToRemove: File) {
    setFiles((current) =>
      current.filter(
        (file) =>
          !(
            file.name === fileToRemove.name &&
            file.size === fileToRemove.size &&
            file.lastModified === fileToRemove.lastModified
          ),
      ),
    );
  }

  function updatePlacemark(coords: [number, number], ymaps: YandexMapsApi) {
    if (!mapRef.current) {
      return;
    }

    if (!placemarkRef.current) {
      placemarkRef.current = new ymaps.Placemark(
        coords,
        {},
        {
          preset: "islands#redDotIcon",
        },
      );
      mapRef.current.geoObjects.add(placemarkRef.current);
      return;
    }

    placemarkRef.current.geometry.setCoordinates(coords);
  }

  function openMapPicker() {
    setMapError(null);
    setDraftLocation(
      values.latitude && values.longitude
        ? {
            addressText: values.addressText,
            latitude: Number(values.latitude),
            longitude: Number(values.longitude),
          }
        : null,
    );
    setIsMapOpen(true);
  }

  function closeMapPicker() {
    mapRef.current?.destroy();
    mapRef.current = null;
    placemarkRef.current = null;
    setIsResolvingAddress(false);
    setMapError(null);
    setIsMapOpen(false);
  }

  function confirmMapSelection() {
    if (!draftLocation) {
      setMapError("Сначала выберите точку на карте.");
      return;
    }

    setValues((current) => ({
      ...current,
      addressText: draftLocation.addressText,
      latitude: draftLocation.latitude.toFixed(6),
      longitude: draftLocation.longitude.toFixed(6),
    }));
    closeMapPicker();
  }

  function buildListingDetails() {
    switch (values.listingDetailsType) {
      case "House":
        return {
          rooms: toInteger(values.rooms),
          bedrooms: toInteger(values.bedrooms),
          floors: toInteger(values.houseFloors),
          houseArea: toNumber(values.houseArea),
          plotArea: toNumber(values.plotArea),
          hasElectricity: values.hasElectricity,
          hasGarage: values.hasGarage,
          hasBasement: values.hasBasement,
          hasFence: values.hasFence,
          wallMaterial: values.wallMaterial,
          heatingType: values.heatingType,
          sewerType: values.sewerType,
          gasType: values.gasType,
          roofMaterial: values.roofMaterial,
          waterSupplyType: values.waterSupplyType,
        };
      case "Land":
        return {
          plotArea: toNumber(values.plotArea),
          allowedUse: values.allowedUse,
          hasElectricity: values.hasElectricity,
          hasGas: values.hasGas,
          hasWater: values.hasWater,
          hasSewer: values.hasSewer,
          hasBuildings: values.hasBuildings,
          landCategory: values.landCategory,
          roadAccessType: values.roadAccessType,
        };
      case "Office":
        return {
          plotArea: toNumber(values.plotArea),
          floor: toInteger(values.floor),
          floorsTotal: toInteger(values.floorsTotal),
          levels: toInteger(values.levels),
          parkingSpaces: toInteger(values.parkingSpaces),
          kitchenette: values.kitchenette,
          officeClass: values.officeClass,
          layoutType: values.layoutType,
          renovationType: values.officeRenovationType,
          parkingType: values.parkingType,
        };
      case "Warehouse":
        return {
          plotArea: toNumber(values.plotArea),
          ceilingHeight: toNumber(values.warehouseCeilingHeight),
          floorLoadTonsPerSquareMeter: toNumber(values.floorLoadTonsPerSquareMeter),
          gateCount: toInteger(values.gateCount),
          hasDockLevelers: values.hasDockLevelers,
          dockCount: toInteger(values.dockCount),
          hasRamps: values.hasRamps,
          powerKw: toNumber(values.powerKw),
          hasOffice: values.hasOffice,
          officeArea: toNumber(values.officeArea),
          classType: values.classType,
          heatingType: values.warehouseHeatingType,
        };
      case "Apartment":
      default:
        return {
          rooms: toInteger(values.rooms),
          bedrooms: toInteger(values.bedrooms),
          floorsTotal: toInteger(values.floorsTotal),
          levels: toInteger(values.levels),
          floor: toInteger(values.floor),
          areaLiving: toNumber(values.areaLiving),
          areaKitchen: toNumber(values.areaKitchen),
          hasBalcony: values.hasBalcony,
          hasLoggia: values.hasLoggia,
          hasElevator: values.hasElevator,
          ceilingHeight: toNumber(values.ceilingHeight),
          bathroomType: values.bathroomType,
          renovationType: values.renovationType,
          buildingMaterial: values.buildingMaterial,
        };
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (status !== "authenticated") {
      setError("Для создания объявления нужно войти в аккаунт.");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    const payload = {
      agentId: crypto.randomUUID(),
      addressId: crypto.randomUUID(),
      addressText: values.addressText,
      latitude: toNumber(values.latitude),
      longitude: toNumber(values.longitude),
      title: values.title,
      description: values.description,
      priceAmount: Number(values.priceAmount),
      priceCurrency: "USD",
      pricePeriod: values.listingType === "Sale" ? "OneTime" : "Monthly",
      areaTotal: Number(values.areaTotal),
      buildYear: Number(values.buildYear),
      buildingType: values.buildingType,
      listingType: values.listingType,
      status: "Active",
      listingDetailsType: values.listingDetailsType,
      listingDetails: buildListingDetails(),
    };

    try {
      await createListingWithImages(api, payload, files);
      await queryClient.invalidateQueries({ queryKey: ["listings"] });
      router.push("/listings");
    } catch (submissionError) {
      setError(
        submissionError instanceof Error
          ? submissionError.message
          : "Не удалось создать объявление.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  function renderLookupSelect(
    label: string,
    value: string,
    onChange: (nextValue: string) => void,
    options: LookupOption[],
    kind?: keyof typeof lookupTranslations,
  ) {
    return (
      <label style={{ display: "grid", gap: 8 }}>
        <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>{label}</span>
        <select
          className="field"
          value={value}
          onChange={(event) => onChange(event.target.value)}
          required
        >
          <option value="" disabled>
            Выберите вариант
          </option>
          {options.map((option) => (
            <option key={option.name} value={option.name}>
              {kind ? translateLookup(kind, option.name) : option.name}
            </option>
          ))}
        </select>
      </label>
    );
  }

  return (
    <AppShell>
      <main style={{ display: "grid", gap: 20, paddingBottom: 36 }}>
        <section
          className="surface"
          style={{
            borderRadius: 28,
            padding: 28,
            background:
              "linear-gradient(135deg, color-mix(in srgb, var(--color-primary-soft) 80%, white), var(--background-elevated))",
          }}
        >
          <div className="eyebrow">Новое объявление</div>
          <h1 className="section-title" style={{ marginTop: 10 }}>
            Создание объявления
          </h1>
          <p style={{ color: "var(--foreground-muted)", maxWidth: 720, marginBottom: 0 }}>
            Укажите основные характеристики объекта и добавьте фотографии.
          </p>
        </section>

        <form
          className="listing-create-form"
          onSubmit={handleSubmit}
        >
          <div style={{ display: "grid", gap: 20 }}>
            <section className="surface" style={{ borderRadius: 28, padding: 24, display: "grid", gap: 22 }}>
            <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "start", flexWrap: "wrap" }}>
              <div>
                <div className="eyebrow">Основное</div>
                <h2 className="section-title" style={{ marginTop: 8 }}>Карточка объекта</h2>
              </div>
              <button className="button-secondary" type="button" onClick={openMapPicker}>
                Выбрать адрес на карте
              </button>
            </div>

            {lookupsQuery.isLoading ? (
              <div className="field">Загружаем справочники...</div>
            ) : null}

            {lookupsQuery.isError ? (
              <div className="listing-create-alert listing-create-alert-error">
                Не удалось загрузить справочники.
              </div>
            ) : null}

            <div className="listing-create-grid listing-create-grid-main">
              <label style={{ display: "grid", gap: 8, gridColumn: "1 / -1" }}>
                <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Заголовок</span>
                <input
                  className="field"
                  value={values.title}
                  onChange={(event) => updateField("title", event.target.value)}
                  required
                />
              </label>
            </div>

            <label style={{ display: "grid", gap: 8 }}>
              <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Описание</span>
              <textarea
                className="field"
                value={values.description}
                onChange={(event) => updateField("description", event.target.value)}
                rows={5}
              />
            </label>

            <div className="listing-create-grid listing-create-grid-main">
              <label style={{ display: "grid", gap: 8 }}>
                <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Цена, USD</span>
                <input
                  className="field"
                  value={values.priceAmount}
                  onChange={(event) => updateField("priceAmount", event.target.value)}
                  type="number"
                  min="1"
                  step="0.01"
                  required
                />
              </label>

              <label style={{ display: "grid", gap: 8 }}>
                <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Общая площадь, м²</span>
                <input
                  className="field"
                  value={values.areaTotal}
                  onChange={(event) => updateField("areaTotal", event.target.value)}
                  type="number"
                  min="0.1"
                  step="0.1"
                  required
                />
              </label>

              <label style={{ display: "grid", gap: 8 }}>
                <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Год постройки</span>
                <input
                  className="field"
                  value={values.buildYear}
                  onChange={(event) => updateField("buildYear", event.target.value)}
                  type="number"
                  min="1800"
                  max="2100"
                  required
                />
              </label>

              {lookupsQuery.data
                ? renderLookupSelect(
                    "Тип сделки",
                    values.listingType,
                    (nextValue) => updateField("listingType", nextValue),
                    lookupsQuery.data.listingTypes,
                    "listingType",
                  )
                : null}

              {lookupsQuery.data
                ? renderLookupSelect(
                    "Тип здания",
                    values.buildingType,
                    (nextValue) => updateField("buildingType", nextValue),
                    lookupsQuery.data.buildingTypes,
                    "buildingType",
                  )
                : null}
            </div>
            </section>

            <section className="surface" style={{ borderRadius: 28, padding: 24, display: "grid", gap: 16 }}>
              <div>
                <div className="eyebrow">Тип недвижимости</div>
                <h2 className="section-title" style={{ marginTop: 8, marginBottom: 8 }}>
                  Что вы размещаете
                </h2>
                <p style={{ color: "var(--foreground-muted)", margin: 0 }}>
                  Выберите тип объекта. Ниже отобразятся нужные для него характеристики.
                </p>
              </div>

              <label style={{ display: "grid", gap: 8, maxWidth: 420 }}>
                <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Тип недвижимости</span>
                <select
                  className="field"
                  value={values.listingDetailsType}
                  onChange={(event) =>
                    updateField("listingDetailsType", event.target.value as ListingDetailsType)
                  }
                  required
                >
                  <option value="Apartment">Квартира</option>
                  <option value="House">Дом</option>
                  <option value="Land">Участок</option>
                  <option value="Office">Офис</option>
                  <option value="Warehouse">Склад</option>
                </select>
              </label>
            </section>

            <section className="surface" style={{ borderRadius: 28, padding: 24, display: "grid", gap: 22 }}>

            {values.listingDetailsType === "Apartment" ? (
              <>
                <div>
                  <div className="eyebrow">Параметры объекта</div>
                  <h2 className="section-title" style={{ marginTop: 8, marginBottom: 16 }}>
                    Характеристики квартиры
                  </h2>
                  <div className="listing-create-grid listing-create-grid-details">
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Комнаты</span>
                      <input className="field" value={values.rooms} onChange={(event) => updateField("rooms", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Спальни</span>
                      <input className="field" value={values.bedrooms} onChange={(event) => updateField("bedrooms", event.target.value)} type="number" min="0" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Этаж</span>
                      <input className="field" value={values.floor} onChange={(event) => updateField("floor", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Этажей в доме</span>
                      <input className="field" value={values.floorsTotal} onChange={(event) => updateField("floorsTotal", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Уровней</span>
                      <input className="field" value={values.levels} onChange={(event) => updateField("levels", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Высота потолков, м</span>
                      <input className="field" value={values.ceilingHeight} onChange={(event) => updateField("ceilingHeight", event.target.value)} type="number" min="0.1" step="0.1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Жилая площадь, м²</span>
                      <input className="field" value={values.areaLiving} onChange={(event) => updateField("areaLiving", event.target.value)} type="number" min="0" step="0.1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Площадь кухни, м²</span>
                      <input className="field" value={values.areaKitchen} onChange={(event) => updateField("areaKitchen", event.target.value)} type="number" min="0" step="0.1" required />
                    </label>

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Санузел",
                          values.bathroomType,
                          (nextValue) => updateField("bathroomType", nextValue),
                          lookupsQuery.data.bathroomTypes,
                          "bathroomType",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Ремонт",
                          values.renovationType,
                          (nextValue) => updateField("renovationType", nextValue),
                          lookupsQuery.data.renovationTypes,
                          "renovationType",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Материал дома",
                          values.buildingMaterial,
                          (nextValue) => updateField("buildingMaterial", nextValue),
                          lookupsQuery.data.buildingMaterials,
                          "buildingMaterial",
                        )
                      : null}
                  </div>
                </div>

                <div className="listing-create-switches">
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasBalcony} onChange={(event) => updateField("hasBalcony", event.target.checked)} />
                    Балкон
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasLoggia} onChange={(event) => updateField("hasLoggia", event.target.checked)} />
                    Лоджия
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasElevator} onChange={(event) => updateField("hasElevator", event.target.checked)} />
                    Лифт
                  </label>
                </div>
              </>
            ) : null}

            {values.listingDetailsType === "House" ? (
              <>
                <div>
                  <div className="eyebrow">Параметры объекта</div>
                  <h2 className="section-title" style={{ marginTop: 8, marginBottom: 16 }}>
                    Характеристики дома
                  </h2>
                  <div className="listing-create-grid listing-create-grid-details">
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Комнаты</span>
                      <input className="field" value={values.rooms} onChange={(event) => updateField("rooms", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Спальни</span>
                      <input className="field" value={values.bedrooms} onChange={(event) => updateField("bedrooms", event.target.value)} type="number" min="0" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Этажей</span>
                      <input className="field" value={values.houseFloors} onChange={(event) => updateField("houseFloors", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Площадь дома, м²</span>
                      <input className="field" value={values.houseArea} onChange={(event) => updateField("houseArea", event.target.value)} type="number" min="0.1" step="0.1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Площадь участка, м²</span>
                      <input className="field" value={values.plotArea} onChange={(event) => updateField("plotArea", event.target.value)} type="number" min="0.1" step="0.1" required />
                    </label>

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Материал стен",
                          values.wallMaterial,
                          (nextValue) => updateField("wallMaterial", nextValue),
                          lookupsQuery.data.wallMaterials,
                          "wallMaterial",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Отопление",
                          values.heatingType,
                          (nextValue) => updateField("heatingType", nextValue),
                          lookupsQuery.data.heatingTypes,
                          "heatingType",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Канализация",
                          values.sewerType,
                          (nextValue) => updateField("sewerType", nextValue),
                          lookupsQuery.data.sewerTypes,
                          "sewerType",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Газ",
                          values.gasType,
                          (nextValue) => updateField("gasType", nextValue),
                          lookupsQuery.data.gasTypes,
                          "gasType",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Материал крыши",
                          values.roofMaterial,
                          (nextValue) => updateField("roofMaterial", nextValue),
                          lookupsQuery.data.wallMaterials,
                          "wallMaterial",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Водоснабжение",
                          values.waterSupplyType,
                          (nextValue) => updateField("waterSupplyType", nextValue),
                          lookupsQuery.data.waterSupplyTypes,
                          "waterSupplyType",
                        )
                      : null}
                  </div>
                </div>

                <div className="listing-create-switches">
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasElectricity} onChange={(event) => updateField("hasElectricity", event.target.checked)} />
                    Электричество
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasGarage} onChange={(event) => updateField("hasGarage", event.target.checked)} />
                    Гараж
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasBasement} onChange={(event) => updateField("hasBasement", event.target.checked)} />
                    Подвал
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasFence} onChange={(event) => updateField("hasFence", event.target.checked)} />
                    Огорожен
                  </label>
                </div>
              </>
            ) : null}

            {values.listingDetailsType === "Land" ? (
              <>
                <div>
                  <div className="eyebrow">Параметры объекта</div>
                  <h2 className="section-title" style={{ marginTop: 8, marginBottom: 16 }}>
                    Характеристики участка
                  </h2>
                  <div className="listing-create-grid listing-create-grid-details">
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Площадь участка, м²</span>
                      <input className="field" value={values.plotArea} onChange={(event) => updateField("plotArea", event.target.value)} type="number" min="0.1" step="0.1" required />
                    </label>

                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Разрешенное использование</span>
                      <input className="field" value={values.allowedUse} onChange={(event) => updateField("allowedUse", event.target.value)} placeholder="Например: ИЖС" required />
                    </label>

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Категория земли",
                          values.landCategory,
                          (nextValue) => updateField("landCategory", nextValue),
                          lookupsQuery.data.landCategories,
                          "landCategory",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Подъездные пути",
                          values.roadAccessType,
                          (nextValue) => updateField("roadAccessType", nextValue),
                          lookupsQuery.data.roadAccessTypes,
                          "roadAccessType",
                        )
                      : null}
                  </div>
                </div>

                <div className="listing-create-switches">
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasElectricity} onChange={(event) => updateField("hasElectricity", event.target.checked)} />
                    Электричество
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasGas} onChange={(event) => updateField("hasGas", event.target.checked)} />
                    Газ
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasWater} onChange={(event) => updateField("hasWater", event.target.checked)} />
                    Вода
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasSewer} onChange={(event) => updateField("hasSewer", event.target.checked)} />
                    Канализация
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasBuildings} onChange={(event) => updateField("hasBuildings", event.target.checked)} />
                    Есть постройки
                  </label>
                </div>
              </>
            ) : null}

            {values.listingDetailsType === "Office" ? (
              <>
                <div>
                  <div className="eyebrow">Параметры объекта</div>
                  <h2 className="section-title" style={{ marginTop: 8, marginBottom: 16 }}>
                    Характеристики офиса
                  </h2>
                  <div className="listing-create-grid listing-create-grid-details">
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Площадь офиса, м²</span>
                      <input className="field" value={values.plotArea} onChange={(event) => updateField("plotArea", event.target.value)} type="number" min="0.1" step="0.1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Этаж</span>
                      <input className="field" value={values.floor} onChange={(event) => updateField("floor", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Этажей в здании</span>
                      <input className="field" value={values.floorsTotal} onChange={(event) => updateField("floorsTotal", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Уровней</span>
                      <input className="field" value={values.levels} onChange={(event) => updateField("levels", event.target.value)} type="number" min="1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Парковочных мест</span>
                      <input className="field" value={values.parkingSpaces} onChange={(event) => updateField("parkingSpaces", event.target.value)} type="number" min="0" required />
                    </label>

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Класс офиса",
                          values.officeClass,
                          (nextValue) => updateField("officeClass", nextValue),
                          lookupsQuery.data.officeClasses,
                          "officeClass",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Планировка",
                          values.layoutType,
                          (nextValue) => updateField("layoutType", nextValue),
                          lookupsQuery.data.layoutTypes,
                          "layoutType",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Ремонт",
                          values.officeRenovationType,
                          (nextValue) => updateField("officeRenovationType", nextValue),
                          lookupsQuery.data.renovationTypes,
                          "renovationType",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Тип парковки",
                          values.parkingType,
                          (nextValue) => updateField("parkingType", nextValue),
                          lookupsQuery.data.layoutTypes,
                          "layoutType",
                        )
                      : null}
                  </div>
                </div>

                <div className="listing-create-switches">
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.kitchenette} onChange={(event) => updateField("kitchenette", event.target.checked)} />
                    Есть мини-кухня
                  </label>
                </div>
              </>
            ) : null}

            {values.listingDetailsType === "Warehouse" ? (
              <>
                <div>
                  <div className="eyebrow">Параметры объекта</div>
                  <h2 className="section-title" style={{ marginTop: 8, marginBottom: 16 }}>
                    Характеристики склада
                  </h2>
                  <div className="listing-create-grid listing-create-grid-details">
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Площадь склада, м²</span>
                      <input className="field" value={values.plotArea} onChange={(event) => updateField("plotArea", event.target.value)} type="number" min="0.1" step="0.1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Высота потолков, м</span>
                      <input className="field" value={values.warehouseCeilingHeight} onChange={(event) => updateField("warehouseCeilingHeight", event.target.value)} type="number" min="0.1" step="0.1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Нагрузка на пол, т/м²</span>
                      <input className="field" value={values.floorLoadTonsPerSquareMeter} onChange={(event) => updateField("floorLoadTonsPerSquareMeter", event.target.value)} type="number" min="0" step="0.1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Количество ворот</span>
                      <input className="field" value={values.gateCount} onChange={(event) => updateField("gateCount", event.target.value)} type="number" min="0" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Количество доков</span>
                      <input className="field" value={values.dockCount} onChange={(event) => updateField("dockCount", event.target.value)} type="number" min="0" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Мощность, кВт</span>
                      <input className="field" value={values.powerKw} onChange={(event) => updateField("powerKw", event.target.value)} type="number" min="0" step="0.1" required />
                    </label>
                    <label style={{ display: "grid", gap: 8 }}>
                      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Площадь офиса, м²</span>
                      <input className="field" value={values.officeArea} onChange={(event) => updateField("officeArea", event.target.value)} type="number" min="0" step="0.1" />
                    </label>

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Класс склада",
                          values.classType,
                          (nextValue) => updateField("classType", nextValue),
                          lookupsQuery.data.classTypes,
                          "classType",
                        )
                      : null}

                    {lookupsQuery.data
                      ? renderLookupSelect(
                          "Отопление",
                          values.warehouseHeatingType,
                          (nextValue) => updateField("warehouseHeatingType", nextValue),
                          lookupsQuery.data.heatingTypes,
                          "heatingType",
                        )
                      : null}
                  </div>
                </div>

                <div className="listing-create-switches">
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasDockLevelers} onChange={(event) => updateField("hasDockLevelers", event.target.checked)} />
                    Есть доклевеллеры
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasRamps} onChange={(event) => updateField("hasRamps", event.target.checked)} />
                    Есть рампы
                  </label>
                  <label className="listing-create-chip">
                    <input type="checkbox" checked={values.hasOffice} onChange={(event) => updateField("hasOffice", event.target.checked)} />
                    Есть офисная зона
                  </label>
                </div>
              </>
            ) : null}
            </section>
          </div>

          <aside className="surface" style={{ borderRadius: 28, padding: 24, display: "grid", gap: 20, position: "sticky", top: 24 }}>
            <div>
              <div className="eyebrow">Фотографии</div>
              <h2 className="section-title" style={{ marginTop: 8 }}>Изображения</h2>
            </div>

            <label style={{ display: "grid", gap: 8 }}>
              <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Добавьте изображения</span>
              <input
                className="field"
                type="file"
                accept="image/*"
                multiple
                onChange={handleFilesChange}
              />
            </label>

            <div style={{ color: "var(--foreground-muted)", fontSize: 14 }}>
              Добавлено изображений: {files.length}
            </div>

            <div className="listing-create-preview-strip">
              {filePreviews.length > 0 ? (
                filePreviews.map((preview) => (
                  <div
                    key={`${preview.file.name}:${preview.file.size}:${preview.file.lastModified}`}
                    className="listing-create-preview-card"
                  >
                    <button
                      type="button"
                      className="listing-create-preview-remove"
                      aria-label={`Удалить ${preview.file.name}`}
                      onClick={() => removeFile(preview.file)}
                    >
                      ×
                    </button>
                    <img src={preview.url} alt={preview.file.name} className="listing-create-preview-image" />
                    <div className="listing-create-preview-meta">
                      <div title={preview.file.name}>{preview.file.name}</div>
                    </div>
                  </div>
                ))
              ) : (
                <div className="listing-create-placeholder">
                  Здесь появятся добавленные изображения.
                </div>
              )}
            </div>

            <div>
              <div className="eyebrow">Адрес</div>
              <h2 className="section-title" style={{ marginTop: 8 }}>Расположение</h2>
            </div>

            <label style={{ display: "grid", gap: 8 }}>
              <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Адрес объекта</span>
              <input
                className="field"
                value={values.addressText}
                placeholder="Выберите точку на карте"
                readOnly
                required
              />
            </label>

            <button className="button-secondary" type="button" onClick={openMapPicker}>
              Выбрать адрес на карте
            </button>

            <div className="listing-create-summary">
              <div className="eyebrow">Сводка</div>
              <div className="listing-create-summary-line">
                <span>Тип объекта</span>
                <strong>{getListingDetailsTypeLabel(values.listingDetailsType)}</strong>
              </div>
              <div className="listing-create-summary-line">
                <span>Раздел формы</span>
                <strong>{getListingSectionLabel(values.listingDetailsType)}</strong>
              </div>
              <div className="listing-create-summary-line">
                <span>Тип сделки</span>
                <strong>{values.listingType ? translateLookup("listingType", values.listingType) : "Не выбран"}</strong>
              </div>
              <div className="listing-create-summary-line">
                <span>Адрес</span>
                <strong>{values.addressText || "Не выбран"}</strong>
              </div>
            </div>

            {error ? <div className="listing-create-alert listing-create-alert-error">{error}</div> : null}

            <div style={{ display: "grid", gap: 10 }}>
              <button className="button-primary" type="submit" disabled={isSubmitting || lookupsQuery.isLoading}>
                {isSubmitting ? "Отправка..." : "Создать объявление"}
              </button>
            </div>
          </aside>
        </form>
      </main>

      {isMapOpen ? (
        <div className="listing-create-modal-backdrop">
          <div className="listing-create-modal surface">
            <div style={{ display: "flex", justifyContent: "space-between", gap: 12, alignItems: "start", flexWrap: "wrap" }}>
              <div>
                <div className="eyebrow">Карта</div>
                <h2 className="section-title" style={{ marginTop: 8 }}>Выбор адреса на карте</h2>
              </div>
              <button className="button-secondary" type="button" onClick={closeMapPicker}>
                Закрыть
              </button>
            </div>

            <div className="listing-create-map-panel">
              <div ref={mapContainerRef} className="listing-create-map-canvas" />
              <div className="listing-create-map-sidebar">
                <div className="listing-create-map-hint">
                  Нажмите на точку на карте. Мы сохраним координаты и попытаемся определить адрес.
                </div>

                {isResolvingAddress ? (
                  <div className="listing-create-alert">Определяем адрес...</div>
                ) : null}

                {mapError ? (
                  <div className="listing-create-alert listing-create-alert-error">{mapError}</div>
                ) : null}

                <div className="listing-create-summary">
                  <div className="listing-create-summary-line">
                    <span>Адрес</span>
                    <strong>{draftLocation?.addressText || "Точка не выбрана"}</strong>
                  </div>
                  <div className="listing-create-summary-line">
                    <span>Широта</span>
                    <strong>
                      {draftLocation ? draftLocation.latitude.toFixed(6) : "—"}
                    </strong>
                  </div>
                  <div className="listing-create-summary-line">
                    <span>Долгота</span>
                    <strong>
                      {draftLocation ? draftLocation.longitude.toFixed(6) : "—"}
                    </strong>
                  </div>
                </div>

                <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                  <button className="button-primary" type="button" onClick={confirmMapSelection}>
                    Подтвердить адрес
                  </button>
                  <button className="button-secondary" type="button" onClick={() => setDraftLocation(null)}>
                    Сбросить точку
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      ) : null}
    </AppShell>
  );
}
