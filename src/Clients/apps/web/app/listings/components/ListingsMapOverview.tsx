"use client";

import { useEffect, useRef, useState } from "react";

const YANDEX_MAPS_API_KEY =
  process.env.NEXT_PUBLIC_YANDEX_MAPS_API_KEY ??
  "3225bb0b-e2a7-455f-9c95-ce49d86acbcb";

type MapListingItem = {
  id: string;
  title: string;
  href: string;
  addressText: string | null;
  propertyType: string;
  latitude: number;
  longitude: number;
  priceText: string;
};

type YandexMapsApi = {
  ready(callback: () => void): void;
  Map: new (container: HTMLElement, state: unknown, options?: unknown) => YandexMapInstance;
  Placemark: new (geometry: [number, number], properties?: unknown, options?: unknown) => YandexPlacemark;
  Circle: new (geometry: [[number, number], number], properties?: unknown, options?: unknown) => YandexGeoObject;
};

type YandexMapInstance = {
  geoObjects: {
    add(object: unknown): void;
    remove(object: unknown): void;
  };
  events?: {
    add(eventName: string, handler: (event: YandexMapEvent) => void): void;
  };
  getBounds?(): [[number, number], [number, number]] | null;
  setCenter?(center: [number, number], zoom?: number, options?: unknown): void;
  destroy(): void;
};

type YandexGeoObject = {
  geometry?: {
    setCoordinates?(coords: [number, number]): void;
    setRadius?(radius: number): void;
  };
};

type YandexPlacemark = {
  events?: {
    add(eventName: string, handler: () => void): void;
  };
  balloon?: {
    open(): void;
  };
} & YandexGeoObject;

type YandexMapEvent = {
  get(name: "coords"): [number, number];
};

let yandexMapsPromise: Promise<YandexMapsApi> | null = null;

function loadYandexMaps() {
  if (typeof window === "undefined") {
    return Promise.reject(new Error("Yandex Maps доступны только в браузере."));
  }

  const globalWindow = window as typeof window & { ymaps?: unknown };

  if (globalWindow.ymaps) {
    return Promise.resolve(globalWindow.ymaps as YandexMapsApi);
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
        resolve(globalWindow.ymaps as YandexMapsApi);
        return;
      }

      reject(new Error("Yandex Maps API не инициализировался."));
    };
    script.onerror = () => reject(new Error("Не удалось загрузить Yandex Maps API."));
    document.head.appendChild(script);
  });

  return yandexMapsPromise;
}

type Props = {
  open: boolean;
  onClose(): void;
  listings: MapListingItem[];
  selectedCenter?: {
    latitude: number;
    longitude: number;
  } | null;
  radiusMeters?: number | null;
  onSelectCenter?(point: {
    latitude: number;
    longitude: number;
  }): void;
  onClearCenter?(): void;
};

