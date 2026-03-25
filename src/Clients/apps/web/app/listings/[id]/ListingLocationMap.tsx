"use client";

import Image from "next/image";
import { useEffect, useMemo, useRef, useState } from "react";
import type { ListingPagePoiDto } from "@diploma/contracts/listingPage";

const YANDEX_MAPS_API_KEY =
  process.env.NEXT_PUBLIC_YANDEX_MAPS_API_KEY ??
  "3225bb0b-e2a7-455f-9c95-ce49d86acbcb";

const knownMetroStationNames = new Set([
  "малиновка",
  "петровщина",
  "михалово",
  "грушевка",
  "институт культуры",
  "площадь ленина",
  "октябрьская",
  "площадь победы",
  "площадь якуба коласа",
  "академия наук",
  "парк челюскинцев",
  "московская",
  "восток",
  "борисовский тракт",
  "уручье",
  "могилевская",
  "автозаводская",
  "партизанская",
  "тракторный завод",
  "пролетарская",
  "первомайская",
  "купаловская",
  "немига",
  "фрунзенская",
  "юбилейная площадь",
  "молодежная",
  "пушкинская",
  "спортивная",
  "кунцевщина",
  "каменная горка",
  "ковальская слобода",
  "вокзальная",
  "площадь франтишка богушевича",
  "аэродромная",
]);

type YandexMapsApi = {
  ready(callback: () => void): void;
  Map: new (container: HTMLElement, state: unknown, options?: unknown) => YandexMapInstance;
  Placemark: new (geometry: [number, number], properties?: unknown, options?: unknown) => YandexPlacemark;
};

type YandexMapInstance = {
  geoObjects: {
    add(object: unknown): void;
    getBounds(): [[number, number], [number, number]] | null;
  };
  getZoom(): number;
  setBounds(
    bounds: [[number, number], [number, number]],
    options?: { checkZoomRange?: boolean; zoomMargin?: number | number[]; useMapMargin?: boolean },
  ): void;
  setZoom(zoom: number, options?: { checkZoomRange?: boolean }): void;
  destroy(): void;
};

type YandexPlacemark = unknown;
let yandexMapsPromise: Promise<YandexMapsApi> | null = null;

function loadYandexMaps() {
  if (typeof window === "undefined") {
    return Promise.reject(new Error("Yandex Maps доступны только в браузере."));
  }

  const globalWindow = window as typeof window & {
    ymaps?: unknown;
  };

  if (globalWindow.ymaps) {
    return Promise.resolve(globalWindow.ymaps as unknown as YandexMapsApi);
  }

  if (yandexMapsPromise) {
    return yandexMapsPromise;
  }

  yandexMapsPromise = new Promise<YandexMapsApi>((resolve, reject) => {
    const script = document.createElement("script");
    script.src = `https://api-maps.yandex.ru/2.1/?apikey=${encodeURIComponent(
      YANDEX_MAPS_API_KEY,
    )}&lang=ru_RU`;
    script.async = true;
    script.onload = () => {
      if (globalWindow.ymaps) {
        resolve(globalWindow.ymaps as unknown as YandexMapsApi);
        return;
      }

      reject(new Error("Yandex Maps API не инициализировался."));
    };
    script.onerror = () => reject(new Error("Не удалось загрузить Yandex Maps API."));
    document.head.appendChild(script);
  });

  return yandexMapsPromise;
}

function getListingMarkerOptions() {
  return {
    iconLayout: "default#image",
    iconImageHref: "/map-markers/listing-home.svg",
    iconImageSize: [48, 60],
    iconImageOffset: [-24, -56],
  };
}

const MIN_DETAIL_VIEW_ZOOM = 13;

