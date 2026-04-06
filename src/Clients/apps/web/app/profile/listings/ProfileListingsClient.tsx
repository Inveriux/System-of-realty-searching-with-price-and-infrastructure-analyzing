"use client";

import { deleteListing, getCreateListingLookups, hideListing, unhideListing, updateListing } from "@diploma/api/listings";
import { updateListingPrice } from "@diploma/api/pricing-analytics";
import type { ListingDetailsDto, ListingDto } from "@diploma/contracts/listings";
import { useListingsQuery } from "@diploma/features";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { EyeOff, LayoutGrid, NotebookPen } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo, useState, type Dispatch, type SetStateAction } from "react";
import { AppShell } from "../../components/AppShell";
import { useAuth } from "../../providers";
import { ListingGrid } from "../../listings/components/ListingGrid";
import { useFavoriteListings } from "../../listings/lib/listing-collections";
import { mapApiListingToCard } from "../../listings/listing-card.mapper";
import { listingsPageClasses } from "../../listings/styles/listings-page.styles";
import { type LookupTranslationKind, translateLookup } from "../../lib/lookup-translations";

type EditFormValue = string | boolean;
type EditFormState = Record<string, EditFormValue>;
type ListingDetailsType = "Apartment" | "House" | "Land" | "Office" | "Warehouse";

