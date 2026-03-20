import { StyleSheet } from "react-native";
import { colors } from "@diploma/theme"

export const listingFavortieButtonStyles = StyleSheet.create({
    mainListingFavoriteButtonContainer: {
        height: 32,
        width: 33,
        backgroundColor: colors.bg,
        borderStartStartRadius: 7,
        borderEndEndRadius: 18,
        borderStartEndRadius:18,
        borderEndStartRadius: 0,
        justifyContent: "center", 
        alignContent: "center",
        paddingBottom: 3
    }
});