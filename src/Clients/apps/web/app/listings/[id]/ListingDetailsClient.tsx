"use client";

import { useListingPageQuery } from "@diploma/features";
import type { CSSProperties } from "react";
import { AppShell } from "../../components/AppShell";
import { useAuth } from "../../providers";
import { ListingGallery } from "./ListingGallery";
import { ListingLocationMap } from "./ListingLocationMap";

const fallbackImage =
  "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1200 760'%3E%3Crect width='1200' height='760' fill='%23efe8ff'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%235326d4' font-family='Arial' font-size='54'%3EListing%20Preview%3C/text%3E%3C/svg%3E";

function formatPrice(value?: number | null) {
  if (value == null) return "Цена по запросу";
  return new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 0 }).format(value);
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
  return value;
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
  houseFloors: "Этажность дома",
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
  Central: "Центральное",
  Gravel: "Гравийный",
};

function normalizePoiName(name: string) {
  const trimmed = name.trim();
  if (!trimmed || trimmed === "Инфраструктурный объект" || trimmed === "Транспортный узел") {
    return null;
  }

  return trimmed;
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

function getDetailEntries(details: Record<string, unknown> | null | undefined) {
  if (!details) {
    return [];
  }

  return Object.entries(details)
    .filter(([, value]) => value !== null && value !== undefined && value !== "")
    .map(([key, value]) => ({
      key,
      label: detailLabelMap[key] ?? key,
      value: formatDetailValue(value),
    }));
}

function dedupePois<T extends { id: string; category: string; distanceMeters: number }>(
  pois: T[],
  getName: (poi: T) => string | null,
) {
  const seen = new Set<string>();

  return pois.filter((poi) => {
    const name = getName(poi);
    if (!name) {
      return false;
    }

    const normalized = name.toLowerCase();
    const bucket = poi.category === "Транспорт" ? Math.round(poi.distanceMeters / 25) : Math.round(poi.distanceMeters / 15);
    const key = `${poi.category}:${normalized}:${bucket}`;
    if (seen.has(key)) {
      return false;
    }

    seen.add(key);
    return true;
  });
}

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
  const { data, isLoading, error } = useListingPageQuery(api, listingId);

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
          <section className="surface rounded-[28px] p-8 text-[#d04b73]">
            Не удалось загрузить страницу объявления.
          </section>
        </main>
      </AppShell>
    );
  }

  const { listing, seller, infrastructure, derived } = data;
  const galleryMedia = [
    listing.mainMedia,
    ...listing.media.map((item) => item.url),
  ]
    .filter((item): item is string => Boolean(item))
    .filter((item, index, array) => array.indexOf(item) === index);
  const media = galleryMedia.length > 0 ? galleryMedia : [fallbackImage];
  const detailEntries = getDetailEntries(listing.listingDetails);
  const bynPricePerSquareMeter = formatNumericPricePerSquareMeter(
    listing.priceAmountByn ?? listing.priceAmount,
    listing.areaTotal,
  );
  const usdPricePerSquareMeter = formatNumericPricePerSquareMeter(
    listing.priceAmountUsd,
    listing.areaTotal,
  );
  const metroPoi = infrastructure?.nearbyPois.find((poi) => {
    const normalizedName = normalizePoiName(poi.name);
    return poi.category === "Транспорт" &&
      poi.distanceMeters <= 350 &&
      normalizedName !== null &&
      /метро|купалаўская|кастрычніцкая|октябрьская|станц/i.test(normalizedName);
  });
  const cleanedNearbyPois = dedupePois(
    (infrastructure?.nearbyPois
    .map((poi) => ({ ...poi, normalizedName: normalizePoiName(poi.name) }))
    ?? []),
    (poi) => poi.normalizedName,
  );
  const infoCardClass = "rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4";

  return (
    <AppShell>
      <main className="grid gap-6 pb-10">
        <section className="grid gap-6 xl:grid-cols-[minmax(0,1.45fr)_360px]">
          <article className="surface overflow-hidden rounded-[32px]">
            <ListingGallery title={listing.title} media={media} />
            <div className="grid gap-5 p-8">
              <div className="grid gap-3">
                <span className="eyebrow">{listing.addressText ?? "Адрес уточняется"}</span>
                <h1 className="page-title" style={{ fontSize: "clamp(2.4rem, 4vw, 4.2rem)" }}>
                  {listing.title}
                </h1>
                <div className="flex flex-wrap gap-2 text-sm font-semibold text-[var(--foreground-muted)]">
                  <span className="rounded-full bg-[var(--background-muted)] px-3 py-1.5">
                    {formatDealType(derived.quickStats.find((item) => item.key === "dealType")?.value)}
                  </span>
                  {listing.buildYear ? (
                    <span className="rounded-full bg-[var(--background-muted)] px-3 py-1.5">
                      Год постройки: {listing.buildYear}
                    </span>
                  ) : null}
                </div>
                <div className="flex flex-wrap items-start gap-3">
                  <div className="inline-flex max-w-full items-center gap-3 rounded-[24px] border border-[var(--border-color)] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_64%,var(--background-elevated))] px-5 py-4 text-[1.72rem] font-black tracking-[-0.05em] text-[var(--color-primary-strong)]">
                    <span className="inline-flex items-center gap-2">
                      <span>{formatPrice(listing.priceAmountByn ?? listing.priceAmount)}</span>
                      <span
                        className="inline-block h-[0.92em] w-[0.74em] shrink-0 bg-current align-[-0.08em]"
                        style={bynIconStyle}
                        aria-hidden="true"
                      />
                    </span>
                    {listing.priceAmountUsd != null ? (
                      <>
                        <span className="text-[1.38rem] font-extrabold text-[var(--foreground-muted)]">/</span>
                        <span className="inline-flex items-center gap-2">
                          <span>{formatPrice(listing.priceAmountUsd)}</span>
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
                    <div className="grid min-w-[132px] gap-1.5 rounded-[22px] border border-[var(--border-color)] bg-[var(--background-elevated)] px-4 py-3 text-[0.98rem] font-bold tracking-[-0.03em] text-[var(--foreground)]">
                      {bynPricePerSquareMeter ? (
                        <span className="inline-flex items-center gap-1.5">
                          <span>{bynPricePerSquareMeter}</span>
                          <span
                            className="inline-block h-[0.88em] w-[0.7em] shrink-0 bg-current align-[-0.08em]"
                            style={bynIconStyle}
                            aria-hidden="true"
                          />
                          <span className="text-[var(--foreground-muted)]">/м²</span>
                        </span>
                      ) : null}
                      {usdPricePerSquareMeter ? (
                        <span className="inline-flex items-center gap-1.5">
                          <span>{usdPricePerSquareMeter}</span>
                          <span
                            className="inline-block h-[0.9em] w-[0.76em] shrink-0 bg-current align-[-0.08em]"
                            style={dollarIconStyle}
                            aria-hidden="true"
                          />
                          <span className="text-[var(--foreground-muted)]">/м²</span>
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
                  <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
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
          </article>

          <aside className="grid gap-5 self-start">
            <section className="surface rounded-[28px] p-6">
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

            <section className="surface rounded-[28px] p-6">
              <div className="eyebrow">Инфраструктура</div>
              {infrastructure ? (
                <div className="mt-3 grid gap-3">
                  <div className="text-[2rem] font-black tracking-[-0.04em] text-[var(--color-primary-strong)]">
                    {infrastructure.totalScore?.toFixed(1) ?? "—"}
                  </div>
                  <div className="grid gap-2 sm:grid-cols-2">
                    <div className={infoCardClass}>
                      Транспорт: {infrastructure.transportScore?.toFixed(1) ?? "—"}
                    </div>
                    <div className={infoCardClass}>
                      Коммерция: {infrastructure.commerceScore?.toFixed(1) ?? "—"}
                    </div>
                    <div className={infoCardClass}>
                      Образование: {infrastructure.educationScore?.toFixed(1) ?? "—"}
                    </div>
                    <div className={infoCardClass}>
                      Здоровье: {infrastructure.healthcareScore?.toFixed(1) ?? "—"}
                    </div>
                  </div>
                  {infrastructure.summary ? (
                    <p style={{ margin: 0, color: "var(--foreground-muted)", lineHeight: 1.6 }}>
                      {infrastructure.summary}
                    </p>
                  ) : null}
                  {metroPoi ? (
                    <div className="rounded-[22px] border border-[color:color-mix(in_srgb,var(--color-primary)_28%,var(--border-color))] bg-[linear-gradient(180deg,color-mix(in_srgb,var(--color-primary-soft)_75%,white)_0%,var(--background-elevated)_100%)] px-4 py-4">
                      <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                        Метро
                      </div>
                      <div className="mt-1 text-[1.1rem] font-extrabold tracking-[-0.03em] text-[var(--foreground)]">
                        {normalizePoiName(metroPoi.name) ?? "Станция метро"}
                      </div>
                      <div className="mt-1 text-sm text-[var(--foreground-muted)]">
                        {metroPoi.distanceMeters.toFixed(0)} м пешком
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
                </div>
              ) : (
                <div className="mt-3 text-sm text-[var(--foreground-muted)]">
                  Инфраструктурный блок временно недоступен.
                </div>
              )}
            </section>
          </aside>
        </section>

        {listing.latitude != null && listing.longitude != null ? (
          <section className="surface rounded-[32px] p-6 sm:p-8">
            <div className="grid gap-5 xl:min-h-[620px] xl:grid-cols-[minmax(0,1.2fr)_360px] xl:items-stretch">
              <div className="grid gap-4 xl:min-h-[620px] xl:grid-rows-[auto_minmax(0,1fr)]">
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
                  nearbyPois={infrastructure?.nearbyPois ?? []}
                />
              </div>

              <div className="grid gap-4 self-start xl:self-stretch">
                <section className="rounded-[24px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-5">
                  <div className="text-sm font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                    Ближайшие объекты
                  </div>
                  {cleanedNearbyPois.length ? (
                    <div className="mt-4 grid gap-2">
                      {cleanedNearbyPois.slice(0, 8).map((poi) => (
                        <div
                          key={poi.id}
                          className={infoCardClass}
                        >
                          <div className="font-semibold">{poi.normalizedName}</div>
                          <div className="mt-1 text-[var(--foreground-muted)]">
                            {poi.category}, {poi.distanceMeters.toFixed(0)} м
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="mt-4 text-sm text-[var(--foreground-muted)]">
                      Рядом пока нет детализированных POI.
                    </div>
                  )}
                </section>
              </div>
            </div>
          </section>
        ) : null}
      </main>
    </AppShell>
  );
}