export function ProfileListingsClient() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { api, status, user } = useAuth();
  const favorites = useFavoriteListings();
  const { data, isLoading, error } = useListingsQuery(api);
  const lookupsQuery = useQuery({
    queryKey: ["create-listing-lookups"],
    queryFn: () => getCreateListingLookups(api),
    enabled: status === "authenticated",
  });
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [hidingId, setHidingId] = useState<string | null>(null);
  const [unhidingId, setUnhidingId] = useState<string | null>(null);
  const [optimisticHiddenIds, setOptimisticHiddenIds] = useState<Set<string>>(new Set());
  const [viewMode, setViewMode] = useState<"active" | "hidden">("active");
  const [editingListing, setEditingListing] = useState<ListingDto | null>(null);
  const [editingPriceListing, setEditingPriceListing] = useState<ListingDto | null>(null);
  const [isSavingEdit, setIsSavingEdit] = useState(false);
  const [isSavingPrice, setIsSavingPrice] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const [editForm, setEditForm] = useState<EditFormState>({});
  const [priceForm, setPriceForm] = useState({ priceAmountByn: "", comment: "" });

  async function handleDelete(id: string) {
    try {
      setDeletingId(id);
      await deleteListing(api, id);
      await invalidateListingQueries(queryClient);
    } finally {
      setDeletingId(null);
    }
  }

  async function handleHide(id: string) {
    try {
      setHidingId(id);
      setOptimisticHiddenIds((current) => new Set(current).add(id));
      await hideListing(api, id);
      await invalidateListingQueries(queryClient);
    } catch (nextError) {
      setOptimisticHiddenIds((current) => {
        const next = new Set(current);
        next.delete(id);
        return next;
      });
      setActionError(nextError instanceof Error ? nextError.message : "Не удалось скрыть объявление.");
    } finally {
      setHidingId(null);
    }
  }

  async function handleUnhide(id: string) {
    try {
      setUnhidingId(id);
      setOptimisticHiddenIds((current) => {
        const next = new Set(current);
        next.delete(id);
        return next;
      });
      await unhideListing(api, id);
      await invalidateListingQueries(queryClient);
    } catch (nextError) {
      setOptimisticHiddenIds((current) => new Set(current).add(id));
      setActionError(nextError instanceof Error ? nextError.message : "Не удалось вернуть объявление.");
    } finally {
      setUnhidingId(null);
    }
  }

  const ownListings = useMemo(() => {
    if (!user) {
      return [];
    }

    return (data?.items ?? []).filter((listing) => listing.agentId === user.id);
  }, [data?.items, user]);

  const listingsWithLocalState = useMemo(() => ownListings.map((listing) => {
    if (optimisticHiddenIds.has(listing.id)) {
      return { ...listing, status: "Hidden" };
    }

    if (unhidingId === listing.id) {
      return { ...listing, status: "Active" };
    }

    return listing;
  }), [optimisticHiddenIds, ownListings, unhidingId]);

  const activeListings = listingsWithLocalState.filter((listing) => listing.status !== "Hidden");
  const hiddenListings = listingsWithLocalState.filter((listing) => listing.status === "Hidden");
  const visibleListings = viewMode === "hidden" ? hiddenListings : activeListings;

  function openEditModal(listing: ListingDto) {
    setActionError(null);
    setEditingListing(listing);
    setEditForm(buildInitialEditForm(listing));
  }

  function openPriceModal(listing: ListingDto) {
    setActionError(null);
    setEditingPriceListing(listing);
    setPriceForm({
      priceAmountByn: listing.priceAmountByn != null ? String(listing.priceAmountByn) : "",
      comment: "",
    });
  }

  const items = visibleListings.map((listing) =>
    mapApiListingToCard(listing, {
      canDelete: true,
      canHide: listing.status !== "Hidden",
      canUnhide: listing.status === "Hidden",
      compactLikeCatalog: true,
      canEdit: true,
      canEditPrice: true,
      deletingId,
      hidingId,
      unhidingId,
      onDelete: handleDelete,
      onHide: handleHide,
      onUnhide: handleUnhide,
      onEdit: () => router.push(`/profile/listings/${listing.id}/edit`),
      onEditPrice: () => openPriceModal(listing),
      isFavorite: favorites.isFavorite(listing.id),
      onToggleFavorite: favorites.toggleFavorite,
    }),
  );

  async function handleSaveEdit() {
    if (!editingListing) {
      return;
    }

    try {
      setIsSavingEdit(true);
      setActionError(null);

      await updateListing(api, editingListing.id, buildUpdatePayload(editingListing, editForm));
      await invalidateListingQueries(queryClient);
      setEditingListing(null);
    } catch (nextError) {
      setActionError(nextError instanceof Error ? nextError.message : "Не удалось сохранить изменения.");
    } finally {
      setIsSavingEdit(false);
    }
  }

  async function handleSavePrice() {
    if (!editingPriceListing) {
      return;
    }

    try {
      const normalizedPrice = normalizePriceInput(priceForm.priceAmountByn);
      if (normalizedPrice == null || normalizedPrice <= 0) {
        setActionError("Укажите корректную цену в BYN больше нуля.");
        return;
      }

      setIsSavingPrice(true);
      setActionError(null);
      await updateListingPrice(api, editingPriceListing.id, {
        priceAmountByn: normalizedPrice,
        comment: priceForm.comment || null,
      });
      await invalidateListingQueries(queryClient);
      setEditingPriceListing(null);
    } catch (nextError) {
      setActionError(nextError instanceof Error ? nextError.message : "Не удалось изменить цену.");
    } finally {
      setIsSavingPrice(false);
    }
  }

  if (status !== "authenticated" || !user) {
    return (
      <AppShell>
        <section className="surface grid gap-4 rounded-[28px] p-7">
          <div className="eyebrow">Мои объявления</div>
          <h1 className="section-title">Войдите в аккаунт</h1>
          <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 680 }}>
            Каталог ваших объектов и удаление объявлений доступны только после входа.
          </p>
          <div>
            <Link className="button-primary" href="/listings">
              Перейти к каталогу
            </Link>
          </div>
        </section>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <main className={listingsPageClasses.main}>
        <section className={listingsPageClasses.section}>
          <div className={listingsPageClasses.headingWrap}>
            <div className="grid gap-3">
              <div className="eyebrow">Личный каталог</div>
              <h1 className="section-title" style={{ marginTop: 0 }}>
                Мои объявления
              </h1>
              <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 720 }}>
                Здесь собраны ваши карточки в том же шаблоне, что и в общем каталоге. Для ваших объектов дополнительно показываются характеристики самого объявления.
              </p>
            </div>

            <Link className="button-secondary" href="/create-listing">
              Добавить объявление
            </Link>
          </div>

          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              onClick={() => setViewMode("active")}
              className={`inline-flex items-center gap-2 rounded-full border px-4 py-2 text-sm font-semibold transition ${
                viewMode === "active"
                  ? "border-[color:var(--color-primary)] bg-[color:color-mix(in_srgb,var(--color-primary-soft)_78%,var(--background-elevated))] text-[color:var(--color-primary-strong)]"
                  : "border-[color:var(--border-color)] bg-[color:var(--background-elevated)] text-[color:var(--foreground-muted)]"
              }`}
            >
              <LayoutGrid className="h-4 w-4" />
              <span>Активные</span>
              <span>{activeListings.length}</span>
            </button>
            <button
              type="button"
              onClick={() => setViewMode("hidden")}
              className={`inline-flex items-center gap-2 rounded-full border px-4 py-2 text-sm font-semibold transition ${
                viewMode === "hidden"
                  ? "border-[color:#b45309] bg-[color:color-mix(in_srgb,#fef3c7_84%,var(--background-elevated))] text-[#b45309]"
                  : "border-[color:var(--border-color)] bg-[color:var(--background-elevated)] text-[color:var(--foreground-muted)]"
              }`}
            >
              <EyeOff className="h-4 w-4" />
              <span>Скрытые</span>
              <span>{hiddenListings.length}</span>
            </button>
          </div>

          {error ? (
            <div className={listingsPageClasses.error}>
              Ошибка: {String(error.message)}
            </div>
          ) : null}

          {actionError ? (
            <div className={listingsPageClasses.error}>
              Ошибка: {actionError}
            </div>
          ) : null}

          {!isLoading && items.length === 0 ? (
            <section className="surface grid gap-4 rounded-[28px] p-7">
              <div className="flex h-14 w-14 items-center justify-center rounded-2xl border border-[color:var(--border-color)] bg-[color:var(--background-elevated)]">
                {viewMode === "hidden" ? (
                  <EyeOff className="h-6 w-6 text-[#b45309]" />
                ) : (
                  <NotebookPen className="h-6 w-6 text-[color:var(--color-primary)]" />
                )}
              </div>
              <div className="eyebrow">{viewMode === "hidden" ? "Скрытые объявления" : "Нет объявлений"}</div>
              <h2 className="section-title" style={{ marginTop: 0 }}>
                {viewMode === "hidden" ? "У вас пока нет скрытых объявлений" : "У вас пока нет активных объявлений"}
              </h2>
              <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 620 }}>
                {viewMode === "hidden"
                  ? "Скрытые из общего каталога объекты появятся здесь отдельной вкладкой."
                  : "Создайте новый объект, и он появится здесь отдельной карточкой в вашем каталоге."}
              </p>
              <div>
                {viewMode === "hidden" ? (
                  <button
                    type="button"
                    className="button-secondary"
                    onClick={() => setViewMode("active")}
                  >
                    Открыть активные
                  </button>
                ) : (
                  <Link className="button-primary" href="/create-listing">
                    Добавить объявление
                  </Link>
                )}
              </div>
            </section>
          ) : (
            <ListingGrid items={items} isLoading={isLoading && items.length === 0} />
          )}
        </section>
      </main>
      {editingListing ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="surface max-h-[90vh] w-full max-w-4xl overflow-y-auto rounded-[28px] p-6">
            <div className="grid gap-4">
              <div>
                <div className="eyebrow">Редактирование</div>
                <h2 className="section-title" style={{ marginTop: 8 }}>Параметры объявления</h2>
              </div>

              <div className="grid gap-3 sm:grid-cols-2">
                {renderTextField("title", "Название", editForm, setEditForm)}
                {renderTextField("addressText", "Адрес", editForm, setEditForm)}
              </div>

              <textarea
                className="field min-h-28"
                value={String(editForm.description ?? "")}
                onChange={(event) => setEditForm((current) => ({ ...current, description: event.target.value }))}
                placeholder="Описание"
              />

              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                {renderTextField("priceAmountByn", "Цена BYN", editForm, setEditForm)}
                {renderTextField("areaTotal", "Площадь", editForm, setEditForm)}
                {renderTextField("buildYear", "Год постройки", editForm, setEditForm)}
                {renderTextField("publishedDate", "Дата публикации ISO", editForm, setEditForm)}
              </div>

              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                {renderTextField("latitude", "Широта", editForm, setEditForm)}
                {renderTextField("longitude", "Долгота", editForm, setEditForm)}
                {renderSelectField("listingType", "Тип сделки", editForm, setEditForm, lookupsQuery.data?.listingTypes.map((item) => item.name) ?? [], "listingType")}
                {renderSelectField("buildingType", "Тип здания", editForm, setEditForm, lookupsQuery.data?.buildingTypes.map((item) => item.name) ?? [], "buildingType")}
              </div>

              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {renderSelectField("listingDetailsType", "Категория", editForm, setEditForm, ["Apartment", "House", "Land", "Office", "Warehouse"])}
                {renderSelectField("status", "Статус", editForm, setEditForm, ["Active", "Hidden"])}
                {renderTextField("pricePeriod", "Период цены", editForm, setEditForm)}
              </div>

              <textarea
                className="field min-h-24"
                value={String(editForm.mediaUrls ?? "")}
                onChange={(event) => setEditForm((current) => ({ ...current, mediaUrls: event.target.value }))}
                placeholder="Ссылки на фото, по одной в строке"
              />

              {renderTypeSpecificFields(
                String(editForm.listingDetailsType ?? editingListing.listingDetailsType) as ListingDetailsType,
                editForm,
                setEditForm,
                lookupsQuery.data,
              )}

              <div className="flex flex-wrap gap-3">
                <button className="button-primary" type="button" onClick={handleSaveEdit} disabled={isSavingEdit}>
                  {isSavingEdit ? "Сохранение..." : "Сохранить"}
                </button>
                <button className="button-secondary" type="button" onClick={() => setEditingListing(null)} disabled={isSavingEdit}>
                  Отмена
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : null}
      {editingPriceListing ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="surface w-full max-w-lg rounded-[28px] p-6">
            <div className="grid gap-4">
              <div>
                <div className="eyebrow">Цена</div>
                <h2 className="section-title" style={{ marginTop: 8 }}>Изменение цены</h2>
                <p style={{ margin: "8px 0 0", color: "var(--foreground-muted)" }}>
                  Новая цена изменяется и сохраняется в <strong>BYN</strong> (белорусских рублях).
                </p>
              </div>
              <input
                className="field"
                inputMode="decimal"
                value={priceForm.priceAmountByn}
                onChange={(event) => setPriceForm((current) => ({ ...current, priceAmountByn: event.target.value }))}
                placeholder="Новая цена, BYN"
              />
              <textarea className="field min-h-24" value={priceForm.comment} onChange={(event) => setPriceForm((current) => ({ ...current, comment: event.target.value }))} placeholder="Комментарий к изменению цены" />
              <div className="flex flex-wrap gap-3">
                <button className="button-primary" type="button" onClick={handleSavePrice} disabled={isSavingPrice}>
                  {isSavingPrice ? "Сохранение..." : "Сохранить цену в BYN"}
                </button>
                <button className="button-secondary" type="button" onClick={() => setEditingPriceListing(null)} disabled={isSavingPrice}>
                  Отмена
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : null}
    </AppShell>
  );
}

