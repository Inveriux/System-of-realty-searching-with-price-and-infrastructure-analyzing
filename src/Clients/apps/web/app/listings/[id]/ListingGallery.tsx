"use client";

import Image from "next/image";
import { useRef, useState } from "react";

type Props = {
  title: string;
  media: string[];
};

export function ListingGallery({ title, media }: Props) {
  const [activeIndex, setActiveIndex] = useState(0);
  const touchStartRef = useRef<{ x: number; y: number } | null>(null);
  const activeImage = media[activeIndex] ?? media[0];
  const hasMultiple = media.length > 1;

  function showPrevious() {
    setActiveIndex((current) => (current === 0 ? media.length - 1 : current - 1));
  }

  function showNext() {
    setActiveIndex((current) => (current === media.length - 1 ? 0 : current + 1));
  }

  function handleTouchStart(event: React.TouchEvent<HTMLDivElement>) {
    const touch = event.touches[0];
    if (!touch) return;
    touchStartRef.current = { x: touch.clientX, y: touch.clientY };
  }

  function handleTouchEnd(event: React.TouchEvent<HTMLDivElement>) {
    if (!hasMultiple || !touchStartRef.current) {
      touchStartRef.current = null;
      return;
    }

    const touch = event.changedTouches[0];
    if (!touch) {
      touchStartRef.current = null;
      return;
    }

    const deltaX = touch.clientX - touchStartRef.current.x;
    const deltaY = touch.clientY - touchStartRef.current.y;
    touchStartRef.current = null;

    if (Math.abs(deltaX) < 36 || Math.abs(deltaX) <= Math.abs(deltaY)) {
      return;
    }

    if (deltaX < 0) {
      showNext();
      return;
    }

    showPrevious();
  }

  return (
    <div className="listing-gallery grid gap-2">
      <div
        className="listing-gallery-stage group relative aspect-[16/8.5] overflow-hidden rounded-[32px] bg-[var(--background-muted)]"
        onTouchStart={handleTouchStart}
        onTouchEnd={handleTouchEnd}
      >
        <Image
          src={activeImage}
          alt={title}
          fill
          unoptimized
          className="object-cover"
          sizes="(min-width: 1280px) 70vw, 100vw"
        />

        {hasMultiple ? (
          <>
            <button
              type="button"
              onClick={showPrevious}
              className="listing-gallery-nav listing-gallery-nav-prev absolute left-4 top-1/2 z-10 flex h-11 w-11 -translate-y-1/2 items-center justify-center rounded-full bg-[color:color-mix(in_srgb,var(--background-elevated)_88%,transparent)] text-[1.35rem] font-black text-[var(--foreground)] opacity-0 shadow-[0_12px_30px_rgba(15,23,42,0.16)] transition group-hover:opacity-100"
              aria-label="Предыдущее изображение"
            >
              ‹
            </button>
            <button
              type="button"
              onClick={showNext}
              className="listing-gallery-nav listing-gallery-nav-next absolute right-4 top-1/2 z-10 flex h-11 w-11 -translate-y-1/2 items-center justify-center rounded-full bg-[color:color-mix(in_srgb,var(--background-elevated)_88%,transparent)] text-[1.35rem] font-black text-[var(--foreground)] opacity-0 shadow-[0_12px_30px_rgba(15,23,42,0.16)] transition group-hover:opacity-100"
              aria-label="Следующее изображение"
            >
              ›
            </button>
            <div className="listing-gallery-counter absolute bottom-4 right-4 rounded-full bg-[color:color-mix(in_srgb,var(--background-elevated)_86%,transparent)] px-3 py-1 text-sm font-semibold">
              {activeIndex + 1} / {media.length}
            </div>
          </>
        ) : null}
      </div>

      {hasMultiple ? (
        <div className="listing-gallery-thumbs flex gap-3 overflow-x-auto pb-0 pl-0">
          {media.map((url, index) => {
            const isActive = index === activeIndex;
            return (
              <button
                key={`${url}-${index}`}
                type="button"
                onClick={() => setActiveIndex(index)}
                className={`relative h-24 w-36 shrink-0 overflow-hidden rounded-[20px] border transition ${
                  isActive
                    ? "border-[var(--color-primary-strong)] shadow-[0_10px_28px_rgba(15,23,42,0.16)]"
                    : "border-[var(--border-color)] opacity-80"
                }`}
                aria-label={`Открыть изображение ${index + 1}`}
              >
                <Image
                  src={url}
                  alt={`${title} ${index + 1}`}
                  fill
                  unoptimized
                  className="object-cover"
                  sizes="144px"
                />
              </button>
            );
          })}
        </div>
      ) : null}
    </div>
  );
}
