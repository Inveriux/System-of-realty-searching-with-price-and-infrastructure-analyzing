import React from "react";
import { View, FlatList, useWindowDimensions } from "react-native";
import { ApiClient } from "@diploma/api/http";
import { useListingsQuery } from "@diploma/features";
import { ListingCard } from "../../../components/listings/listringCard/ListingsCard";
import { listingCardStyles } from "./listingsCatalogStyles";

function getNumColumns(width: number) {
  if (width >= 1024) return 4; 
  if (width >= 768) return 3;  
  return 2;                    
}

const api = new ApiClient({
  baseUrl: process.env.EXPO_PUBLIC_API_BASE_URL ?? "http://localhost:5084",
});

export default function ListingCatalog() {
  const { data } = useListingsQuery(api);
  const { width } = useWindowDimensions();

  const numColumns = getNumColumns(width);

  const padding = width >= 768 ? 8 : 2;
  const gap = width >= 768 ? 6 : 4;

  const itemWidth = (width - padding * 2 - gap * (numColumns - 1)) / numColumns;

  return (
  <View style={listingCardStyles.mainListingsCardContainer}>
    <FlatList
      data={data?.items ?? []}
      keyExtractor={(item) => item.id.toString()}
      numColumns={numColumns}
      contentContainerStyle={{ padding: padding }}
      columnWrapperStyle={{ columnGap: gap }}
      renderItem={({ item }) => (
        <View style={{ width: itemWidth }}>
          <ListingCard
            item={item}
            onPress={() => {
              console.log("Pressed:", item.id);
            }}/>
        </View>
      )}/>
  </View>
  );
}