async function invalidateListingQueries(queryClient: ReturnType<typeof useQueryClient>) {
  await queryClient.invalidateQueries({ queryKey: ["listings"] });
  await queryClient.invalidateQueries({ queryKey: ["search-listings"] });
  await queryClient.invalidateQueries({ queryKey: ["search-listings-map"] });
  await queryClient.invalidateQueries({ queryKey: ["pricing-analytics"] });
}

function normalizePriceInput(value: string) {
  const normalized = value.replace(/\s+/g, "").replace(",", ".").trim();
  if (!normalized) {
    return null;
  }

  const parsed = Number(normalized);
  if (!Number.isFinite(parsed)) {
    return null;
  }

  return parsed;
}

function buildInitialEditForm(listing: ListingDto): EditFormState {
  const details = listing.listingDetails as Record<string, string | number | boolean | null | undefined>;

  return {
    title: listing.title,
    addressText: listing.addressText ?? "",
    description: listing.description ?? "",
    areaTotal: stringifyNullable(listing.areaTotal),
    buildYear: stringifyNullable(listing.buildYear),
    latitude: stringifyNullable(listing.latitude),
    longitude: stringifyNullable(listing.longitude),
    priceAmountByn: stringifyNullable(listing.priceAmountByn),
    pricePeriod: listing.pricePeriod ?? "",
    publishedDate: listing.publishedDate ?? "",
    buildingType: listing.buildingType ?? "",
    listingType: listing.listingType ?? "",
    listingDetailsType: listing.listingDetailsType ?? "Apartment",
    status: listing.status ?? "Active",
    mediaUrls: (listing.media ?? []).map((item) => item.url).join("\n"),
    rooms: stringifyNullable(details.rooms as number | null | undefined),
    bedrooms: stringifyNullable(details.bedrooms as number | null | undefined),
    floorsTotal: stringifyNullable(details.floorsTotal as number | null | undefined),
    levels: stringifyNullable(details.levels as number | null | undefined),
    floor: stringifyNullable(details.floor as number | null | undefined),
    areaLiving: stringifyNullable(details.areaLiving as number | null | undefined),
    areaKitchen: stringifyNullable(details.areaKitchen as number | null | undefined),
    hasBalcony: Boolean(details.hasBalcony),
    hasLoggia: Boolean(details.hasLoggia),
    hasElevator: Boolean(details.hasElevator),
    ceilingHeight: stringifyNullable(details.ceilingHeight as number | null | undefined),
    bathroomType: String(details.bathroomType ?? ""),
    renovationType: String(details.renovationType ?? ""),
    buildingMaterial: String(details.buildingMaterial ?? ""),
    floors: stringifyNullable(details.floors as number | null | undefined),
    houseArea: stringifyNullable(details.houseArea as number | null | undefined),
    plotArea: stringifyNullable(details.plotArea as number | null | undefined),
    hasElectricity: Boolean(details.hasElectricity),
    hasGarage: Boolean(details.hasGarage),
    hasBasement: Boolean(details.hasBasement),
    hasFence: Boolean(details.hasFence),
    wallMaterial: String(details.wallMaterial ?? ""),
    heatingType: String(details.heatingType ?? ""),
    sewerType: String(details.sewerType ?? ""),
    gasType: String(details.gasType ?? ""),
    roofMaterial: String(details.roofMaterial ?? ""),
    waterSupplyType: String(details.waterSupplyType ?? ""),
    allowedUse: String(details.allowedUse ?? ""),
    hasGas: Boolean(details.hasGas),
    hasWater: Boolean(details.hasWater),
    hasSewer: Boolean(details.hasSewer),
    hasBuildings: Boolean(details.hasBuildings),
    landCategory: String(details.landCategory ?? ""),
    roadAccessType: String(details.roadAccessType ?? ""),
    officeClass: String(details.officeClass ?? ""),
    layoutType: String(details.layoutType ?? ""),
    officeRenovationType: String(details.renovationType ?? ""),
    parkingType: String(details.parkingType ?? ""),
    parkingSpaces: stringifyNullable(details.parkingSpaces as number | null | undefined),
    kitchenette: Boolean(details.kitchenette),
    floorLoadTonsPerSquareMeter: stringifyNullable(details.floorLoadTonsPerSquareMeter as number | null | undefined),
    gateCount: stringifyNullable(details.gateCount as number | null | undefined),
    hasDockLevelers: Boolean(details.hasDockLevelers),
    dockCount: stringifyNullable(details.dockCount as number | null | undefined),
    hasRamps: Boolean(details.hasRamps),
    powerKw: stringifyNullable(details.powerKw as number | null | undefined),
    hasOffice: Boolean(details.hasOffice),
    officeArea: stringifyNullable(details.officeArea as number | null | undefined),
    classType: String(details.classType ?? ""),
    warehouseHeatingType: String(details.heatingType ?? ""),
  };
}

