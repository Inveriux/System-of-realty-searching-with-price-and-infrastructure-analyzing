import { StyleSheet } from "react-native";
import { colors } from "@diploma/theme";

export const listingCardStyles = StyleSheet.create({
    cardShadowWrap: {
        borderRadius: 16,
        backgroundColor: colors.bg,

        // iOS shadow
        // shadowColor: "#000",
        // shadowOffset: { width: 0, height: 6 },
        // shadowOpacity: 0.12,
        // shadowRadius: 12,

        // Android shadow
        elevation: 4,

        marginHorizontal: 6,
        marginVertical: 5,
    },

    mainListingCardContainer: {
        height: 220,
        borderRadius: 16,
        overflow: "hidden",
    },
    listingCardMainImageContainer: {
        height: "50%",
    },
    listingCardInfoContainer: {
        height: "50%",
        padding: 6,
    },
    listingTypeView: {
        borderRadius: 5,
        backgroundColor: colors.bg,
        position: "absolute",
        top: 8,
        left: 6,
        width: "auto",
        height: "auto",
        zIndex: 2,
        justifyContent: "center",
        alignContent: "center",
        paddingLeft: 2,
        paddingRight: 2
    }
});