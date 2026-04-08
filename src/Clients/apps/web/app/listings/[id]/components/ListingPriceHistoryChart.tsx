"use client";

import type { ListingPriceHistory } from "@diploma/contracts/pricing-analytics";
import { useEffect, useState } from "react";

type Props = {
  history: ListingPriceHistory;
};

function formatPrice(value?: number | null) {
  if (value == null) {
    return "Нет информации";
  }

  return `${new Intl.NumberFormat("ru-RU", {
    minimumFractionDigits: 0,
    maximumFractionDigits: Number.isInteger(value) ? 0 : 2,
  }).format(value)} BYN`;
}

function formatDate(value?: string | null, options?: Intl.DateTimeFormatOptions) {
  if (!value) {
    return "Нет информации";
  }

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return "Нет информации";
  }

  return new Intl.DateTimeFormat("ru-BY", {
    timeZone: "Europe/Minsk",
    ...options,
  }).format(parsed);
}

export function ListingPriceHistoryChart({ history }: Props) {
  const [isMobileViewport, setIsMobileViewport] = useState(false);

  useEffect(() => {
    const updateViewport = () => setIsMobileViewport(window.innerWidth < 768);

    updateViewport();
    window.addEventListener("resize", updateViewport);

    return () => window.removeEventListener("resize", updateViewport);
  }, []);

  if (history.history.length === 0) {
    return (
      <div className="rounded-[24px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-5 text-sm text-[var(--foreground-muted)]">
        История изменения цены отсутствует.
      </div>
    );
  }

  const points = history.history;
  const values = points.map((point) => point.priceAmountByn).filter((value): value is number => value != null);

  if (values.length === 0) {
    return null;
  }

  const width = 980;
  const height = 340;
  const paddingTop = 20;
  const paddingRight = 24;
  const paddingBottom = isMobileViewport ? 64 : 54;
  const paddingLeft = isMobileViewport ? 160 : 84;
  const minValue = Math.min(...values);
  const maxValue = Math.max(...values);
  const range = Math.max(maxValue - minValue, 1);
  const chartWidth = width - paddingLeft - paddingRight;
  const chartHeight = height - paddingTop - paddingBottom;
  const stepX = points.length > 1 ? chartWidth / (points.length - 1) : 0;
  const axisFontSize = isMobileViewport ? 24 : 11;

  const normalized = points
    .map((point, index) => point.priceAmountByn == null ? null : ({
      x: paddingLeft + index * stepX,
      y: paddingTop + ((maxValue - point.priceAmountByn) / range) * chartHeight,
    }))
    .filter((value): value is { x: number; y: number } => value != null);

  const path = normalized
    .map((point, index) => `${index === 0 ? "M" : "L"} ${point.x.toFixed(2)} ${point.y.toFixed(2)}`)
    .join(" ");

  return (
    <div className="listing-price-history-chart grid gap-4 rounded-[24px] border border-[var(--border-color)] bg-[var(--background-elevated)] p-5" style={{ minWidth: 0, maxWidth: "100%" }}>
      <div className="grid gap-2 sm:grid-cols-4">
        <div>
          <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Текущая цена</div>
          <div className="mt-1 text-[1rem] font-black" style={{ overflowWrap: "anywhere" }}>{formatPrice(history.currentPriceAmountByn)}</div>
        </div>
        <div>
          <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Первая цена</div>
          <div className="mt-1 text-[1rem] font-black" style={{ overflowWrap: "anywhere" }}>{formatPrice(history.firstRecordedPriceAmountByn)}</div>
        </div>
        <div>
          <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Изменение</div>
          <div className="mt-1 text-[1rem] font-black" style={{ overflowWrap: "anywhere" }}>
            {history.percentChangeFromFirst == null ? "Нет информации" : `${history.percentChangeFromFirst > 0 ? "+" : ""}${history.percentChangeFromFirst}%`}
          </div>
        </div>
        <div>
          <div className="text-[12px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">Последнее обновление</div>
          <div className="mt-1 text-[1rem] font-black" style={{ overflowWrap: "anywhere" }}>
            {formatDate(history.lastRecordedAt, { day: "2-digit", month: "2-digit", year: "numeric" })}
          </div>
        </div>
      </div>

      <div className="overflow-hidden" style={{ minWidth: 0 }}>
        <svg
          viewBox={`0 0 ${width} ${height}`}
          style={{ width: "100%", minWidth: 0, maxWidth: "100%", height: "auto", display: "block" }}
        >
          {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
            const y = paddingTop + chartHeight * ratio;
            const value = maxValue - (maxValue - minValue) * ratio;
            return (
              <g key={ratio}>
                <line
                  x1={paddingLeft}
                  y1={y}
                  x2={width - paddingRight}
                  y2={y}
                  stroke="color-mix(in srgb, var(--border-color) 82%, transparent)"
                  strokeWidth="1"
                />
                <text x={12} y={y + 4} fill="var(--foreground-muted)" fontSize={axisFontSize}>
                  {Math.round(value)}
                </text>
              </g>
            );
          })}
          {points.map((point, index) => {
            const x = paddingLeft + index * stepX;
            return (
              <line
                key={`${point.id}-grid`}
                x1={x}
                y1={paddingTop}
                x2={x}
                y2={paddingTop + chartHeight}
                stroke="color-mix(in srgb, var(--border-color) 44%, transparent)"
                strokeWidth="1"
              />
            );
          })}
          <line
            x1={paddingLeft}
            y1={paddingTop}
            x2={paddingLeft}
            y2={paddingTop + chartHeight}
            stroke="color-mix(in srgb, var(--foreground) 34%, transparent)"
            strokeWidth="1.4"
          />
          <line
            x1={paddingLeft}
            y1={paddingTop + chartHeight}
            x2={width - paddingRight}
            y2={paddingTop + chartHeight}
            stroke="color-mix(in srgb, var(--foreground) 34%, transparent)"
            strokeWidth="1.4"
          />
          {points.map((point, index) => (
            <text
              key={point.id}
              x={paddingLeft + index * stepX}
              y={height - 18}
              textAnchor="middle"
              fill="var(--foreground-muted)"
              fontSize={axisFontSize}
            >
              {formatDate(point.effectiveDateUtc, { month: "2-digit", year: "2-digit" })}
            </text>
          ))}
          {normalized.length > 1 ? (
            <path d={path} fill="none" stroke="var(--chart-primary)" strokeWidth="3" strokeLinecap="round" />
          ) : null}
          {normalized.map((point, index) => (
            <circle
              key={index}
              cx={point.x}
              cy={point.y}
              r="4.5"
              fill="var(--chart-primary)"
              stroke="color-mix(in srgb, var(--background-elevated) 92%, white)"
              strokeWidth="2"
            />
          ))}
        </svg>
      </div>

      <div className="grid gap-2">
        {history.history.slice().reverse().map((point) => (
          <div key={point.id} className="flex flex-wrap items-center justify-between gap-3 rounded-[18px] bg-[var(--background-muted)] px-4 py-3 text-sm">
            <div className="font-semibold">
              {formatDate(point.effectiveDateUtc, {
                day: "2-digit",
                month: "2-digit",
                year: "numeric",
              })}
            </div>
            <div className="text-[var(--foreground-muted)]" style={{ minWidth: 0, overflowWrap: "anywhere" }}>
              {formatPrice(point.priceAmountByn)}
              {point.pricePerSqmByn != null ? ` • ${Math.round(point.pricePerSqmByn)} BYN/м²` : ""}
            </div>
            {point.comment ? <div className="w-full text-[var(--foreground-muted)]">{point.comment}</div> : null}
          </div>
        ))}
      </div>
    </div>
  );
}