function buildUpdatePayload(listing: ListingDto, form: EditFormState) {
  const listingDetailsType = String(form.listingDetailsType || listing.listingDetailsType || "Apartment") as ListingDetailsType;
  const rawPublishedDate = String(form.publishedDate ?? "").trim();

  return {
    agentId: listing.agentId,
    title: String(form.title ?? ""),
    addressText: String(form.addressText ?? ""),
    locationId: listing.locationId ?? null,
    latitude: toNumberOrNull(form.latitude),
    longitude: toNumberOrNull(form.longitude),
    description: String(form.description ?? "") || null,
    priceAmount: toNumberOrNull(form.priceAmountByn),
    priceCurrency: listing.priceCurrency ?? "BYN",
    priceAmountByn: toNumberOrNull(form.priceAmountByn),
    priceAmountUsd: null,
    pricePeriod: String(form.pricePeriod ?? "") || null,
    areaTotal: toNumberOrZero(form.areaTotal),
    buildYear: toIntegerOrNull(form.buildYear),
    publishedDate: rawPublishedDate || null,
    addressId: listing.addressText === String(form.addressText ?? "") ? (listing as { addressId?: string }).addressId ?? crypto.randomUUID() : crypto.randomUUID(),
    buildingType: String(form.buildingType ?? "") || null,
    listingType: String(form.listingType ?? listing.listingType ?? ""),
    status: String(form.status ?? listing.status ?? "Active") || null,
    listingDetailsType,
    mediaUrls: parseMediaUrls(form.mediaUrls),
    listingDetails: buildListingDetails(listingDetailsType, form),
  };
}

