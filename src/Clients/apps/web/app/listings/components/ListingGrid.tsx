import { ListingCard, ListingCardSkeleton } from "./ListingCard";
import { listingGridClasses } from "./styles/listing-grid.styles";
import type { ListingCardItem } from "./listing-card.types";

type ListingGridProps = {
  items: ListingCardItem[];
  isLoading?: boolean;
};

export function ListingGrid({ items, isLoading = false }: ListingGridProps) {
  if (isLoading) {
    return (
      <div className={listingGridClasses.root}>
        {Array.from({ length: 6 }).map((_, index) => (
          <ListingCardSkeleton key={index} />
        ))}
      </div>
    );
  }

  return (
    <div className={listingGridClasses.root}>
      {items.map((item) => (
        <ListingCard key={item.id} item={item} />
      ))}
    </div>
  );
}
