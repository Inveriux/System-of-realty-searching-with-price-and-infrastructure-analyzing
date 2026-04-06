"use client";

const YANDEX_MAPS_API_KEY =
  process.env.NEXT_PUBLIC_YANDEX_MAPS_API_KEY ??
  "3225bb0b-e2a7-455f-9c95-ce49d86acbcb";

export type YandexMapsApi = {
  ready(callback: () => void): void;
  Map: new (container: HTMLElement, state: unknown, options?: unknown) => YandexMapInstance;
  Placemark: new (geometry: [number, number], properties?: unknown, options?: unknown) => YandexPlacemark;
  Circle: new (geometry: [[number, number], number], properties?: unknown, options?: unknown) => YandexGeoObject;
  templateLayoutFactory?: {
    createClass(template: string): unknown;
  };
};

export type YandexMapInstance = {
  geoObjects: {
    add(object: unknown): void;
    remove?(object: unknown): void;
    getBounds?(): [[number, number], [number, number]] | null;
  };
  events?: {
    add(eventName: string, handler: (event: unknown) => void): void;
  };
  getBounds?(): [[number, number], [number, number]] | null;
  getZoom?(): number;
  setCenter?(
    center: [number, number],
    zoom?: number,
    options?: { checkZoomRange?: boolean; duration?: number },
  ): void;
  setBounds?(
    bounds: [[number, number], [number, number]],
    options?: { checkZoomRange?: boolean; zoomMargin?: number | number[]; useMapMargin?: boolean },
  ): void;
  setZoom?(zoom: number, options?: { checkZoomRange?: boolean }): void;
  destroy(): void;
};

export type YandexGeoObject = {
  geometry?: {
    setCoordinates?(coords: [number, number]): void;
    setRadius?(radius: number): void;
  };
};

export type YandexPlacemark = {
  events?: {
    add(eventName: string, handler: () => void): void;
  };
  balloon?: {
    open(): void;
  };
} & YandexGeoObject;

type GlobalWindowWithYandexMaps = typeof window & {
  ymaps?: unknown;
  __diplomaYandexMapsPromise__?: Promise<YandexMapsApi> | null;
};

export function loadYandexMaps() {
  if (typeof window === "undefined") {
    return Promise.reject(new Error("Yandex Maps доступны только в браузере."));
  }

  const globalWindow = window as GlobalWindowWithYandexMaps;

  if (globalWindow.ymaps) {
    return Promise.resolve(globalWindow.ymaps as unknown as YandexMapsApi);
  }

  if (globalWindow.__diplomaYandexMapsPromise__) {
    return globalWindow.__diplomaYandexMapsPromise__;
  }

  const existingScript = document.querySelector<HTMLScriptElement>(
    'script[data-diploma-yandex-maps="true"]',
  );

  globalWindow.__diplomaYandexMapsPromise__ = new Promise<YandexMapsApi>((resolve, reject) => {
    const resolveYandexMapsApi = () => {
      if (globalWindow.ymaps) {
        resolve(globalWindow.ymaps as unknown as YandexMapsApi);
        return;
      }

      reject(new Error("Yandex Maps API не инициализировался."));
    };

    const handleLoad = () => resolveYandexMapsApi();
    const handleError = () => {
      globalWindow.__diplomaYandexMapsPromise__ = null;
      reject(new Error("Не удалось загрузить Yandex Maps API."));
    };

    if (existingScript) {
      existingScript.addEventListener("load", handleLoad, { once: true });
      existingScript.addEventListener("error", handleError, { once: true });
      return;
    }

    const script = document.createElement("script");
    script.src = `https://api-maps.yandex.ru/2.1/?apikey=${encodeURIComponent(
      YANDEX_MAPS_API_KEY,
    )}&lang=ru_RU`;
    script.async = true;
    script.dataset.diplomaYandexMaps = "true";
    script.onload = handleLoad;
    script.onerror = handleError;
    document.head.appendChild(script);
  });

  return globalWindow.__diplomaYandexMapsPromise__;
}
