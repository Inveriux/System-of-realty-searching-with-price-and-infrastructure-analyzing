export const lookupTranslations = {
  listingType: {
    Rent: "Аренда",
    Sale: "Продажа",
  },
  buildingType: {
    "Apartment Building (Multi-family)": "Многоквартирный дом",
    "Detached House": "Отдельный дом",
    Townhouse: "Таунхаус",
    "Business Center": "Бизнес-центр",
    "Warehouse Complex": "Складской комплекс",
    "Industrial Building": "Промышленное здание",
    "Shopping Mall": "Торговый центр",
    "Mixed-use Building": "Многофункциональное здание",
    Other: "Другое",
  },
  bathroomType: {
    "Combined (shared)": "Совмещенный",
    Separate: "Раздельный",
    "Two or more": "Два и более",
    "Bathroom + WC (separate rooms)": "Ванная и туалет раздельно",
  },
  renovationType: {
    "No renovation / requires renovation": "Без ремонта / требует ремонта",
    Cosmetic: "Косметический",
    "Major / Capital": "Капитальный",
    Designer: "Дизайнерский",
    "Shell & Core": "Под чистовую отделку",
    "Finished / Turnkey": "С отделкой / под ключ",
  },
  buildingMaterial: {
    Brick: "Кирпич",
    Panel: "Панель",
    Monolith: "Монолит",
    "Monolith-Brick": "Монолит-кирпич",
    Wood: "Дерево",
    "Block (foam/gas block)": "Блоки (пеноблок / газоблок)",
    "Metal frame": "Металлокаркас",
    Other: "Другое",
  },
  wallMaterial: {
    Brick: "Кирпич",
    Concrete: "Бетон",
    "Sandwich panels": "Сэндвич-панели",
    Metal: "Металл",
    Wood: "Дерево",
    "Block (foam/gas block)": "Блоки (пеноблок / газоблок)",
    Other: "Другое",
  },
  heatingType: {
    Central: "Центральное",
    "Autonomous (building)": "Автономное (на здание)",
    "Individual gas boiler": "Индивидуальный газовый котел",
    Electric: "Электрическое",
    "Solid fuel": "Твердотопливное",
    None: "Нет",
  },
  waterSupplyType: {
    Central: "Центральное",
    Well: "Колодец",
    Borehole: "Скважина",
    "Delivered / tank": "Привозная вода / емкость",
    None: "Нет",
  },
  sewerType: {
    Central: "Центральная",
    "Septic tank": "Септик",
    Cesspool: "Выгребная яма",
    None: "Нет",
  },
  gasType: {
    "Central gas": "Центральный газ",
    "Gas holder / tank": "Газгольдер / резервуар",
    "Bottled gas": "Баллонный газ",
    None: "Нет",
  },
  landCategory: {
    "Residential development": "Жилая застройка",
    "Personal subsidiary farming": "Личное подсобное хозяйство",
    "Garden / dacha plot": "Садовый / дачный участок",
    Agricultural: "Сельскохозяйственная",
    "Industrial / commercial": "Промышленная / коммерческая",
    "Forestry / protected area": "Лесной фонд / охраняемая территория",
    Other: "Другое",
  },
  roadAccessType: {
    Asphalt: "Асфальт",
    Gravel: "Гравий",
    "Dirt road": "Грунтовая дорога",
    "No direct access / off-road": "Нет прямого подъезда / бездорожье",
  },
  classType: {
    A: "Класс A",
    B: "Класс B",
    C: "Класс C",
    D: "Класс D",
  },
  layoutType: {
    "Open space": "Открытая планировка",
    "Cabinet / rooms": "Кабинеты / комнаты",
    Mixed: "Смешанная",
    Coworking: "Коворкинг",
  },
  officeClass: {
    A: "Класс A",
    "B+": "Класс B+",
    B: "Класс B",
    C: "Класс C",
  },
} as const;

export type LookupTranslationKind = keyof typeof lookupTranslations;

export function translateLookup(kind: LookupTranslationKind, value: string) {
  return lookupTranslations[kind][value as keyof (typeof lookupTranslations)[typeof kind]] ?? value;
}
