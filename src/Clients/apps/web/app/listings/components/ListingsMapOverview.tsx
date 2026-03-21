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
};

type YandexMapInstance = {
  geoObjects: {
    add(object: unknown): void;
  };
  destroy(): void;
};

type YandexPlacemark = {
  events?: {
    add(eventName: string, handler: () => void): void;
  };
  balloon?: {
    open(): void;
  };
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
};

export function ListingsMapOverview({ open, onClose, listings }: Props) {
  const mapContainerRef = useRef<HTMLDivElement | null>(null);
  const mapRef = useRef<YandexMapInstance | null>(null);
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
              center: fallbackCenter,
              zoom: 7,
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
    };
  }, [listings, open]);

  if (!open) {
    return null;
  }

  return (
    <div
      onClick={onClose}
      style={{
        position: "fixed",
        inset: 0,
        zIndex: 60,
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
          <button className="button-secondary" onClick={onClose} type="button">
            Закрыть
          </button>
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
