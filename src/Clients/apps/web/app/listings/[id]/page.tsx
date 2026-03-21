import { ListingDetailsClient } from "./ListingDetailsClient";

type Props = {
  params: Promise<{
    id: string;
  }>;
};

export default async function ListingDetailsPage({ params }: Props) {
  const { id } = await params;
  return <ListingDetailsClient listingId={id} />;
}
