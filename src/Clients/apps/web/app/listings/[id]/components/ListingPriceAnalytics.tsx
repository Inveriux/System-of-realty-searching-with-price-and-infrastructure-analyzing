"use client";

import { getListingPriceComparison, getListingPriceHistory } from "@diploma/api/pricing-analytics";
import { useQuery } from "@tanstack/react-query";
import { LineChart } from "lucide-react";
import { useState } from "react";
import { useAuth } from "../../../providers";
import { ListingPriceHistoryChart } from "./ListingPriceHistoryChart";

type Props = {
  listingId: string;
  currentPriceAmountByn: number | null;
  areaTotal: number;
};

function formatPricePerSqm(value?: number | null) {
  if (value == null) {
    return "Нет информации";
  }

  return `${new Intl.NumberFormat("ru-RU", { maximumFractionDigits: 0 }).format(value)} BYN/м²`;
}

function getRelativeTone(relativePosition?: string) {
  switch (relativePosition) {
    case "below_average":
      return { background: "#e7f8ee", color: "#0f7a3b", label: "Ниже рынка" };
    case "above_average":
      return { background: "#fff1ea", color: "#b45309", label: "Выше рынка" };
    case "near_average":
      return { background: "#eef2ff", color: "#4338ca", label: "Близко к рынку" };
    default:
      return { background: "var(--background-muted)", color: "var(--foreground-muted)", label: "Нет информации" };
  }
}

