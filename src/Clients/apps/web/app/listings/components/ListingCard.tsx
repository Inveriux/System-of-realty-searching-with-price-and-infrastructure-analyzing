"use client";

import {
  Camera,
  Heart,
  MapPin,
  MoveRight,
  Trash2,
} from "lucide-react";
import Image from "next/image";
import Link from "next/link";
import {
  formatPrice,
  formatNumericAmount,
  formatNumericPricePerSquareMeter,
  formatPricePeriodSuffix,
  formatInfrastructureScore,
  formatMeters,
  getBuildingTypeLabel,
  getDealBadgeTone,
  getMaterialLabel,
  getPropertyBadgeTone,
  getPrimarySpecs,
  getSecondaryFeatures,
  getTypeLabel,
} from "./listing-card.helpers";
import type { ListingCardItem } from "./listing-card.types";
import {
  bynIconStyle,
  dollarIconStyle,
  listingCardClasses,
} from "./styles/listing-card.styles";
import {
  getInlineSpecLabel,
  getSpecIcon,
  placeholderImage,
} from "./listing-card.ui";

type ListingCardProps = {
  item: ListingCardItem;
};

export function ListingCard({ item }: ListingCardProps) {
  const primarySpecs = getPrimarySpecs(item);
  const secondaryFeatures = getSecondaryFeatures(item);
  const href = item.href ?? `/listings/${item.id}`;
  const typeLabel = getTypeLabel(item);
  const isSale = item.listingType === "Продажа";
  const photosCount = item.mediaCount ?? (item.mainMedia ? 1 : 0);
  const materialLabel =
    item.details.kind === "apartment"
      ? getMaterialLabel(item.details.buildingMaterial)
      : item.details.kind === "house"
        ? getMaterialLabel(item.details.wallMaterial)
        : null;
  const bottomFeatures = materialLabel
    ? [...secondaryFeatures, { label: `Материал: ${materialLabel}` }]
    : secondaryFeatures;
  const bynPrice = formatNumericAmount(item.priceAmount);
  const usdPrice = formatNumericAmount(item.priceAmountUsd);
  const periodSuffix = formatPricePeriodSuffix(item.pricePeriod);
  const bynPricePerSquareMeter = isSale
    ? formatNumericPricePerSquareMeter(item.priceAmount, item.areaTotal)
    : null;
  const usdPricePerSquareMeter = isSale
    ? formatNumericPricePerSquareMeter(item.priceAmountUsd, item.areaTotal)
    : null;
  const infrastructureScore = formatInfrastructureScore(item.infrastructureScore);
  const transportNearest = formatMeters(item.transportNearestMeters);

  async function handleDelete(event: React.MouseEvent<HTMLButtonElement>) {
    event.preventDefault();
    event.stopPropagation();

    if (!item.onDelete || item.isDeleting) {
      return;
    }

    const confirmed = window.confirm(`Удалить объявление "${item.title}"?`);
    if (!confirmed) {
      return;
    }

    await item.onDelete(item.id);
  }

  return (
    <article className={listingCardClasses.article}>
      <Link href={href} className="flex h-full flex-col">
        <div className={listingCardClasses.media}>
          <Image
            src={item.mainMedia ?? placeholderImage}
            alt={item.title}
            fill
            unoptimized
            className="object-cover transition duration-500 ease-out group-hover:scale-[1.02]"
            sizes="(min-width: 1280px) 33vw, (min-width: 768px) 50vw, 100vw"
          />

          <div className="absolute inset-x-0 top-0 flex items-start justify-between gap-3 p-3.5">
            <div className="flex flex-wrap items-center gap-2">
              <span className={`rounded-full px-3 py-1.5 text-[12px] font-semibold shadow-sm backdrop-blur ${getDealBadgeTone(item.listingType)}`}>
                {item.listingType}
              </span>
              <span className={`rounded-full px-3 py-1.5 text-[12px] font-semibold shadow-sm backdrop-blur ${getPropertyBadgeTone(item)}`}>
                {typeLabel}
              </span>
            </div>

            <div className="flex flex-col items-center gap-2">
              <span className="grid h-8.5 w-8.5 place-items-center rounded-full border border-white/25 bg-[color:color-mix(in_srgb,var(--background-elevated)_86%,var(--color-primary-soft))] text-[color:var(--color-primary-strong)] shadow-sm backdrop-blur">
                <Heart className="h-4.5 w-4.5" />
              </span>
              {item.canDelete ? (
                <button
                  type="button"
                  onClick={handleDelete}
                  disabled={item.isDeleting}
                  className="grid h-8.5 w-8.5 place-items-center rounded-full border border-white/25 bg-[color:color-mix(in_srgb,#ffe2e2_88%,white)] text-[#b42318] shadow-sm backdrop-blur transition hover:scale-[1.03] disabled:cursor-not-allowed disabled:opacity-60"
                  aria-label="Удалить объявление"
                  title="Удалить объявление"
                >
                  <Trash2 className="h-4.5 w-4.5" />
                </button>
              ) : null}
            </div>
          </div>

          {photosCount > 0 ? (
            <div className="absolute right-3 bottom-3 flex items-center gap-1.5 rounded-full border border-white/12 bg-[color:color-mix(in_srgb,var(--color-primary-strong)_62%,black)] px-2.5 py-1 text-[12px] font-semibold text-white backdrop-blur">
              <Camera className="h-3.5 w-3.5" />
              <span>{photosCount} фото</span>
            </div>
          ) : null}
        </div>

        <div className={listingCardClasses.body}>
          <div className="flex flex-wrap items-start justify-between gap-x-4 gap-y-2">
            <div className={listingCardClasses.priceRow}>
              {item.priceCurrency === "BYN" && bynPrice ? (
                <>
                  <span>{bynPrice}</span>
                  <span
                    aria-label="BYN"
                    className={listingCardClasses.bynIcon}
                    style={bynIconStyle}
                  />
                  {usdPrice ? (
                    <>
                      <span className="text-[color:var(--foreground-muted)]">/</span>
                      <span>{usdPrice}</span>
                      <span
                        aria-label="USD"
                        className={listingCardClasses.usdIcon}
                        style={dollarIconStyle}
                      />
                    </>
                  ) : null}
                  {periodSuffix ? (
                    <span className="text-[1rem] font-semibold tracking-[-0.02em] text-[color:var(--foreground-muted)]">
                      /{periodSuffix}
                    </span>
                  ) : null}
                </>
              ) : (
                <span>{formatPrice(item.priceAmount, item.priceCurrency, item.pricePeriod)}</span>
              )}
            </div>

            {isSale && (bynPricePerSquareMeter || usdPricePerSquareMeter) ? (
              <div className={listingCardClasses.meterPrices}>
                {bynPricePerSquareMeter ? (
                  <span className={listingCardClasses.meterPrice}>
                    <span>{bynPricePerSquareMeter}</span>
                    <span
                      aria-label="BYN per square meter"
                      className={listingCardClasses.bynMeterIcon}
                      style={bynIconStyle}
                    />
                    <span>/м²</span>
                  </span>
                ) : null}
                {usdPricePerSquareMeter ? (
                  <span className={listingCardClasses.meterPrice}>
                    <span>{usdPricePerSquareMeter}</span>
                    <span
                      aria-label="USD per square meter"
                      className={listingCardClasses.usdMeterIcon}
                      style={dollarIconStyle}
                    />
                    <span>/м²</span>
                  </span>
                ) : null}
              </div>
            ) : null}
          </div>

          <h3 className={listingCardClasses.title}>
            {item.title}
          </h3>

          <div className="mt-2 flex items-start gap-2 text-[11.5px] leading-[1.35rem] font-semibold text-[color:color-mix(in_srgb,var(--foreground-muted)_82%,var(--foreground))]">
            <MapPin className="mt-0.5 h-3.5 w-3.5 shrink-0 text-[color:var(--color-primary)]" />
            <p className="line-clamp-2 min-w-0">{item.addressText ?? "Адрес уточняется"}</p>
          </div>

          <div className="mt-3 flex flex-wrap items-center gap-x-3 gap-y-2 text-[11.5px] leading-5 text-[color:color-mix(in_srgb,var(--foreground-muted)_78%,var(--foreground))]">
            {primarySpecs.map((spec) => {
              const Icon = getSpecIcon(spec.label);

              return (
                <span key={spec.label} className="inline-flex items-center gap-1.5 rounded-full border border-[color:color-mix(in_srgb,var(--color-primary)_20%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_64%,var(--background-elevated))] px-2.5 py-1">
                  <Icon className="h-3.5 w-3.5 text-[color:var(--color-primary)]" />
                  <span className="font-semibold text-[color:color-mix(in_srgb,var(--foreground-muted)_82%,var(--foreground))]">{getInlineSpecLabel(spec.label)}</span>
                  <span className="font-bold text-[color:var(--foreground)]">{spec.value}</span>
                </span>
              );
            })}
          </div>

          <div className="mt-3 border-t border-[color:color-mix(in_srgb,var(--color-primary)_14%,var(--border-color))] pt-2.5">
            <div className="flex flex-wrap gap-2">
              {infrastructureScore ? (
                <span className="rounded-full border border-[color:color-mix(in_srgb,#0f766e_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#ccfbf1_72%,var(--background-elevated))] px-2.5 py-1 text-[10.5px] font-bold text-[#115e59]">
                  Инфраструктура: {infrastructureScore}
                </span>
              ) : null}
              {item.transportCount != null && item.transportCount > 0 ? (
                <span className="rounded-full border border-[color:color-mix(in_srgb,#2563eb_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#dbeafe_78%,var(--background-elevated))] px-2.5 py-1 text-[10.5px] font-bold text-[#1d4ed8]">
                  Транспорт: {item.transportCount}
                </span>
              ) : null}
              {item.educationCount != null && item.educationCount > 0 ? (
                <span className="rounded-full border border-[color:color-mix(in_srgb,#7c3aed_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#ede9fe_78%,var(--background-elevated))] px-2.5 py-1 text-[10.5px] font-bold text-[#6d28d9]">
                  Учеба: {item.educationCount}
                </span>
              ) : null}
              {item.healthcareCount != null && item.healthcareCount > 0 ? (
                <span className="rounded-full border border-[color:color-mix(in_srgb,#b42318_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#fee4e2_80%,var(--background-elevated))] px-2.5 py-1 text-[10.5px] font-bold text-[#b42318]">
                  Медицина: {item.healthcareCount}
                </span>
              ) : null}
              {transportNearest ? (
                <span className="rounded-full border border-[color:color-mix(in_srgb,#0f766e_18%,var(--border-color))] bg-[color:color-mix(in_srgb,#ecfeff_72%,var(--background-elevated))] px-2.5 py-1 text-[10.5px] font-bold text-[#155e75]">
                  До транспорта: {transportNearest}
                </span>
              ) : null}
              {bottomFeatures.map((feature) => (
                <span
                  key={feature.label}
                  className="rounded-full border border-[color:color-mix(in_srgb,var(--color-primary)_18%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_58%,var(--background-elevated))] px-2.5 py-1 text-[10.5px] font-bold text-[color:color-mix(in_srgb,var(--foreground-muted)_82%,var(--foreground))]"
                >
                  {feature.label}
                </span>
              ))}
              <span className="rounded-full border border-[color:color-mix(in_srgb,var(--color-primary)_18%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_58%,var(--background-elevated))] px-2.5 py-1 text-[10.5px] font-bold text-[color:color-mix(in_srgb,var(--foreground-muted)_82%,var(--foreground))]">
                {getBuildingTypeLabel(item.buildingType)}
              </span>
            </div>
          </div>
        </div>

        <div className={listingCardClasses.footer}>
          <span className={listingCardClasses.button}>
            Подробнее
            <MoveRight className="h-4 w-4" />
          </span>
        </div>
      </Link>
    </article>
  );
}