type MarkerKind =
  | "metro_station"
  | "bus_stop"
  | "tram_stop"
  | "railway_station"
  | "parking"
  | "pharmacy"
  | "clinic"
  | "hospital"
  | "kindergarten"
  | "school"
  | "park"
  | "waterfront"
  | "fitness"
  | "pool"
  | "stadium"
  | "museum"
  | "theater"
  | "cinema"
  | "cafe"
  | "restaurant"
  | "bar"
  | "gallery"
  | "library"
  | "supermarket"
  | "commerce"
  | "education"
  | "healthcare"
  | "leisure"
  | "transport";

function isLikelyMetroPoi(poi: ListingPagePoiDto) {
  const normalizedName = poi.name.trim().toLowerCase();
  return poi.subcategoryCode === "metro_station"
    || (poi.category === "Транспорт"
      && (normalizedName.includes("метро")
        || normalizedName.includes("metro")
        || knownMetroStationNames.has(normalizedName)));
}

function getPoiMarkerKind(poi: ListingPagePoiDto): MarkerKind {
  const subcategoryCode = poi.subcategoryCode ?? (isLikelyMetroPoi(poi) ? "metro_station" : null);

  switch (subcategoryCode) {
    case "metro_station":
    case "bus_stop":
    case "tram_stop":
    case "railway_station":
    case "parking":
    case "pharmacy":
    case "clinic":
    case "hospital":
    case "kindergarten":
    case "school":
    case "park":
    case "waterfront":
    case "fitness":
    case "pool":
    case "stadium":
    case "museum":
    case "theater":
    case "cinema":
    case "cafe":
    case "restaurant":
    case "bar":
    case "gallery":
    case "library":
      return subcategoryCode;
    case "trolleybus_stop":
      return "bus_stop";
    case "dentistry":
    case "laboratory":
      return "clinic";
    case "college":
    case "university":
      return "school";
    case "square":
    case "recreation_zone":
      return "park";
    case "gym":
      return "fitness";
    case "sports_center":
      return "stadium";
    case "exhibition_hall":
      return "gallery";
    case "hypermarket":
    case "discount_store":
    case "convenience_store":
    case "supermarket":
      return "supermarket";
    case "mall":
    case "electronics":
    case "furniture":
    case "building_store":
    case "drogerie":
      return "commerce";
    default:
      switch (poi.category) {
        case "Коммерция":
          return "commerce";
        case "Здоровье":
          return "healthcare";
        case "Образование":
          return "education";
        case "Досуг":
          return "leisure";
        case "Транспорт":
          return "transport";
        default:
          return "transport";
      }
  }
}

function getMarkerMeta(markerKind: MarkerKind) {
  switch (markerKind) {
    case "metro_station":
      return { iconHref: "/map-markers/minsk-metro.svg", label: "Метро" };
    case "bus_stop":
      return { iconHref: "/map-markers/bus-stop.svg", label: "Автобус" };
    case "tram_stop":
      return { iconHref: "/map-markers/tram-stop.svg", label: "Трамвай" };
    case "railway_station":
      return { iconHref: "/map-markers/railway-station.svg", label: "Ж/д" };
    case "parking":
      return { iconHref: "/map-markers/parking.svg", label: "Парковка" };
    case "pharmacy":
      return { iconHref: "/map-markers/pharmacy.svg", label: "Аптека" };
    case "clinic":
      return { iconHref: "/map-markers/healthcare.svg", label: "Клиника" };
    case "hospital":
      return { iconHref: "/map-markers/hospital.svg", label: "Больница" };
    case "kindergarten":
      return { iconHref: "/map-markers/kindergarten.svg", label: "Сад" };
    case "school":
      return { iconHref: "/map-markers/school.svg", label: "Школа" };
    case "park":
      return { iconHref: "/map-markers/park.svg", label: "Парк" };
    case "waterfront":
      return { iconHref: "/map-markers/waterfront.svg", label: "Набережная" };
    case "fitness":
      return { iconHref: "/map-markers/fitness.svg", label: "Фитнес" };
    case "pool":
      return { iconHref: "/map-markers/pool.svg", label: "Бассейн" };
    case "stadium":
      return { iconHref: "/map-markers/stadium.svg", label: "Спорт" };
    case "museum":
      return { iconHref: "/map-markers/museum.svg", label: "Музей" };
    case "theater":
      return { iconHref: "/map-markers/theater.svg", label: "Театр" };
    case "cinema":
      return { iconHref: "/map-markers/cinema.svg", label: "Кино" };
    case "cafe":
      return { iconHref: "/map-markers/cafe.svg", label: "Кафе" };
    case "restaurant":
      return { iconHref: "/map-markers/restaurant.svg", label: "Ресторан" };
    case "bar":
      return { iconHref: "/map-markers/bar.svg", label: "Бар" };
    case "gallery":
      return { iconHref: "/map-markers/gallery.svg", label: "Галерея" };
    case "library":
      return { iconHref: "/map-markers/library.svg", label: "Библиотека" };
    case "supermarket":
      return { iconHref: "/map-markers/supermarket.svg", label: "Магазины" };
    case "commerce":
      return { iconHref: "/map-markers/commerce.svg", label: "Коммерция" };
    case "education":
      return { iconHref: "/map-markers/education.svg", label: "Образование" };
    case "healthcare":
      return { iconHref: "/map-markers/healthcare.svg", label: "Здоровье" };
    case "leisure":
      return { iconHref: "/map-markers/leisure.svg", label: "Досуг" };
    case "transport":
    default:
      return { iconHref: "/map-markers/transport.svg", label: "Транспорт" };
  }
}

