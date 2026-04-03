"use client";

import {
  getPricingAnalyticsCategories,
  getPricingAnalyticsCities,
  getPricingAnalyticsSummary,
  getPricingAnalyticsTimeSeries,
} from "@diploma/api/pricing-analytics";
import type { PricingAnalyticsGeography, PricingAnalyticsListingType, PricingAnalyticsQuery } from "@diploma/contracts/pricing-analytics";
import { useQuery } from "@tanstack/react-query";
import { ChevronDown, Search } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { AppShell } from "../../components/AppShell";
import { useAuth } from "../../providers";
import { PricingTrendChart } from "./components/PricingTrendChart";

function formatPricePerSqm(value?: number | null) {
  if (value == null) {
    return "Нет информации";
  }

  return `${new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 0 }).format(value)} BYN/м²`;
}

function formatCount(value: number) {
  return new Intl.NumberFormat("ru-RU").format(value);
}

function formatDate(value?: string | null) {
  if (!value) {
    return "Нет информации";
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return "Нет информации";
  }

  return new Intl.DateTimeFormat("ru-BY", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    timeZone: "Europe/Minsk",
  }).format(parsed);
}

const listingTypeOptions: Array<{ value: PricingAnalyticsListingType; label: string }> = [
  { value: "sale", label: "Продажа" },
  { value: "rent", label: "Аренда" },
];

const geographyOptions: Array<{ value: PricingAnalyticsGeography; label: string }> = [
  { value: "country", label: "Вся Беларусь" },
  { value: "minsk", label: "Минск" },
  { value: "city", label: "Город" },
];

const segmentOptions = [
  { value: "", label: "Все сегменты" },
  { value: "Apartment", label: "Квартиры" },
  { value: "House", label: "Дома" },
  { value: "Office", label: "Офисы" },
  { value: "Warehouse", label: "Склады" },
  { value: "Land", label: "Участки" },
];

const fallbackCityOptions = [
  "Минск",
  "Брест",
  "Витебск",
  "Гомель",
  "Гродно",
  "Могилев",
  "Мозырь",
  "Барановичи",
  "Бобруйск",
  "Борисов",
  "Пинск",
  "Орша",
  "Новополоцк",
  "Лида",
  "Солигорск",
  "Молодечно",
  "Жлобин",
  "Слуцк",
  "Кобрин",
  "Речица",
  "Светлогорск",
  "Жодино",
  "Сморгонь",
  "Полоцк",
  "Калинковичи",
  "Волковыск",
  "Рогачев",
  "Осиповичи",
  "Несвиж",
  "Заславль",
];

function CitySelect({
  value,
  onChange,
  options,
}: {
  value: string;
  onChange: (value: string) => void;
  options: string[];
}) {
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState(value);
  const containerRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    setSearch(value);
  }, [value]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    function handlePointerDown(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    return () => document.removeEventListener("mousedown", handlePointerDown);
  }, [isOpen]);

  const filteredCities = useMemo(() => {
    const normalized = search.trim().toLowerCase();
    if (!normalized) {
      return options;
    }

    return options.filter((city) => city.toLowerCase().includes(normalized));
  }, [options, search]);

  return (
    <div ref={containerRef} style={{ display: "grid", gap: 8, position: "relative" }}>
      <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Город</span>
      <button
        type="button"
        className="field"
        onClick={() => setIsOpen((current) => !current)}
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          gap: 12,
          textAlign: "left",
          minHeight: 48,
        }}
      >
        <span style={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
          {value || "Выберите город"}
        </span>
        <ChevronDown
          style={{
            width: 16,
            height: 16,
            color: "var(--foreground-muted)",
            transform: isOpen ? "rotate(180deg)" : "rotate(0deg)",
            transition: "transform 180ms ease",
          }}
        />
      </button>

      {isOpen ? (
        <div
          className="surface"
          style={{
            position: "absolute",
            top: "calc(100% + 8px)",
            left: 0,
            right: 0,
            zIndex: 20,
            borderRadius: 18,
            padding: 12,
            display: "grid",
            gap: 10,
          }}
        >
          <div style={{ position: "relative" }}>
            <Search
              style={{
                position: "absolute",
                left: 12,
                top: "50%",
                width: 16,
                height: 16,
                transform: "translateY(-50%)",
                color: "var(--foreground-muted)",
              }}
            />
            <input
              autoFocus
              className="field"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Поиск города"
              style={{ paddingLeft: 38 }}
            />
          </div>

          <div
            style={{
              maxHeight: 220,
              overflowY: "auto",
              display: "grid",
              gap: 6,
              paddingRight: 4,
            }}
          >
            {filteredCities.length > 0 ? filteredCities.map((cityOption) => (
              <button
                key={cityOption}
                type="button"
                onClick={() => {
                  onChange(cityOption);
                  setSearch(cityOption);
                  setIsOpen(false);
                }}
                style={{
                  border: "1px solid var(--border-color)",
                  background: cityOption === value ? "var(--color-primary-soft)" : "var(--background-elevated)",
                  color: cityOption === value ? "var(--color-primary-strong)" : "var(--foreground)",
                  borderRadius: 14,
                  padding: "10px 12px",
                  textAlign: "left",
                  fontWeight: 600,
                  cursor: "pointer",
                }}
              >
                {cityOption}
              </button>
            )) : (
              <div
                style={{
                  border: "1px solid var(--border-color)",
                  background: "var(--background-elevated)",
                  borderRadius: 14,
                  padding: "12px",
                  color: "var(--foreground-muted)",
                  fontSize: 14,
                }}
              >
                Ничего не найдено
              </div>
            )}
          </div>
        </div>
      ) : null}
    </div>
  );
}