export function ListingCardSkeleton() {
  return (
    <div className={listingCardClasses.skeletonArticle}>
      <div className={listingCardClasses.skeletonMedia} />
      <div className={listingCardClasses.skeletonBody}>
        <div className="h-6 w-36 rounded-full bg-[color:color-mix(in_srgb,var(--color-primary-soft)_72%,var(--background-muted))]" />
        <div className="mt-2.5 h-4.5 w-5/6 rounded-full bg-[color:color-mix(in_srgb,var(--color-primary-soft)_68%,var(--background-muted))]" />
        <div className="mt-2 h-4.5 w-3/5 rounded-full bg-[color:color-mix(in_srgb,var(--color-primary-soft)_54%,var(--background-muted))]" />
        <div className="mt-2.5 h-3.5 w-4/5 rounded-full bg-[color:color-mix(in_srgb,var(--color-primary-soft)_50%,var(--background-muted))]" />
        <div className="mt-3 flex flex-wrap gap-2">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="h-7 w-28 rounded-full bg-[color:color-mix(in_srgb,var(--color-primary-soft)_60%,var(--background-muted))]" />
          ))}
        </div>
        <div className="mt-3 border-t border-[color:color-mix(in_srgb,var(--color-primary)_14%,var(--border-color))] pt-2.5">
          <div className="flex gap-2">
            {Array.from({ length: 3 }).map((_, index) => (
              <span key={index} className="h-6 w-20 rounded-full bg-[color:color-mix(in_srgb,var(--color-primary-soft)_64%,var(--background-muted))]" />
            ))}
          </div>
        </div>
      </div>
      <div className={listingCardClasses.skeletonFooter}>
        <div className={listingCardClasses.skeletonButton} />
      </div>
    </div>
  );
}