function buildListingDetails(type: ListingDetailsType, form: EditFormState): ListingDetailsDto {
  switch (type) {
    case "House":
      return {
        rooms: toIntegerOrNull(form.rooms),
        bedrooms: toIntegerOrNull(form.bedrooms),
        floors: toIntegerOrNull(form.floors),
        houseArea: toNumberOrNull(form.houseArea),
        plotArea: toNumberOrNull(form.plotArea),
        hasElectricity: toBoolean(form.hasElectricity),
        hasGarage: toBoolean(form.hasGarage),
        hasBasement: toBoolean(form.hasBasement),
        hasFence: toBoolean(form.hasFence),
        wallMaterial: toStringOrNull(form.wallMaterial),
        heatingType: toStringOrNull(form.heatingType),
        sewerType: toStringOrNull(form.sewerType),
        gasType: toStringOrNull(form.gasType),
        roofMaterial: toStringOrNull(form.roofMaterial),
        waterSupplyType: toStringOrNull(form.waterSupplyType),
      };
    case "Land":
      return {
        plotArea: toNumberOrNull(form.plotArea),
        allowedUse: toStringOrNull(form.allowedUse),
        hasElectricity: toBoolean(form.hasElectricity),
        hasGas: toBoolean(form.hasGas),
        hasWater: toBoolean(form.hasWater),
        hasSewer: toBoolean(form.hasSewer),
        hasBuildings: toBoolean(form.hasBuildings),
        landCategory: toStringOrNull(form.landCategory),
        roadAccessType: toStringOrNull(form.roadAccessType),
      };
    case "Office":
      return {
        plotArea: toNumberOrNull(form.plotArea),
        floor: toIntegerOrNull(form.floor),
        floorsTotal: toIntegerOrNull(form.floorsTotal),
        levels: toIntegerOrNull(form.levels),
        parkingSpaces: toIntegerOrNull(form.parkingSpaces),
        kitchenette: toBoolean(form.kitchenette),
        officeClass: toStringOrNull(form.officeClass),
        layoutType: toStringOrNull(form.layoutType),
        renovationType: toStringOrNull(form.officeRenovationType),
        parkingType: toStringOrNull(form.parkingType),
      };
    case "Warehouse":
      return {
        plotArea: toNumberOrNull(form.plotArea),
        ceilingHeight: toNumberOrNull(form.ceilingHeight),
        floorLoadTonsPerSquareMeter: toNumberOrNull(form.floorLoadTonsPerSquareMeter),
        gateCount: toIntegerOrNull(form.gateCount),
        hasDockLevelers: toBoolean(form.hasDockLevelers),
        dockCount: toIntegerOrNull(form.dockCount),
        hasRamps: toBoolean(form.hasRamps),
        powerKw: toNumberOrNull(form.powerKw),
        hasOffice: toBoolean(form.hasOffice),
        officeArea: toNumberOrNull(form.officeArea),
        classType: toStringOrNull(form.classType),
        heatingType: toStringOrNull(form.warehouseHeatingType),
      };
    case "Apartment":
    default:
      return {
        rooms: toIntegerOrNull(form.rooms),
        bedrooms: toIntegerOrNull(form.bedrooms),
        floorsTotal: toIntegerOrNull(form.floorsTotal),
        levels: toIntegerOrNull(form.levels),
        floor: toIntegerOrNull(form.floor),
        areaLiving: toNumberOrNull(form.areaLiving),
        areaKitchen: toNumberOrNull(form.areaKitchen),
        hasBalcony: toBoolean(form.hasBalcony),
        hasLoggia: toBoolean(form.hasLoggia),
        hasElevator: toBoolean(form.hasElevator),
        ceilingHeight: toNumberOrNull(form.ceilingHeight),
        bathroomType: toStringOrNull(form.bathroomType),
        renovationType: toStringOrNull(form.renovationType),
        buildingMaterial: toStringOrNull(form.buildingMaterial),
      };
  }
}

