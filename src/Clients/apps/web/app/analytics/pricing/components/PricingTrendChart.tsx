"use client";

import { useMemo, useState } from "react";

type Point = {
  label: string;
  average: number | null;
  median: number | null;
};

type Props = {
  title: string;
  description?: string;
  points: Point[];
};

function formatValue(value: number | null) {
  if (value == null) {
    return "Нет данных";
  }

  return `${new Intl.NumberFormat("ru-RU", {
    maximumFractionDigits: 0,
  }).format(value)} BYN/м²`;
}

function buildPath(points: Array<{ x: number; y: number }>) {
  return points
    .map((point, index) => `${index === 0 ? "M" : "L"} ${point.x.toFixed(2)} ${point.y.toFixed(2)}`)
    .join(" ");
}

export function PricingTrendChart({ title, description, points }: Props) {
  const [activeIndex, setActiveIndex] = useState<number | null>(null);
  const values = points.flatMap((point) => [point.average, point.median]).filter((value): value is number => value != null);

  if (values.length === 0) {
    return (
      <div className="surface rounded-[28px] p-6">
        <div className="eyebrow">Динамика</div>
        <h3 className="section-title mt-2">{title}</h3>
        <p style={{ margin: "0.5rem 0 0", color: "var(--foreground-muted)" }}>
          {description ?? "Недостаточно данных для построения графика."}
        </p>
      </div>
    );
  }

  const width = 980;
  const height = 340;
  const paddingTop = 20;
  const paddingRight = 24;
  const paddingBottom = 54;
  const paddingLeft = 84;
  const minValue = Math.min(...values);
  const maxValue = Math.max(...values);
  const range = Math.max(maxValue - minValue, 1);
  const chartWidth = width - paddingLeft - paddingRight;
  const chartHeight = height - paddingTop - paddingBottom;
  const xStep = points.length > 1 ? chartWidth / (points.length - 1) : 0;
  const toY = (value: number) => paddingTop + ((maxValue - value) / range) * chartHeight;

  const averagePoints = points
    .map((point, index) => point.average == null ? null : ({ x: paddingLeft + xStep * index, y: toY(point.average) }))
    .filter((value): value is { x: number; y: number } => value != null);
  const medianPoints = points
    .map((point, index) => point.median == null ? null : ({ x: paddingLeft + xStep * index, y: toY(point.median) }))
    .filter((value): value is { x: number; y: number } => value != null);
  const activePoint = activeIndex != null ? points[activeIndex] : null;
  const activeX = activeIndex != null ? paddingLeft + xStep * activeIndex : null;
  const tooltipLeft = activeX == null
    ? 0
    : Math.min(Math.max(((activeX / width) * 100), 14), 86);
  const axisUnitLabels = useMemo(() => ["BYN/м²", "цена за м²"], []);

  return (
    <div className="surface rounded-[28px] p-6">
      <div className="eyebrow">Динамика</div>
      <div className="mt-2 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h3 className="section-title">{title}</h3>
          {description ? (
            <p style={{ margin: "0.35rem 0 0", color: "var(--foreground-muted)" }}>{description}</p>
          ) : null}
        </div>
        <div className="flex flex-wrap gap-3 text-sm font-semibold">
          <span className="inline-flex items-center gap-2 rounded-full border border-[var(--border-color)] bg-[var(--background-elevated)] px-3 py-1 text-[var(--foreground-muted)]">
            {axisUnitLabels[0]}
          </span>
          <span className="inline-flex items-center gap-2">
            <span style={{ width: 10, height: 10, borderRadius: 999, background: "#6d28d9" }} />
            Средняя
          </span>
          <span className="inline-flex items-center gap-2">
            <span style={{ width: 10, height: 10, borderRadius: 999, background: "#059669" }} />
            Медиана
          </span>
        </div>
      </div>

      <div className="relative mt-5 overflow-x-auto">
        {activePoint ? (
          <div
            className="pointer-events-none absolute top-2 z-10 -translate-x-1/2 rounded-2xl border border-[var(--border-color)] bg-[color:color-mix(in_srgb,var(--background-elevated)_96%,white)] px-4 py-3 text-sm shadow-[0_18px_50px_var(--shadow-color)]"
            style={{ left: `${tooltipLeft}%` }}
          >
            <div className="text-[11px] font-semibold uppercase tracking-[0.12em] text-[var(--foreground-muted)]">
              {activePoint.label}
            </div>
            <div className="mt-2 grid gap-1.5">
              <div className="flex items-center gap-2">
                <span style={{ width: 10, height: 10, borderRadius: 999, background: "#6d28d9" }} />
                <span>Средняя: {formatValue(activePoint.average)}</span>
              </div>
              <div className="flex items-center gap-2">
                <span style={{ width: 10, height: 10, borderRadius: 999, background: "#059669" }} />
                <span>Медиана: {formatValue(activePoint.median)}</span>
              </div>
            </div>
          </div>
        ) : null}
        <svg viewBox={`0 0 ${width} ${height}`} style={{ width: "100%", minWidth: 720, height: "auto", display: "block" }}>
          {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
            const y = paddingTop + chartHeight * ratio;
            const value = maxValue - range * ratio;
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
                <text x={12} y={y + 4} fill="var(--foreground-muted)" fontSize="11">
                  {Math.round(value)} BYN
                </text>
              </g>
            );
          })}
          {points.map((point, index) => {
            const x = paddingLeft + xStep * index;
            return (
              <g
                key={`${point.label}-grid`}
                onMouseEnter={() => setActiveIndex(index)}
                onMouseLeave={() => setActiveIndex((current) => current === index ? null : current)}
              >
                <line
                  x1={x}
                  y1={paddingTop}
                  x2={x}
                  y2={paddingTop + chartHeight}
                  stroke={activeIndex === index
                    ? "color-mix(in srgb, var(--color-primary) 48%, transparent)"
                    : "color-mix(in srgb, var(--border-color) 44%, transparent)"}
                  strokeWidth={activeIndex === index ? "1.6" : "1"}
                />
                <rect
                  x={x - Math.max(xStep / 2, 22)}
                  y={paddingTop}
                  width={Math.max(xStep, 44)}
                  height={chartHeight}
                  fill="transparent"
                />
              </g>
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

          {points.map((point, index) => {
            const x = paddingLeft + xStep * index;
            return (
              <text
                key={point.label}
                x={x}
                y={height - 18}
                textAnchor="middle"
                fill="var(--foreground-muted)"
                fontSize="11"
              >
                {point.label}
              </text>
            );
          })}

          {averagePoints.length > 1 ? (
            <path d={buildPath(averagePoints)} fill="none" stroke="#6d28d9" strokeWidth="3" strokeLinecap="round" />
          ) : null}
          {medianPoints.length > 1 ? (
            <path d={buildPath(medianPoints)} fill="none" stroke="#059669" strokeWidth="3" strokeLinecap="round" />
          ) : null}

          {averagePoints.map((point, index) => (
            <circle
              key={`avg-${index}`}
              cx={point.x}
              cy={point.y}
              r={activeIndex === index ? "6" : "4.5"}
              fill="#6d28d9"
              stroke="white"
              strokeWidth="2"
              onMouseEnter={() => setActiveIndex(index)}
            />
          ))}
          {medianPoints.map((point, index) => (
            <circle
              key={`med-${index}`}
              cx={point.x}
              cy={point.y}
              r={activeIndex === index ? "6" : "4.5"}
              fill="#059669"
              stroke="white"
              strokeWidth="2"
              onMouseEnter={() => setActiveIndex(index)}
            />
          ))}
        </svg>
      </div>
    </div>
  );
}