function getPoiMarkerOptions(poi: ListingPagePoiDto) {
  const { iconHref } = getMarkerMeta(getPoiMarkerKind(poi));

  return {
    iconLayout: "default#image",
    iconImageHref: iconHref,
    iconImageSize: [48, 60],
    iconImageOffset: [-24, -56],
  };
}

type Props = {
  title: string;
  addressText: string | null;
  latitude: number;
  longitude: number;
  nearbyPois: ListingPagePoiDto[];
};

export function ListingLocationMap({ title, addressText, latitude, longitude, nearbyPois }: Props) {
  const mapRef = useRef<YandexMapInstance | null>(null);
  const mapContainerRef = useRef<HTMLDivElement | null>(null);
  const [mapError, setMapError] = useState<string | null>(null);
  const availableMarkerKinds = useMemo(() => {
    const seen = new Set<MarkerKind>();
    for (const poi of nearbyPois) {
      seen.add(getPoiMarkerKind(poi));
    }

    return Array.from(seen);
  }, [nearbyPois]);
  const [enabledMarkerKinds, setEnabledMarkerKinds] = useState<MarkerKind[] | null>(null);

  useEffect(() => {
    setEnabledMarkerKinds((current) => {
      if (current === null) {
        return availableMarkerKinds;
      }

      const availableSet = new Set(availableMarkerKinds);
      return current.filter((markerKind) => availableSet.has(markerKind));
    });
  }, [availableMarkerKinds]);

  const visiblePois = useMemo(() => {
    if (enabledMarkerKinds === null) {
      return nearbyPois;
    }

    const enabledSet = new Set(enabledMarkerKinds);
    return nearbyPois.filter((poi) => enabledSet.has(getPoiMarkerKind(poi)));
  }, [enabledMarkerKinds, nearbyPois]);

  function toggleMarkerKind(markerKind: MarkerKind) {
    setEnabledMarkerKinds((current) =>
      (current ?? availableMarkerKinds).includes(markerKind)
        ? (current ?? availableMarkerKinds).filter((item) => item !== markerKind)
        : [...(current ?? availableMarkerKinds), markerKind]);
  }

  useEffect(() => {
    let cancelled = false;

    async function initMap() {
      try {
        const ymaps = await loadYandexMaps();
        if (!ymaps) {
          setMapError("Не удалось инициализировать карту.");
          return;
        }

        if (cancelled || !mapContainerRef.current) {
          return;
        }

        ymaps.ready(() => {
          if (cancelled || !mapContainerRef.current) {
            return;
          }

          mapRef.current?.destroy();

          const map = new ymaps.Map(
            mapContainerRef.current,
            {
              center: [latitude, longitude],
              zoom: 15,
              controls: ["zoomControl", "fullscreenControl"],
            },
            {
              suppressMapOpenBlock: true,
              minZoom: MIN_DETAIL_VIEW_ZOOM,
            },
          );

          map.geoObjects.add(
            new ymaps.Placemark(
              [latitude, longitude],
              {
                balloonContentHeader: title,
                balloonContentBody: addressText ?? "Адрес уточняется",
              },
              {
                ...getListingMarkerOptions(),
              },
            ),
          );

          for (const poi of visiblePois) {
            map.geoObjects.add(
              new ymaps.Placemark(
                [poi.latitude, poi.longitude],
                {
                  balloonContentHeader: poi.name,
                  balloonContentBody: poi.addressText ?? `${poi.category}, ${poi.distanceMeters.toFixed(0)} м`,
                },
                getPoiMarkerOptions(poi),
              ),
            );
          }

          const bounds = map.geoObjects.getBounds();
          if (bounds) {
            map.setBounds(bounds, {
              checkZoomRange: true,
              zoomMargin: [56, 56, 56, 56],
              useMapMargin: true,
            });
          }

          // Keep the listing map focused on the property area even after bounds auto-fit.
          const currentZoom = map.getZoom?.();
          if (typeof currentZoom === "number" && currentZoom < MIN_DETAIL_VIEW_ZOOM) {
            map.setZoom?.(MIN_DETAIL_VIEW_ZOOM, { checkZoomRange: true });
          }

          mapRef.current = map;
        });
      } catch (error) {
        setMapError(error instanceof Error ? error.message : "Не удалось загрузить карту.");
      }
    }

    void initMap();

    return () => {
      cancelled = true;
      mapRef.current?.destroy();
      mapRef.current = null;
    };
  }, [addressText, latitude, longitude, title, visiblePois]);

  return (
    <div className="grid h-full min-h-[420px] gap-3 xl:min-h-[560px]">
      {availableMarkerKinds.length > 0 ? (
        <div className="flex flex-wrap gap-2">
          {availableMarkerKinds.map((markerKind) => {
            const meta = getMarkerMeta(markerKind);
            const isActive = (enabledMarkerKinds ?? availableMarkerKinds).includes(markerKind);

            return (
              <button
                key={markerKind}
                type="button"
                onClick={() => toggleMarkerKind(markerKind)}
                className={`inline-flex items-center gap-2 rounded-full border px-3 py-1.5 text-[11.5px] font-bold transition ${
                  isActive
                    ? "border-[color:color-mix(in_srgb,var(--color-primary)_28%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_72%,white)] text-[color:var(--foreground)]"
                    : "border-[color:color-mix(in_srgb,var(--border-color)_90%,white)] bg-[color:color-mix(in_srgb,var(--background-elevated)_92%,white)] text-[color:var(--foreground-muted)] opacity-70"
                }`}
              >
                <Image
                  src={meta.iconHref}
                  alt=""
                  width={16}
                  height={16}
                  unoptimized
                  aria-hidden="true"
                  className={isActive ? "" : "grayscale"}
                />
                <span>{meta.label}</span>
              </button>
            );
          })}
        </div>
      ) : null}

      <div className="relative h-full min-h-[420px] overflow-hidden rounded-[28px] border border-[var(--border-color)] bg-[var(--background-muted)] xl:min-h-[560px]">
        <div
          ref={mapContainerRef}
          className="h-full min-h-[420px] xl:min-h-[560px]"
        />
      </div>
      {mapError ? (
        <div className="rounded-[18px] border border-[#f0c36d] bg-[#fff5df] px-4 py-3 text-sm text-[#8c5a00]">
          {mapError}
        </div>
      ) : null}
    </div>
  );
}
