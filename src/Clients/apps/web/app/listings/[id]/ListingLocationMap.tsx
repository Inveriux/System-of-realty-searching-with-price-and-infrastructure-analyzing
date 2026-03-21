"use client";

import { useEffect, useRef, useState } from "react";
import type { ListingPagePoiDto } from "@diploma/contracts/listingPage";

const YANDEX_MAPS_API_KEY =
  process.env.NEXT_PUBLIC_YANDEX_MAPS_API_KEY ??
  "3225bb0b-e2a7-455f-9c95-ce49d86acbcb";

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
                preset: "islands#darkBlueDotIcon",
              },
            ),
          );

          for (const poi of nearbyPois.slice(0, 12)) {
            map.geoObjects.add(
              new ymaps.Placemark(
                [poi.latitude, poi.longitude],
                {
                  balloonContentHeader: poi.name,
                  balloonContentBody: poi.addressText ?? `${poi.category}, ${poi.distanceMeters.toFixed(0)} м`,
                },
                {
                  preset: "islands#blueCircleDotIcon",
                },
              ),
            );
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
  }, [addressText, latitude, longitude, nearbyPois, title]);

  return (
    <div className="grid h-full min-h-[420px] gap-3 xl:min-h-[560px]">
      <div
        ref={mapContainerRef}
        className="h-full min-h-[420px] overflow-hidden rounded-[28px] border border-[var(--border-color)] bg-[var(--background-muted)] xl:min-h-[560px]"
      />
      {mapError ? (
        <div className="rounded-[18px] border border-[#f0c36d] bg-[#fff5df] px-4 py-3 text-sm text-[#8c5a00]">
          {mapError}
        </div>
      ) : null}
    </div>
  );
}
