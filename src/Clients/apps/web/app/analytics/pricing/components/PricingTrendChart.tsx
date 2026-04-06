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

  const desktopWidth = 980;
  const mobileWidth = Math.max(620, points.length * 58);
  const desktopHeight = 340;
  const mobileHeight = 308;
  const minValue = Math.min(...values);
  const maxValue = Math.max(...values);
  const chartTopHeadroom = Math.max((maxValue - minValue) * 0.14, maxValue * 0.04, 1);
  const adjustedMaxValue = maxValue + chartTopHeadroom;
  const range = Math.max(adjustedMaxValue - minValue, 1);
  const desktopPaddingTop = 20;
  const desktopPaddingRight = 24;
  const desktopPaddingBottom = 54;
  const desktopPaddingLeft = 84;
  const mobilePaddingTop = 10;
  const mobilePaddingRight = 18;
  const mobilePaddingBottom = 36;
  const mobilePaddingLeft = 64;
  const mobileAxisWidth = 44;
  const mobileEdgePointInset = 54;
  const mobileScrollableWidth = mobileWidth - mobilePaddingLeft;
  const desktopChartWidth = desktopWidth - desktopPaddingLeft - desktopPaddingRight;
  const desktopChartHeight = desktopHeight - desktopPaddingTop - desktopPaddingBottom;
  const mobileChartWidth = mobileScrollableWidth - mobilePaddingRight - mobileEdgePointInset * 2;
  const mobileChartHeight = mobileHeight - mobilePaddingTop - mobilePaddingBottom;
  const desktopXStep = points.length > 1 ? desktopChartWidth / (points.length - 1) : 0;
  const mobileXStep = points.length > 1 ? mobileChartWidth / (points.length - 1) : 0;
  const toDesktopY = (value: number) => desktopPaddingTop + ((adjustedMaxValue - value) / range) * desktopChartHeight;
  const toMobileY = (value: number) => mobilePaddingTop + ((adjustedMaxValue - value) / range) * mobileChartHeight;
  const getMobileX = (index: number) => {
    if (points.length <= 1) {
      return mobileEdgePointInset;
    }

    return mobileEdgePointInset + mobileXStep * index;
  };

  const averagePoints = points
    .map((point, index) => point.average == null ? null : ({ x: desktopPaddingLeft + desktopXStep * index, y: toDesktopY(point.average) }))
    .filter((value): value is { x: number; y: number } => value != null);
  const medianPoints = points
    .map((point, index) => point.median == null ? null : ({ x: desktopPaddingLeft + desktopXStep * index, y: toDesktopY(point.median) }))
    .filter((value): value is { x: number; y: number } => value != null);
  const mobileAveragePoints = points
    .map((point, index) => point.average == null ? null : ({ x: mobilePaddingLeft + getMobileX(index), y: toMobileY(point.average) }))
    .filter((value): value is { x: number; y: number } => value != null);
  const mobileMedianPoints = points
    .map((point, index) => point.median == null ? null : ({ x: mobilePaddingLeft + getMobileX(index), y: toMobileY(point.median) }))
    .filter((value): value is { x: number; y: number } => value != null);
  const activePoint = activeIndex != null ? points[activeIndex] : null;
  const activeMobileIndex = activeIndex ?? 0;
  const activeDesktopX = activeIndex != null ? desktopPaddingLeft + desktopXStep * activeIndex : null;
  const tooltipLeft = activeDesktopX == null
    ? 0
    : Math.min(Math.max(((activeDesktopX / desktopWidth) * 100), 14), 86);
  const axisUnitLabels = useMemo(() => ["BYN/м²", "цена за м²"], []);

  return (
    <div className="surface rounded-[28px] p-6" style={{ minWidth: 0, width: "100%", maxWidth: "100%", overflow: "hidden" }}>
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

      <div className="mt-5 sm:hidden" style={{ minWidth: 0, width: "100%", maxWidth: "100%", overflow: "visible", paddingTop: 28 }}>
        <div
          onClick={() => setActiveIndex(null)}
          onTouchStart={() => setActiveIndex(null)}
          style={{ display: "grid", gridTemplateColumns: `${mobileAxisWidth}px minmax(0, 1fr)`, alignItems: "start", overflow: "visible" }}
        >
          <svg
            viewBox={`0 0 ${mobileAxisWidth} ${mobileHeight}`}
            style={{ width: mobileAxisWidth, minWidth: mobileAxisWidth, height: "auto", display: "block", overflow: "visible" }}
          >
            {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
              const y = mobilePaddingTop + mobileChartHeight * ratio;
              const value = adjustedMaxValue - range * ratio;
              return (
                <g key={ratio}>
                  <text x={4} y={y + 4} fill="var(--foreground-muted)" fontSize="10.5" fontWeight="700">
                    {Math.round(value)}
                  </text>
                </g>
              );
            })}
            <line
              x1={mobileAxisWidth - 1}
              y1={mobilePaddingTop}
              x2={mobileAxisWidth - 1}
              y2={mobilePaddingTop + mobileChartHeight}
              stroke="color-mix(in srgb, var(--foreground) 34%, transparent)"
              strokeWidth="1.4"
            />
          </svg>
          <div
            className="overflow-x-auto"
            style={{
              minWidth: 0,
              width: "100%",
              maxWidth: "100%",
              overflowY: "hidden",
              WebkitOverflowScrolling: "touch",
              overscrollBehaviorX: "contain",
              overflowX: "auto",
              position: "relative",
              zIndex: 2,
            }}
          >
            <div style={{ position: "relative", width: mobileScrollableWidth, minWidth: mobileScrollableWidth }}>
              {activePoint ? (
                <div
                  className="pointer-events-none absolute z-10 -translate-x-1/2 rounded-xl border border-[var(--border-color)] bg-[color:color-mix(in_srgb,var(--background-elevated)_96%,white)] px-3 py-2 text-xs shadow-[0_18px_50px_var(--shadow-color)]"
                  style={{
                    top: 4,
                    left: Math.min(Math.max(getMobileX(activeMobileIndex), 76), mobileScrollableWidth - 76),
                    width: 144,
                    maxWidth: "calc(100vw - 40px)",
                  }}
                >
                  <div className="text-[10px] font-semibold uppercase tracking-[0.08em] text-[var(--foreground-muted)]">
                    {activePoint.label}
                  </div>
                  <div className="mt-1.5 grid gap-1">
                    <div>Дата: {activePoint.label}</div>
                    <div className="flex items-center gap-2">
                      <span style={{ width: 8, height: 8, borderRadius: 999, background: "#6d28d9" }} />
                      <span>Средняя: {formatValue(activePoint.average)}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <span style={{ width: 8, height: 8, borderRadius: 999, background: "#059669" }} />
                      <span>Медиана: {formatValue(activePoint.median)}</span>
                    </div>
                  </div>
                </div>
              ) : null}
              <svg viewBox={`0 0 ${mobileScrollableWidth} ${mobileHeight}`} style={{ width: mobileScrollableWidth, minWidth: mobileScrollableWidth, height: "auto", display: "block" }}>
                {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
                  const y = mobilePaddingTop + mobileChartHeight * ratio;
                  return (
                    <g key={ratio}>
                      <line
                        x1={0}
                        y1={y}
                        x2={mobileScrollableWidth - mobilePaddingRight}
                        y2={y}
                        stroke="color-mix(in srgb, var(--border-color) 82%, transparent)"
                        strokeWidth="1"
                      />
                    </g>
                  );
                })}
                <line
                  x1={0}
                  y1={mobilePaddingTop + mobileChartHeight}
                  x2={mobileScrollableWidth - mobilePaddingRight}
                  y2={mobilePaddingTop + mobileChartHeight}
                  stroke="color-mix(in srgb, var(--foreground) 34%, transparent)"
                  strokeWidth="1.4"
                />
                {points.map((point, index) => {
                  const x = getMobileX(index);
                  return (
                    <g
                      key={`${point.label}-mobile-grid`}
                      onClick={(event) => {
                        event.stopPropagation();
                        setActiveIndex(index);
                      }}
                      onTouchStart={(event) => {
                        event.stopPropagation();
                        setActiveIndex(index);
                      }}
                    >
                      <line
                        x1={x}
                        y1={mobilePaddingTop}
                        x2={x}
                        y2={mobilePaddingTop + mobileChartHeight}
                        stroke={activeIndex === index
                          ? "color-mix(in srgb, var(--color-primary) 48%, transparent)"
                          : "color-mix(in srgb, var(--border-color) 44%, transparent)"}
                        strokeWidth={activeIndex === index ? "1.6" : "1"}
                      />
                      <rect
                        x={x - Math.max(mobileXStep / 2, 26)}
                        y={mobilePaddingTop}
                        width={Math.max(mobileXStep, 52)}
                        height={mobileChartHeight}
                        fill="transparent"
                      />
                    </g>
                  );
                })}
                {points.map((point, index) => (
                  <text
                    key={`mobile-${point.label}`}
                    x={getMobileX(index)}
                    y={mobileHeight - 12}
                    textAnchor="middle"
                    fill="var(--foreground-muted)"
                    fontSize="10.5"
                    fontWeight="700"
                  >
                    {point.label}
                  </text>
                ))}
                {mobileAveragePoints.length > 1 ? (
                  <path d={buildPath(mobileAveragePoints.map((point) => ({ x: point.x - mobilePaddingLeft, y: point.y })))} fill="none" stroke="#6d28d9" strokeWidth="2.5" strokeLinecap="round" />
                ) : null}
                {mobileMedianPoints.length > 1 ? (
                  <path d={buildPath(mobileMedianPoints.map((point) => ({ x: point.x - mobilePaddingLeft, y: point.y })))} fill="none" stroke="#059669" strokeWidth="2.5" strokeLinecap="round" />
                ) : null}
                {mobileAveragePoints.map((point, index) => (
                  <circle
                    key={`mobile-avg-${index}`}
                    cx={point.x - mobilePaddingLeft}
                    cy={point.y}
                    r={activeIndex === index ? "6" : "4.5"}
                    fill="#6d28d9"
                    stroke="white"
                    strokeWidth="2"
                    onClick={(event) => {
                      event.stopPropagation();
                      setActiveIndex(index);
                    }}
                    onTouchStart={(event) => {
                      event.stopPropagation();
                      setActiveIndex(index);
                    }}
                  />
                ))}
                {mobileMedianPoints.map((point, index) => (
                  <circle
                    key={`mobile-med-${index}`}
                    cx={point.x - mobilePaddingLeft}
                    cy={point.y}
                    r={activeIndex === index ? "6" : "4.5"}
                    fill="#059669"
                    stroke="white"
                    strokeWidth="2"
                    onClick={(event) => {
                      event.stopPropagation();
                      setActiveIndex(index);
                    }}
                    onTouchStart={(event) => {
                      event.stopPropagation();
                      setActiveIndex(index);
                    }}
                  />
                ))}
              </svg>
            </div>
          </div>
        </div>
      </div>

      <div className="relative mt-5 hidden overflow-hidden sm:block">
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
        <svg viewBox={`0 0 ${desktopWidth} ${desktopHeight}`} style={{ width: "100%", minWidth: 0, maxWidth: "100%", height: "auto", display: "block" }}>
          {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
            const y = desktopPaddingTop + desktopChartHeight * ratio;
            const value = adjustedMaxValue - range * ratio;
            return (
              <g key={ratio}>
                <line
                  x1={desktopPaddingLeft}
                  y1={y}
                  x2={desktopWidth - desktopPaddingRight}
                  y2={y}
                  stroke="color-mix(in srgb, var(--border-color) 82%, transparent)"
                  strokeWidth="1"
                />
                <text x={12} y={y + 4} fill="var(--foreground-muted)" fontSize="13" fontWeight="700">
                  {Math.round(value)} BYN
                </text>
              </g>
            );
          })}
          {points.map((point, index) => {
            const x = desktopPaddingLeft + desktopXStep * index;
            return (
              <g
                key={`${point.label}-grid`}
                onMouseEnter={() => setActiveIndex(index)}
                onMouseLeave={() => setActiveIndex((current) => current === index ? null : current)}
              >
                <line
                  x1={x}
                  y1={desktopPaddingTop}
                  x2={x}
                  y2={desktopPaddingTop + desktopChartHeight}
                  stroke={activeIndex === index
                    ? "color-mix(in srgb, var(--color-primary) 48%, transparent)"
                    : "color-mix(in srgb, var(--border-color) 44%, transparent)"}
                  strokeWidth={activeIndex === index ? "1.6" : "1"}
                />
                <rect
                  x={x - Math.max(desktopXStep / 2, 22)}
                  y={desktopPaddingTop}
                  width={Math.max(desktopXStep, 44)}
                  height={desktopChartHeight}
                  fill="transparent"
                />
              </g>
            );
          })}
          <line
            x1={desktopPaddingLeft}
            y1={desktopPaddingTop}
            x2={desktopPaddingLeft}
            y2={desktopPaddingTop + desktopChartHeight}
            stroke="color-mix(in srgb, var(--foreground) 34%, transparent)"
            strokeWidth="1.4"
          />
          <line
            x1={desktopPaddingLeft}
            y1={desktopPaddingTop + desktopChartHeight}
            x2={desktopWidth - desktopPaddingRight}
            y2={desktopPaddingTop + desktopChartHeight}
            stroke="color-mix(in srgb, var(--foreground) 34%, transparent)"
            strokeWidth="1.4"
          />

          {points.map((point, index) => {
            const x = desktopPaddingLeft + desktopXStep * index;
            return (
              <text
                key={point.label}
                x={x}
                y={desktopHeight - 18}
                textAnchor="middle"
                fill="var(--foreground-muted)"
                fontSize="13"
                fontWeight="700"
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