function renderTypeSpecificFields(
  listingType: ListingDetailsType,
  form: EditFormState,
  setForm: Dispatch<SetStateAction<EditFormState>>,
  lookups: Awaited<ReturnType<typeof getCreateListingLookups>> | undefined,
) {
  switch (listingType) {
    case "House":
      return (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderTextField("rooms", "Комнаты", form, setForm)}
            {renderTextField("bedrooms", "Спальни", form, setForm)}
            {renderTextField("floors", "Этажей", form, setForm)}
            {renderTextField("houseArea", "Площадь дома", form, setForm)}
            {renderTextField("plotArea", "Площадь участка", form, setForm)}
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderSelectField("wallMaterial", "Материал стен", form, setForm, lookups?.wallMaterials.map((item) => item.name) ?? [], "wallMaterial")}
            {renderSelectField("heatingType", "Отопление", form, setForm, lookups?.heatingTypes.map((item) => item.name) ?? [], "heatingType")}
            {renderSelectField("sewerType", "Канализация", form, setForm, lookups?.sewerTypes.map((item) => item.name) ?? [], "sewerType")}
            {renderSelectField("gasType", "Газ", form, setForm, lookups?.gasTypes.map((item) => item.name) ?? [], "gasType")}
            {renderSelectField("roofMaterial", "Крыша", form, setForm, lookups?.wallMaterials.map((item) => item.name) ?? [], "wallMaterial")}
            {renderSelectField("waterSupplyType", "Водоснабжение", form, setForm, lookups?.waterSupplyTypes.map((item) => item.name) ?? [], "waterSupplyType")}
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {renderCheckboxField("hasElectricity", "Электричество", form, setForm)}
            {renderCheckboxField("hasGarage", "Гараж", form, setForm)}
            {renderCheckboxField("hasBasement", "Подвал", form, setForm)}
            {renderCheckboxField("hasFence", "Забор", form, setForm)}
          </div>
        </>
      );
    case "Land":
      return (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderTextField("plotArea", "Площадь участка", form, setForm)}
            {renderTextField("allowedUse", "Назначение", form, setForm)}
            {renderSelectField("landCategory", "Категория земли", form, setForm, lookups?.landCategories.map((item) => item.name) ?? [], "landCategory")}
            {renderSelectField("roadAccessType", "Подъезд", form, setForm, lookups?.roadAccessTypes.map((item) => item.name) ?? [], "roadAccessType")}
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
            {renderCheckboxField("hasElectricity", "Электричество", form, setForm)}
            {renderCheckboxField("hasGas", "Газ", form, setForm)}
            {renderCheckboxField("hasWater", "Вода", form, setForm)}
            {renderCheckboxField("hasSewer", "Канализация", form, setForm)}
            {renderCheckboxField("hasBuildings", "Есть строения", form, setForm)}
          </div>
        </>
      );
    case "Office":
      return (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderTextField("plotArea", "Площадь", form, setForm)}
            {renderTextField("floor", "Этаж", form, setForm)}
            {renderTextField("floorsTotal", "Этажей в здании", form, setForm)}
            {renderTextField("levels", "Уровней", form, setForm)}
            {renderTextField("parkingSpaces", "Парковочных мест", form, setForm)}
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {renderSelectField("officeClass", "Класс офиса", form, setForm, lookups?.officeClasses.map((item) => item.name) ?? [], "officeClass")}
            {renderSelectField("layoutType", "Планировка", form, setForm, lookups?.layoutTypes.map((item) => item.name) ?? [], "layoutType")}
            {renderSelectField("officeRenovationType", "Ремонт", form, setForm, lookups?.renovationTypes.map((item) => item.name) ?? [], "renovationType")}
            {renderSelectField("parkingType", "Паркинг", form, setForm, lookups?.layoutTypes.map((item) => item.name) ?? [], "layoutType")}
          </div>
          <div className="grid gap-3 sm:grid-cols-2">
            {renderCheckboxField("kitchenette", "Есть кухня", form, setForm)}
          </div>
        </>
      );
    case "Warehouse":
      return (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderTextField("plotArea", "Площадь участка", form, setForm)}
            {renderTextField("ceilingHeight", "Высота потолков", form, setForm)}
            {renderTextField("floorLoadTonsPerSquareMeter", "Нагрузка на пол", form, setForm)}
            {renderTextField("gateCount", "Количество ворот", form, setForm)}
            {renderTextField("dockCount", "Количество доков", form, setForm)}
            {renderTextField("powerKw", "Мощность кВт", form, setForm)}
            {renderTextField("officeArea", "Площадь офиса", form, setForm)}
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderSelectField("classType", "Класс склада", form, setForm, lookups?.classTypes.map((item) => item.name) ?? [], "classType")}
            {renderSelectField("warehouseHeatingType", "Отопление", form, setForm, lookups?.heatingTypes.map((item) => item.name) ?? [], "heatingType")}
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {renderCheckboxField("hasDockLevelers", "Доклевеллеры", form, setForm)}
            {renderCheckboxField("hasRamps", "Рампы", form, setForm)}
            {renderCheckboxField("hasOffice", "Есть офис", form, setForm)}
          </div>
        </>
      );
    case "Apartment":
    default:
      return (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderTextField("rooms", "Комнаты", form, setForm)}
            {renderTextField("bedrooms", "Спальни", form, setForm)}
            {renderTextField("floor", "Этаж", form, setForm)}
            {renderTextField("floorsTotal", "Этажей в доме", form, setForm)}
            {renderTextField("levels", "Уровней", form, setForm)}
            {renderTextField("areaLiving", "Жилая площадь", form, setForm)}
            {renderTextField("areaKitchen", "Площадь кухни", form, setForm)}
            {renderTextField("ceilingHeight", "Высота потолков", form, setForm)}
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderSelectField("bathroomType", "Санузел", form, setForm, lookups?.bathroomTypes.map((item) => item.name) ?? [], "bathroomType")}
            {renderSelectField("renovationType", "Ремонт", form, setForm, lookups?.renovationTypes.map((item) => item.name) ?? [], "renovationType")}
            {renderSelectField("buildingMaterial", "Материал", form, setForm, lookups?.buildingMaterials.map((item) => item.name) ?? [], "buildingMaterial")}
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {renderCheckboxField("hasBalcony", "Балкон", form, setForm)}
            {renderCheckboxField("hasLoggia", "Лоджия", form, setForm)}
            {renderCheckboxField("hasElevator", "Лифт", form, setForm)}
          </div>
        </>
      );
  }
}