export function ListingsMapOverview({
  open,
  onClose,
  listings,
  selectedCenter = null,
  radiusMeters = null,
  onSelectCenter,
  onClearCenter,
}: Props) {
  const mapContainerRef = useRef<HTMLDivElement | null>(null);
  const mapRef = useRef<YandexMapInstance | null>(null);
  const selectedPlacemarkRef = useRef<YandexPlacemark | null>(null);
  const radiusCircleRef = useRef<YandexGeoObject | null>(null);
  const [mapError, setMapError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) {
      return;
    }

    let cancelled = false;

    async function initMap() {
      try {
        const ymaps = await loadYandexMaps();
        if (cancelled || !mapContainerRef.current) {
          return;
        }

        const mappableListings = listings.filter(
          (listing) => Number.isFinite(listing.latitude) && Number.isFinite(listing.longitude),
        );

        ymaps.ready(() => {
          if (cancelled || !mapContainerRef.current) {
            return;
          }

          mapRef.current?.destroy();

          const fallbackCenter = mappableListings[0]
            ? [53.7098, 27.9534]
            : [53.7098, 27.9534];

          const map = new ymaps.Map(
            mapContainerRef.current,
            {
              center: selectedCenter
                ? [selectedCenter.latitude, selectedCenter.longitude]
                : fallbackCenter,
              zoom: selectedCenter ? 12 : 7,
              controls: ["zoomControl", "fullscreenControl"],
            },
            {
              suppressMapOpenBlock: true,
            },
          );

          for (const listing of mappableListings) {
            const placemark = new ymaps.Placemark(
              [listing.latitude, listing.longitude],
              {
                hintContent: listing.title,
                balloonContentHeader: listing.title,
                balloonContentBody: `
                  <div style="display:grid;gap:8px;min-width:220px;">
                    <div style="color:#5b3df5;font-size:13px;font-weight:700;">${listing.propertyType}</div>
                    <div style="color:#667085;font-size:13px;">${listing.addressText ?? "Адрес уточняется"}</div>
                    <div style="font-weight:800;">${listing.priceText}</div>
                    <a href="${listing.href}" style="color:#5b3df5;font-weight:700;text-decoration:none;">Открыть объявление</a>
                  </div>
                `,
              },
              {
                preset: "islands#darkBlueDotIcon",
              },
            );

            placemark.events?.add("mouseenter", () => {
              placemark.balloon?.open();
            });

            map.geoObjects.add(placemark);
          }

          let selectedPlacemark: YandexPlacemark | null = null;
          let radiusCircle: YandexGeoObject | null = null;

          if (selectedCenter) {
            selectedPlacemark = new ymaps.Placemark(
              [selectedCenter.latitude, selectedCenter.longitude],
              {
                hintContent: "Точка поиска",
                balloonContentHeader: "Точка поиска",
                balloonContentBody: "Объявления ищутся вокруг этой точки по заданному радиусу.",
              },
              {
                preset: "islands#redIcon",
              },
            );

            radiusCircle = new ymaps.Circle(
              [
                [selectedCenter.latitude, selectedCenter.longitude],
                radiusMeters && radiusMeters > 0 ? radiusMeters : 1000,
              ],
              {},
              {
                fillColor: "rgba(91, 61, 245, 0.12)",
                strokeColor: "rgba(91, 61, 245, 0.55)",
                strokeWidth: 2,
              },
            );

            map.geoObjects.add(radiusCircle);
            map.geoObjects.add(selectedPlacemark);
          }

          map.events?.add("click", (event) => {
            const [latitude, longitude] = event.get("coords");
            onSelectCenter?.({ latitude, longitude });
          });

          selectedPlacemarkRef.current = selectedPlacemark;
          radiusCircleRef.current = radiusCircle;
          mapRef.current = map;
        });
      } catch (error) {
        setMapError(error instanceof Error ? error.message : "Не удалось загрузить обзорную карту.");
      }
    }

    void initMap();

    return () => {
      cancelled = true;
      mapRef.current?.destroy();
      mapRef.current = null;
      selectedPlacemarkRef.current = null;
      radiusCircleRef.current = null;
    };
  }, [listings, onSelectCenter, open, radiusMeters, selectedCenter]);

  useEffect(() => {
    if (!open || !selectedCenter) {
      return;
    }

    selectedPlacemarkRef.current?.geometry?.setCoordinates?.([
      selectedCenter.latitude,
      selectedCenter.longitude,
    ]);
    radiusCircleRef.current?.geometry?.setCoordinates?.([
      selectedCenter.latitude,
      selectedCenter.longitude,
    ]);
    if (radiusMeters && radiusMeters > 0) {
      radiusCircleRef.current?.geometry?.setRadius?.(radiusMeters);
    }
    mapRef.current?.setCenter?.([selectedCenter.latitude, selectedCenter.longitude], 12, {
      duration: 250,
    });
  }, [open, radiusMeters, selectedCenter]);

  if (!open) {
    return null;
  }

  return (
    <div
      onClick={onClose}
      style={{
        position: "fixed",
        inset: 0,
        zIndex: 80,
        background: "rgba(10, 7, 20, 0.5)",
      }}
    >
      <div
        className="surface"
        onClick={(event) => event.stopPropagation()}
        style={{
          width: "100%",
          height: "100%",
          borderRadius: 0,
          padding: 18,
          display: "grid",
          gap: 14,
          gridTemplateRows: "auto minmax(0, 1fr)",
        }}
      >
        <div style={{ display: "flex", justifyContent: "space-between", gap: 16, alignItems: "center" }}>
          <h2 className="section-title" style={{ margin: 0 }}>Объекты на карте</h2>
          <div style={{ display: "flex", gap: 10, alignItems: "center", flexWrap: "wrap", justifyContent: "flex-end" }}>
            <div style={{ color: "var(--foreground-muted)", fontSize: 14, maxWidth: 420, textAlign: "right" }}>
              Кликни по карте, чтобы поставить метку. Поиск объявлений будет идти рядом с этой точкой в заданном радиусе.
            </div>
            {onClearCenter ? (
              <button className="button-secondary" onClick={onClearCenter} type="button">
                Сбросить точку
              </button>
            ) : null}
            <button className="button-secondary" onClick={onClose} type="button">
              Закрыть
            </button>
          </div>
        </div>

        <div
          ref={mapContainerRef}
          className="overflow-hidden rounded-[24px] border border-[var(--border-color)] bg-[var(--background-muted)]"
          style={{ minHeight: 0 }}
        />

        {mapError ? (
          <div className="rounded-[18px] border border-[#f0c36d] bg-[#fff5df] px-4 py-3 text-sm text-[#8c5a00]">
            {mapError}
          </div>
        ) : null}
      </div>
    </div>
  );
}