export function ListingPriceAnalytics({ listingId, currentPriceAmountByn, areaTotal }: Props) {
  const { api } = useAuth();
  const [isModalOpen, setIsModalOpen] = useState(false);

  const comparisonQuery = useQuery({
    queryKey: ["pricing-analytics", "listing-comparison", listingId],
    queryFn: () => getListingPriceComparison(api, listingId),
    enabled: Boolean(listingId),
  });

  const historyQuery = useQuery({
    queryKey: ["pricing-analytics", "listing-history", listingId],
    queryFn: () => getListingPriceHistory(api, listingId),
    enabled: Boolean(listingId),
  });

  const currentPricePerSqm = currentPriceAmountByn != null && areaTotal > 0 ? currentPriceAmountByn / areaTotal : null;
  const comparison = comparisonQuery.data;
  const tone = getRelativeTone(comparison?.relativePosition);

  return (
    <section className="surface rounded-[28px] p-6">
      <div className="grid gap-4">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <div className="eyebrow">Аналитика</div>
            <h2 className="mt-2 text-[1.2rem] font-black tracking-[-0.03em]">Цена и рынок</h2>
          </div>
          <div
            className="inline-flex rounded-full px-3 py-1 text-[11px] font-black uppercase tracking-[0.12em]"
            style={{ background: tone.background, color: tone.color }}
          >
            {tone.label}
          </div>
        </div>

        {comparisonQuery.isLoading ? (
          <div className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4 text-sm">
            Загружаем аналитику…
          </div>
        ) : comparisonQuery.isError ? (
          <div className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4 text-sm">
            Не удалось загрузить ценовую аналитику.
          </div>
        ) : comparison ? (
          <>
            <div className="grid gap-3">
              <div className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4">
                <div className="text-[11px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                  Цена этого объекта
                </div>
                <div className="mt-2 text-[1.05rem] font-black tracking-[-0.03em]">
                  {formatPricePerSqm(currentPricePerSqm)}
                </div>
              </div>

              <div className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4">
                <div className="text-[11px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
                  Средняя цена для этого типа
                </div>
                <div className="mt-2 text-[1.05rem] font-black tracking-[-0.03em]">
                  {formatPricePerSqm(comparison.countryAveragePricePerSqm)}
                </div>
                <div className="mt-2 text-sm text-[var(--foreground-muted)]">
                  {comparison.message}
                </div>
              </div>
            </div>

            <button
              type="button"
              onClick={() => setIsModalOpen(true)}
              className="inline-flex items-center justify-center gap-2 rounded-[16px] border border-[var(--border-color)] bg-[var(--background-elevated)] px-4 py-3 text-sm font-semibold text-[var(--foreground)] transition hover:bg-[var(--background-muted)]"
            >
              <LineChart className="h-4 w-4 text-[var(--color-primary)]" />
              <span>Показать график и детали</span>
            </button>
          </>
        ) : null}
      </div>

      {comparison && isModalOpen ? (
        <div
          style={{
            position: "fixed",
            inset: 0,
            zIndex: 80,
            background: "color-mix(in srgb, #0f172a 56%, transparent)",
            display: "grid",
            placeItems: "center",
            padding: "24px",
          }}
          onClick={() => setIsModalOpen(false)}
        >
          <div
            className="surface"
            style={{
              width: "min(980px, 100%)",
              maxHeight: "min(88vh, 920px)",
              overflowY: "auto",
              borderRadius: 28,
              padding: 24,
            }}
            onClick={(event) => event.stopPropagation()}
          >
            <div className="flex items-start justify-between gap-4">
              <div>
                <div className="eyebrow">Аналитика</div>
                <h3 className="mt-2 text-[1.5rem] font-black tracking-[-0.04em]">График и детали цены</h3>
              </div>
              <button
                type="button"
                onClick={() => setIsModalOpen(false)}
                className="inline-flex h-10 min-w-10 items-center justify-center rounded-[14px] border border-[var(--border-color)] bg-[var(--background-elevated)] px-3 text-sm font-semibold text-[var(--foreground)] transition hover:bg-[var(--background-muted)]"
              >
                Закрыть
              </button>
            </div>

            <div className="mt-5 grid gap-4">
              <div className="grid gap-3 md:grid-cols-3">
                {[
                  { label: "Рядом 1.5 км", value: formatPricePerSqm(comparison.localRadiusAveragePricePerSqm) },
                  { label: "По городу", value: formatPricePerSqm(comparison.cityAveragePricePerSqm) },
                  { label: "По стране", value: formatPricePerSqm(comparison.countryAveragePricePerSqm) },
                ].map((item) => (
                  <div key={item.label} className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4">
                    <div className="text-[11px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">{item.label}</div>
                    <div className="mt-2 text-[1rem] font-black tracking-[-0.03em]">{item.value}</div>
                  </div>
                ))}
              </div>

              <div className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4">
                <div className="grid gap-2 text-sm">
                  <div>{comparison.message}</div>
                  <div>Объектов рядом: <strong>{comparison.localRadiusListingsCount}</strong></div>
                  <div>Объектов по городу: <strong>{comparison.cityListingsCount}</strong></div>
                  <div>Объектов по стране: <strong>{comparison.countryListingsCount}</strong></div>
                  <div>Надёжность: <strong>{comparison.confidenceLevel === "high" ? "высокая" : comparison.confidenceLevel === "medium" ? "средняя" : "низкая"}</strong></div>
                  {comparison.marketRangeLowPricePerSqm != null && comparison.marketRangeHighPricePerSqm != null ? (
                    <div>
                      Диапазон рынка: <strong>{formatPricePerSqm(comparison.marketRangeLowPricePerSqm)}</strong> – <strong>{formatPricePerSqm(comparison.marketRangeHighPricePerSqm)}</strong>
                    </div>
                  ) : null}
                </div>
              </div>

              {historyQuery.isLoading ? (
                <div className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4 text-sm">
                  Загружаем историю цены…
                </div>
              ) : historyQuery.isError ? (
                <div className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4 text-sm">
                  Не удалось загрузить историю цены.
                </div>
              ) : historyQuery.data ? (
                <ListingPriceHistoryChart history={historyQuery.data} />
              ) : (
                <div className="rounded-[20px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-4 text-sm">
                  История изменения цены отсутствует.
                </div>
              )}
            </div>
          </div>
        </div>
      ) : null}
    </section>
  );
}
