export type ListingDto = {
    id: string;
    title: string;
    description: string | null;
    priceAmount: number | null;
    priceCurrency: string | null;
    mainMedia: string | null;
};