function renderTextField(key: string, placeholder: string, form: EditFormState, setForm: Dispatch<SetStateAction<EditFormState>>) {
  return (
    <input
      className="field"
      value={String(form[key] ?? "")}
      onChange={(event) => setForm((current) => ({ ...current, [key]: event.target.value }))}
      placeholder={placeholder}
    />
  );
}

function renderSelectField(
  key: string,
  placeholder: string,
  form: EditFormState,
  setForm: Dispatch<SetStateAction<EditFormState>>,
  options: string[],
  kind?: LookupTranslationKind,
) {
  return (
    <select
      className="field"
      value={String(form[key] ?? "")}
      onChange={(event) => setForm((current) => ({ ...current, [key]: event.target.value }))}
      aria-label={placeholder}
    >
      <option value="">{placeholder}</option>
      {options.map((option) => (
        <option key={option} value={option}>
          {kind ? translateLookup(kind, option) : option}
        </option>
      ))}
    </select>
  );
}

function renderCheckboxField(
  key: string,
  label: string,
  form: EditFormState,
  setForm: Dispatch<SetStateAction<EditFormState>>,
) {
  return (
    <label className="flex items-center gap-2 rounded-2xl border border-[color:var(--border-color)] px-4 py-3 text-sm">
      <input
        type="checkbox"
        checked={Boolean(form[key])}
        onChange={(event) => setForm((current) => ({ ...current, [key]: event.target.checked }))}
      />
      <span>{label}</span>
    </label>
  );
}

function stringifyNullable(value: string | number | null | undefined) {
  return value == null ? "" : String(value);
}

function toStringOrNull(value: EditFormValue | undefined) {
  if (typeof value !== "string") {
    return null;
  }

  const trimmed = value.trim();
  return trimmed ? trimmed : null;
}

function toNumberOrNull(value: EditFormValue | undefined) {
  if (typeof value !== "string" || !value.trim()) {
    return null;
  }

  const normalized = Number(value.replace(",", "."));
  return Number.isFinite(normalized) ? normalized : null;
}

function toIntegerOrNull(value: EditFormValue | undefined) {
  const parsed = toNumberOrNull(value);
  return parsed == null ? null : Math.trunc(parsed);
}

function toNumberOrZero(value: EditFormValue | undefined) {
  return toNumberOrNull(value) ?? 0;
}

function toBoolean(value: EditFormValue | undefined) {
  return Boolean(value);
}

function parseMediaUrls(value: EditFormValue | undefined) {
  if (typeof value !== "string") {
    return [];
  }

  return value
    .split("\n")
    .map((item) => item.trim())
    .filter(Boolean);
}
