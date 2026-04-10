import type { CSSProperties } from "react";

export const listingCardClasses = {
  article:
    "group flex h-full flex-col overflow-hidden rounded-[22px] sm:rounded-[28px] border border-[color:color-mix(in_srgb,var(--border-color)_90%,transparent)] bg-[color:color-mix(in_srgb,var(--background-elevated)_96%,var(--background-muted))] shadow-[0_14px_30px_var(--shadow-color)] transition duration-300 ease-out hover:-translate-y-1 hover:shadow-[0_20px_40px_var(--shadow-color)]",
  media:
    "relative aspect-[16/7.2] overflow-hidden bg-[color:color-mix(in_srgb,var(--background-muted)_92%,var(--background-elevated))]",
  body: "flex flex-1 flex-col px-4 pt-3 sm:px-4.5 sm:pt-3.5",
  footer: "px-4 pt-3 pb-3 sm:px-4.5",
  button:
    "inline-flex w-full items-center justify-center gap-2 rounded-[14px] border border-[color:color-mix(in_srgb,var(--color-primary)_44%,var(--border-color))] bg-[color:color-mix(in_srgb,var(--color-primary-active)_82%,var(--background-elevated))] px-4 py-2.5 text-[0.95rem] font-semibold text-[color:var(--text-on-accent)] shadow-[0_14px_26px_color-mix(in_srgb,var(--shadow-color)_74%,transparent)] transition group-hover:translate-y-[-1px]",
  skeletonArticle:
    "flex h-full animate-pulse flex-col overflow-hidden rounded-[28px] border border-[color:color-mix(in_srgb,var(--border-color)_90%,transparent)] bg-[color:color-mix(in_srgb,var(--background-elevated)_96%,var(--background-muted))] shadow-[0_14px_30px_var(--shadow-color)]",
  skeletonMedia:
    "aspect-[16/7.2] bg-[color:color-mix(in_srgb,var(--background-muted)_92%,var(--background-elevated))]",
  skeletonBody: "flex flex-1 flex-col px-4.5 pt-3.5",
  skeletonFooter: "px-4.5 pt-3 pb-3",
  skeletonButton:
    "h-11 rounded-[14px] bg-[color:color-mix(in_srgb,var(--color-primary-active)_82%,var(--background-elevated))]",
  priceRow:
    "flex min-w-0 flex-wrap items-center gap-x-1.5 gap-y-1 text-[1.16rem] sm:text-[1.28rem] xl:text-[1.38rem] leading-none font-bold tracking-[-0.05em] text-[color:var(--color-primary-strong)]",
  title:
    "mt-0.5 line-clamp-2 text-[1.35rem] sm:text-[1.55rem] xl:text-[1.9rem] leading-[1.02] font-black tracking-[-0.05em] text-[color:var(--foreground)]",
  meterPrices: "flex flex-col items-end gap-1 text-right",
  meterPrice:
    "inline-flex items-center gap-1 text-[0.78rem] font-semibold tracking-[-0.01em] text-[color:var(--foreground-muted)]",
  bynIcon: "inline-block h-[0.9em] w-[0.72em] shrink-0 bg-current align-[-0.08em]",
  bynMeterIcon: "inline-block h-[0.95em] w-[0.72em] shrink-0 bg-current align-[-0.08em]",
  usdIcon: "inline-block h-[1.08em] w-[0.78em] shrink-0 bg-current align-[-0.1em]",
  usdMeterIcon: "inline-block h-[1em] w-[0.78em] shrink-0 bg-current align-[-0.1em]",
} as const;

export const bynIconStyle: CSSProperties = {
  WebkitMaskImage: "url('/byn.svg')",
  maskImage: "url('/byn.svg')",
  WebkitMaskRepeat: "no-repeat",
  maskRepeat: "no-repeat",
  WebkitMaskPosition: "center",
  maskPosition: "center",
  WebkitMaskSize: "contain",
  maskSize: "contain",
};

export const dollarIconStyle: CSSProperties = {
  WebkitMaskImage: "url('/dollar.svg')",
  maskImage: "url('/dollar.svg')",
  WebkitMaskRepeat: "no-repeat",
  maskRepeat: "no-repeat",
  WebkitMaskPosition: "center",
  maskPosition: "center",
  WebkitMaskSize: "108% 108%",
  maskSize: "108% 108%",
};
