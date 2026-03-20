import { Text } from "react-native";
import { listingPriceStyles } from './listingPriceStyles';

type Props = {
  price: number | null,
  currencyCode: string | null
};

export function ListingPrice({price, currencyCode} : Props) {
  const priceText = price != null ? String(price) : "Not set";
  const currencyText = currencyCode != null ? ` ${currencyCode}` : "";

    return(
        <Text style={listingPriceStyles.listingPriceText}>{priceText + currencyText}</Text>
    );
}