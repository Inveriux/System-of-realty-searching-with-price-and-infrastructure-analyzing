import { CreateListingClient } from "../../../../create-listing/CreateListingClient";

type PageProps = {
  params: Promise<{
    id: string;
  }>;
};

export default async function EditListingPage({ params }: PageProps) {
  const { id } = await params;
  return <CreateListingClient mode="edit" listingId={id} />;
}
