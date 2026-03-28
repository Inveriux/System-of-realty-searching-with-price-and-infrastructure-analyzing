"use client";

import { useEffect, useRef, useState } from "react";
import {
  loadYandexMaps,
  type YandexGeoObject,
  type YandexMapInstance,
  type YandexMapsApi,
  type YandexPlacemark,
} from "../lib/loadYandexMaps";
import { listingsMapOverviewStyles } from "./styles/listings-map-overview.styles";

type MapListingItem = {
  id: string;
  title: string;
  href: string;
  addressText: string | null;
  propertyType: string;
  latitude: number;
  longitude: number;
  priceText: string;
  publishedDate: string | null;
};

type YandexMapEvent = {
  get(name: "coords"): [number, number];
};

function formatPublishedDate(value: string | null) {
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
  }).format(parsed);
}

function getListingMarkerOptions(propertyType: string) {
  const normalized = propertyType.trim().toLowerCase();

  let iconImageHref = "/map-markers/listing-apartment.svg";

  switch (normalized) {
    case "дом":
      iconImageHref = "/map-markers/listing-house.svg";
      break;
    case "участок":
      iconImageHref = "/map-markers/listing-land.svg";
      break;
    case "офис":
      iconImageHref = "/map-markers/listing-office.svg";
      break;
    case "склад":
      iconImageHref = "/map-markers/listing-warehouse.svg";
      break;
    case "квартира":
    default:
      iconImageHref = "/map-markers/listing-apartment.svg";
      break;
  }

  return {
    iconLayout: "default#image",
    iconImageHref,
    iconImageSize: [32, 40],
    iconImageOffset: [-16, -36],
  };
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
            const publishedDateLabel = formatPublishedDate(listing.publishedDate);
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
                    ${publishedDateLabel ? `<div style="color:#667085;font-size:12px;">Опубликовано: ${publishedDateLabel}</div>` : ""}
                    <a href="${listing.href}" style="color:#5b3df5;font-weight:700;text-decoration:none;">Открыть объявление</a>
                  </div>
                `,
              },
              {
                ...getListingMarkerOptions(listing.propertyType),
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
      style={listingsMapOverviewStyles.backdrop}
    >
      <div
        className="surface"
        onClick={(event) => event.stopPropagation()}
        style={listingsMapOverviewStyles.panel}
      >
        <div style={listingsMapOverviewStyles.header}>
          <h2 className="section-title" style={listingsMapOverviewStyles.title}>Объекты на карте</h2>
          <div style={listingsMapOverviewStyles.headerActions}>
            <div style={listingsMapOverviewStyles.helpText}>
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
          style={listingsMapOverviewStyles.mapCanvas}
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