export function PricingAnalyticsClient() {
  const { api } = useAuth();
  const [listingType, setListingType] = useState<PricingAnalyticsListingType>("sale");
  const [geography, setGeography] = useState<PricingAnalyticsGeography>("country");
  const [city, setCity] = useState("Минск");
  const [listingDetailsType, setListingDetailsType] = useState("");
  const [months, setMonths] = useState(12);

  const query = useMemo<PricingAnalyticsQuery>(() => ({
    listingType,
    geography,
    city: geography === "city" ? city.trim() || null : null,
    listingDetailsType: listingDetailsType || null,
    months,
  }), [city, geography, listingDetailsType, listingType, months]);

  const summaryQuery = useQuery({
    queryKey: ["pricing-analytics", "summary", query],
    queryFn: () => getPricingAnalyticsSummary(api, query),
  });

  const timeseriesQuery = useQuery({
    queryKey: ["pricing-analytics", "timeseries", query],
    queryFn: () => getPricingAnalyticsTimeSeries(api, query),
  });

  const categoriesQuery = useQuery({
    queryKey: ["pricing-analytics", "categories", query],
    queryFn: () => getPricingAnalyticsCategories(api, query),
  });

  const citiesQuery = useQuery({
    queryKey: ["pricing-analytics", "cities"],
    queryFn: () => getPricingAnalyticsCities(api),
  });

  const summary = summaryQuery.data;
  const timeSeries = timeseriesQuery.data ?? [];
  const categories = categoriesQuery.data ?? [];
  const cityOptions = citiesQuery.data?.length ? citiesQuery.data : fallbackCityOptions;

  return (
    <AppShell>
      <main className="grid gap-6 pb-10">
        <section className="surface rounded-[32px] p-8">
          <div className="eyebrow">Аналитика</div>
          <div className="mt-2 flex flex-wrap items-start justify-between gap-4">
            <div>
              <h1 className="page-title">Ценовая аналитика</h1>
              <p style={{ margin: "0.6rem 0 0", maxWidth: 760, color: "var(--foreground-muted)", lineHeight: 1.7 }}>
                Аналитика строится по активным объявлениям и показывает цену за квадратный метр.
                При малом количестве объявлений точность ниже, поэтому сервис честно помечает слабую выборку.
              </p>
            </div>
          </div>

          <div className="mt-6 grid gap-4 lg:grid-cols-[1.1fr_1fr_1fr_0.9fr]">
            <div className="flex flex-wrap gap-2">
              {listingTypeOptions.map((option) => (
                <button
                  key={option.value}
                  type="button"
                  onClick={() => setListingType(option.value)}
                  className="button-secondary"
                  style={{
                    background: listingType === option.value ? "var(--color-primary-soft)" : undefined,
                    color: listingType === option.value ? "var(--color-primary-strong)" : undefined,
                  }}
                >
                  {option.label}
                </button>
              ))}
            </div>

            <label style={{ display: "grid", gap: 8 }}>
              <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>География</span>
              <select className="field" value={geography} onChange={(event) => setGeography(event.target.value as PricingAnalyticsGeography)}>
                {geographyOptions.map((option) => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </select>
            </label>

            <label style={{ display: "grid", gap: 8 }}>
              <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Сегмент</span>
              <select className="field" value={listingDetailsType} onChange={(event) => setListingDetailsType(event.target.value)}>
                {segmentOptions.map((option) => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </select>
            </label>

            <label style={{ display: "grid", gap: 8 }}>
              <span style={{ fontSize: 13, color: "var(--foreground-muted)" }}>Период</span>
              <select className="field" value={months} onChange={(event) => setMonths(Number(event.target.value))}>
                <option value={6}>6 месяцев</option>
                <option value={12}>12 месяцев</option>
                <option value={24}>24 месяца</option>
              </select>
            </label>
          </div>

          {geography === "city" ? (
            <div className="mt-4 max-w-[360px]">
              <CitySelect value={city} onChange={setCity} options={cityOptions} />
            </div>
          ) : null}
        </section>

        {summaryQuery.isLoading ? (
          <section className="surface rounded-[28px] p-6">Загружаем ценовую аналитику…</section>
        ) : summaryQuery.isError ? (
          <section className="surface rounded-[28px] p-6">Не удалось загрузить ценовую аналитику.</section>
        ) : summary ? (
          <>
            <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
              <div className="surface rounded-[24px] p-5">
                <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Средняя цена</div>
                <div className="mt-2 text-[1.2rem] font-black tracking-[-0.03em]">{formatPricePerSqm(summary.averagePricePerSqm)}</div>
              </div>
              <div className="surface rounded-[24px] p-5">
                <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Медиана</div>
                <div className="mt-2 text-[1.2rem] font-black tracking-[-0.03em]">{formatPricePerSqm(summary.medianPricePerSqm)}</div>
              </div>
              <div className="surface rounded-[24px] p-5">
                <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Объявлений</div>
                <div className="mt-2 text-[1.2rem] font-black tracking-[-0.03em]">{formatCount(summary.listingsCount)}</div>
              </div>
              <div className="surface rounded-[24px] p-5">
                <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Диапазон рынка</div>
                {summary.marketRangeLowPricePerSqm != null && summary.marketRangeHighPricePerSqm != null ? (
                  <div className="mt-2 grid gap-2 text-sm">
                    <div>
                      <div className="text-[11px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Минимум</div>
                      <div className="mt-1 text-[1rem] font-black tracking-[-0.03em]">{formatPricePerSqm(summary.marketRangeLowPricePerSqm)}</div>
                    </div>
                    <div>
                      <div className="text-[11px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Максимум</div>
                      <div className="mt-1 text-[1rem] font-black tracking-[-0.03em]">{formatPricePerSqm(summary.marketRangeHighPricePerSqm)}</div>
                    </div>
                  </div>
                ) : (
                  <div className="mt-2 text-[1.2rem] font-black tracking-[-0.03em]">Нет информации</div>
                )}
              </div>
              <div className="surface rounded-[24px] p-5">
                <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Надёжность</div>
                <div className="mt-2 text-[1.2rem] font-black tracking-[-0.03em]">
                  {summary.confidenceLevel === "high" ? "Высокая" : summary.confidenceLevel === "medium" ? "Средняя" : "Низкая"}
                </div>
              </div>
            </section>

            <PricingTrendChart
              title="Динамика цены за м²"
              description={summary.hasEnoughData
                ? `Обновлено: ${formatDate(summary.updatedAt)}`
                : "Данных пока мало, поэтому часть значений может не отображаться."}
              points={timeSeries.map((point) => ({
                label: point.period.slice(5),
                average: point.averagePricePerSqm,
                median: point.medianPricePerSqm,
              }))}
            />

            <section className="surface rounded-[28px] p-6">
              <div className="eyebrow">Категории</div>
              <h2 className="section-title mt-2">Срез по сегментам</h2>
              <div className="mt-5 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                {categories.map((category) => (
                  <div key={category.categoryKey} className="rounded-[24px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-5">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <div className="text-[1.1rem] font-black tracking-[-0.03em]">{category.categoryLabel}</div>
                        <div className="mt-1 text-sm text-[var(--foreground-muted)]">Объявлений: {formatCount(category.listingsCount)}</div>
                      </div>
                      <span className="rounded-full bg-[var(--background-muted)] px-3 py-1 text-[12px] font-bold">
                        {category.trendDirection === "up" ? "Рост" : category.trendDirection === "down" ? "Снижение" : "Стабильно"}
                      </span>
                    </div>
                    <div className="mt-4 grid gap-2 text-sm">
                      <div>Средняя: <strong>{formatPricePerSqm(category.averagePricePerSqm)}</strong></div>
                      <div>Медиана: <strong>{formatPricePerSqm(category.medianPricePerSqm)}</strong></div>
                      <div>Мин/макс: <strong>{formatPricePerSqm(category.minPricePerSqm)}</strong> / <strong>{formatPricePerSqm(category.maxPricePerSqm)}</strong></div>
                    </div>
                  </div>
                ))}
              </div>
            </section>
          </>
        ) : null}
      </main>
    </AppShell>
  );
}
