"use client";

import { useCallback, useEffect, useMemo, useState } from "react";

const FAVORITES_KEY = "diploma.favorite-listings";
const HISTORY_KEY = "diploma.viewed-listings";
const COLLECTION_EVENT = "diploma:listings:collections";
const HISTORY_LIMIT = 80;

function canUseStorage() {
  return typeof window !== "undefined" && typeof window.localStorage !== "undefined";
}

function readIds(key: string): string[] {
  if (!canUseStorage()) {
    return [];
  }

  try {
    const raw = window.localStorage.getItem(key);
    if (!raw) {
      return [];
    }

    const parsed = JSON.parse(raw) as unknown;
    return Array.isArray(parsed) ? parsed.filter((value): value is string => typeof value === "string") : [];
  } catch {
    return [];
  }
}

function writeIds(key: string, ids: string[]) {
  if (!canUseStorage()) {
    return;
  }

  window.localStorage.setItem(key, JSON.stringify(ids));
  window.dispatchEvent(new CustomEvent(COLLECTION_EVENT, { detail: key }));
}

function useStoredIds(key: string) {
  const [ids, setIds] = useState<string[]>([]);

  useEffect(() => {
    setIds(readIds(key));

    function sync(event?: Event) {
      const detailKey = event instanceof CustomEvent ? event.detail : null;
      if (detailKey == null || detailKey === key) {
        setIds(readIds(key));
      }
    }

    window.addEventListener("storage", sync);
    window.addEventListener(COLLECTION_EVENT, sync as EventListener);
    return () => {
      window.removeEventListener("storage", sync);
      window.removeEventListener(COLLECTION_EVENT, sync as EventListener);
    };
  }, [key]);

  return [ids, setIds] as const;
}

export function useFavoriteListings() {
  const [favoriteIds, setFavoriteIds] = useStoredIds(FAVORITES_KEY);

  const isFavorite = useCallback((listingId: string) => {
    return readIds(FAVORITES_KEY).includes(listingId);
  }, []);

  const toggleFavorite = useCallback((listingId: string) => {
    const currentIds = readIds(FAVORITES_KEY);
    const next = currentIds.includes(listingId)
      ? currentIds.filter((id) => id !== listingId)
      : [listingId, ...currentIds];

    writeIds(FAVORITES_KEY, next);
    setFavoriteIds(next);
  }, [setFavoriteIds]);

  const removeFavorite = useCallback((listingId: string) => {
    const next = readIds(FAVORITES_KEY).filter((id) => id !== listingId);
    writeIds(FAVORITES_KEY, next);
    setFavoriteIds(next);
  }, [setFavoriteIds]);

  const api = useMemo(() => ({
    favoriteIds,
    isFavorite,
    toggleFavorite,
    removeFavorite,
  }), [favoriteIds, isFavorite, removeFavorite, toggleFavorite]);

  return api;
}

export function useViewedListings() {
  const [viewedIds, setViewedIds] = useStoredIds(HISTORY_KEY);

  const pushViewed = useCallback((listingId: string) => {
    const currentIds = readIds(HISTORY_KEY);
    const next = [listingId, ...currentIds.filter((id) => id !== listingId)].slice(0, HISTORY_LIMIT);
    writeIds(HISTORY_KEY, next);
    setViewedIds(next);
  }, [setViewedIds]);

  const clearViewed = useCallback(() => {
    writeIds(HISTORY_KEY, []);
    setViewedIds([]);
  }, [setViewedIds]);

  const api = useMemo(() => ({
    viewedIds,
    pushViewed,
    clearViewed,
  }), [clearViewed, pushViewed, viewedIds]);

  return api;
}
