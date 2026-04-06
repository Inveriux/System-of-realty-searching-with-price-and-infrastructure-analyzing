"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import type { ListingPagePoiDto } from "@diploma/contracts/listingPage";
import {
  loadYandexMaps,
  type YandexMapInstance,
  type YandexMapsApi,
  type YandexPlacemark,
} from "../lib/loadYandexMaps";

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

function getListingMarkerOptions() {
  return {
    iconLayout: "default#image",
    iconImageHref: "/map-markers/listing-home.svg",
    iconImageSize: [38, 48],
    iconImageOffset: [-19, -44],
  };
}

const MIN_DETAIL_VIEW_ZOOM = 13;

type MarkerKind =
  | "metro_station"
  | "bus_stop"
  | "tram_stop"
  | "railway_station"
  | "parking"
  | "fuel_station"
  | "service_station"
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
  | "beauty_service"
  | "landmark"
  | "commerce"
  | "education"
  | "healthcare"
  | "leisure"
  | "transport";

const MARKER_KIND_ORDER: MarkerKind[] = [
  "metro_station",
  "bus_stop",
  "tram_stop",
  "railway_station",
  "parking",
  "fuel_station",
  "service_station",
  "pharmacy",
  "clinic",
  "hospital",
  "kindergarten",
  "school",
  "park",
  "waterfront",
  "fitness",
  "pool",
  "stadium",
  "museum",
  "theater",
  "cinema",
  "gallery",
  "library",
  "landmark",
  "cafe",
  "restaurant",
  "bar",
  "supermarket",
  "beauty_service",
  "commerce",
  "education",
  "healthcare",
  "leisure",
  "transport",
];

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
  const normalizedName = poi.name.trim().toLowerCase();
  const looksLikeMetro = subcategoryCode === "metro_station"
    || isLikelyMetroPoi(poi)
    || normalizedName.includes("метро")
    || normalizedName.includes("metro")
    || knownMetroStationNames.has(normalizedName);

  if (looksLikeMetro) {
    return "metro_station";
  }

  switch (subcategoryCode) {
    case "bus_stop":
    case "tram_stop":
    case "railway_station":
    case "parking":
    case "fuel_station":
    case "service_station":
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
    case "beauty_service":
    case "landmark":
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
    case "fuel_station":
      return { iconHref: "/map-markers/fuel-station.svg", label: "АЗС" };
    case "service_station":
      return { iconHref: "/map-markers/service-station.svg", label: "СТО / мойка" };
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
    case "beauty_service":
      return { iconHref: "/map-markers/beauty-service.svg", label: "Красота" };
    case "landmark":
      return { iconHref: "/map-markers/landmark.svg", label: "Место" };
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

function normalizeMarkerTitle(value: string | null | undefined) {
  const trimmed = value?.trim() ?? "";
  if (!trimmed) {
    return null;
  }

  return trimmed;
}

function truncateMarkerText(value: string, maxLength = 22) {
  if (value.length <= maxLength) {
    return value;
  }

  return `${value.slice(0, maxLength - 1).trimEnd()}…`;
}

function escapeHtml(value: string) {
  return value
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll("\"", "&quot;")
    .replaceAll("'", "&#39;");
}

function getPoiMarkerTitle(poi: ListingPagePoiDto) {
  const explicitName = normalizeMarkerTitle(poi.name);
  if (explicitName) {
    return truncateMarkerText(explicitName);
  }

  return getMarkerMeta(getPoiMarkerKind(poi)).label;
}

function getPoiMarkerDistance(poi: ListingPagePoiDto) {
  return `${poi.distanceMeters.toFixed(0)} м`;
}

function getPoiMarkerLayoutTemplate() {
  return "";
}

function getPoiMarkerOptions(ymaps: YandexMapsApi, poi: ListingPagePoiDto) {
  const { iconHref } = getMarkerMeta(getPoiMarkerKind(poi));
  void ymaps;
  void getPoiMarkerLayoutTemplate;

  return {
    iconLayout: "default#image",
    iconImageHref: iconHref,
    iconImageSize: [30, 38],
    iconImageOffset: [-15, -34],
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

    return Array.from(seen).sort(
      (left, right) => MARKER_KIND_ORDER.indexOf(left) - MARKER_KIND_ORDER.indexOf(right),
    );
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

  function enableAllMarkerKinds() {
    setEnabledMarkerKinds(availableMarkerKinds);
  }

  function disableAllMarkerKinds() {
    setEnabledMarkerKinds([]);
  }

  function focusListingLocation() {
    setMapError(null);

    const map = mapRef.current;
    if (!map) {
      return;
    }

    const nextZoom = Math.max(map.getZoom?.() ?? MIN_DETAIL_VIEW_ZOOM, 15);
    map.setCenter?.(
      [latitude, longitude],
      nextZoom,
      { checkZoomRange: true, duration: 220 },
    );
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
            const markerMeta = getMarkerMeta(getPoiMarkerKind(poi));
            map.geoObjects.add(
              new ymaps.Placemark(
              [poi.latitude, poi.longitude],
                {
                  balloonContentHeader: getPoiMarkerTitle(poi),
                  balloonContentBody: [
                    poi.addressText,
                    `${getMarkerMeta(getPoiMarkerKind(poi)).label}, ${poi.distanceMeters.toFixed(0)} м`,
                  ]
                    .filter(Boolean)
                    .join("<br/>"),
                  iconHref: markerMeta.iconHref,
                  iconTitle: escapeHtml(getPoiMarkerTitle(poi)),
                  iconDistance: escapeHtml(getPoiMarkerDistance(poi)),
                },
                getPoiMarkerOptions(ymaps, poi),
              ),
            );
          }

          const bounds = map.geoObjects.getBounds?.();
          if (bounds) {
            map.setBounds?.(bounds, {
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
        <div className="grid gap-2">
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              onClick={enableAllMarkerKinds}
              className="inline-flex items-center gap-2 rounded-full border border-[color:color-mix(in_srgb,var(--color-primary)_28%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_72%,white)] px-3 py-1.5 text-[11.5px] font-bold text-[color:var(--foreground)] transition"
            >
              Включить всё
            </button>
            <button
              type="button"
              onClick={disableAllMarkerKinds}
              className="inline-flex items-center gap-2 rounded-full border border-[color:color-mix(in_srgb,var(--border-color)_90%,white)] bg-[color:color-mix(in_srgb,var(--background-elevated)_92%,white)] px-3 py-1.5 text-[11.5px] font-bold text-[color:var(--foreground-muted)] transition"
            >
              Убрать всё
            </button>
          </div>
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
                  <img
                    src={meta.iconHref}
                    alt=""
                    width={14}
                    height={14}
                    aria-hidden="true"
                    className={isActive ? "" : "grayscale"}
                  />
                  <span>{meta.label}</span>
                </button>
              );
            })}
          </div>
        </div>
      ) : null}

      <div className="relative h-full min-h-[420px] overflow-hidden rounded-[28px] border border-[var(--border-color)] bg-[var(--background-muted)] xl:min-h-[560px]">
        <div className="pointer-events-none absolute right-3 top-3 z-10 flex justify-end">
          <button
            type="button"
            onClick={focusListingLocation}
            className="pointer-events-auto inline-flex items-center gap-2 rounded-full border border-[color:color-mix(in_srgb,var(--color-primary)_22%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--background-elevated)_92%,white)] px-3 py-2 text-[11.5px] font-bold text-[color:var(--foreground)] shadow-[0_10px_30px_rgba(15,23,42,0.12)] transition hover:bg-white disabled:cursor-wait disabled:opacity-70"
          >
            <span aria-hidden="true">◎</span>
            <span>К объекту</span>
          </button>
        </div>
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
