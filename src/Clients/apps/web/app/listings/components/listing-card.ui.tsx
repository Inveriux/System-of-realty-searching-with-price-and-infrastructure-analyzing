"use client";

import {
  BedDouble,
  Building2,
  LandPlot,
  Ruler,
  Sparkles,
  type LucideIcon,
} from "lucide-react";

export const placeholderMarkup = encodeURIComponent(`
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1200 780">
    <defs>
      <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0%" stop-color="#edf3fb"/>
        <stop offset="100%" stop-color="#dfe7f5"/>
      </linearGradient>
    </defs>
    <rect width="1200" height="780" fill="url(#g)"/>
    <circle cx="180" cy="160" r="120" fill="#ffffff" fill-opacity="0.45"/>
    <circle cx="1020" cy="630" r="180" fill="#ffffff" fill-opacity="0.22"/>
    <path d="M240 560 430 390l170 125 110-90 185 135" fill="none" stroke="#70839d" stroke-opacity="0.35" stroke-width="28" stroke-linecap="round" stroke-linejoin="round"/>
    <rect x="330" y="290" width="540" height="260" rx="28" fill="#ffffff" fill-opacity="0.62"/>
    <text x="600" y="420" text-anchor="middle" fill="#475569" font-family="Arial, sans-serif" font-size="44" font-weight="700">Объявление</text>
    <text x="600" y="478" text-anchor="middle" fill="#64748b" font-family="Arial, sans-serif" font-size="26">Фотография появится позже</text>
  </svg>
`);

export const placeholderImage = `data:image/svg+xml;charset=UTF-8,${placeholderMarkup}`;

export function getSpecIcon(label: string): LucideIcon {
  switch (label) {
    case "Площадь":
    case "Дом":
    case "Потолки":
      return Ruler;
    case "Комнаты":
      return BedDouble;
    case "Участок":
      return LandPlot;
    case "Тип":
    case "Этаж":
    case "Этажей":
    case "Класс":
    case "Ворота":
    case "Парковка":
      return Building2;
    default:
      return Sparkles;
  }
}

export function getInlineSpecLabel(label: string) {
  switch (label) {
    case "Дом":
      return "Дом";
    case "Площадь":
      return "Площадь";
    case "Комнаты":
      return "Комнат";
    case "Спальни":
      return "Спален";
    case "Этаж":
      return "Этаж";
    case "Этажей":
      return "Этажей";
    case "Жилая":
      return "Жилая";
    case "Кухня":
      return "Кухня";
    case "Участок":
      return "Участок";
    case "Тип":
      return "Тип";
    case "Потолки":
      return "Потолки";
    case "Категория":
      return "Категория";
    case "Назначение":
      return "Назначение";
    case "Подъезд":
      return "Подъезд";
    case "Класс":
      return "Класс";
    case "Уровней":
      return "Уровней";
    case "Парковка":
      return "Парковка";
    case "Ворота":
      return "Ворота";
    case "Нагрузка":
      return "Нагрузка";
    default:
      return label;
  }
}
