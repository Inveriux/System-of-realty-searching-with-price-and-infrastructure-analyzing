import React, { useRef, useState } from "react";
import { Animated, Pressable, View, Text, Image } from "react-native";
import { listingCardStyles as s } from "./listingCardStyles";
import type { ListingDto } from "@diploma/contracts/listings";
import { HorizontalDelimiter } from "../../delimiters/delimiter";
import { ListingPrice } from "../listingPrice/ListingPrice";
import { ListingFavoriteButton } from "../listingFavoriteButton/ListingFavoriteButton";
import GradientText from '../../text/GradientText';

type Props = {
  item: ListingDto;
  onPress?: () => void;
};

export function ListingCard({ item, onPress }: Props) {
  const scale = useRef(new Animated.Value(1)).current;
  const [imageFailed, setImageFailed] = useState(false);

  const pressIn = () => {
    Animated.spring(scale, {
      toValue: 0.98,
      useNativeDriver: true,
      speed: 20,
      bounciness: 0,
    }).start();
  };

  const pressOut = () => {
    Animated.spring(scale, {
      toValue: 1,
      useNativeDriver: true,
      speed: 20,
      bounciness: 0,
    }).start();
  };

    const defaultCardImage = require("../../../../../assets/images/listings/card/default-card-image.png");

    const uri = /* item.mainMedia?.trim() ?? */ "https://i.ibb.co/jvB2D8tJ/test-flat.jpg";
    const isRemote = !!uri && !imageFailed;

    const imageSource = isRemote ? { uri } : defaultCardImage;

  return (
    <Pressable onPress={onPress} onPressIn={pressIn} onPressOut={pressOut}>
      {({ pressed }) => (
        <Animated.View
          style={[s.cardShadowWrap, { transform: [{ scale }] }, pressed && { opacity: 0.95 }]}>
          <View style={s.mainListingCardContainer}>
            <View style={s.listingCardMainImageContainer}>
              <View style={s.listingTypeView}>
                <GradientText colors={["#2e9a1b", "#41f856"]} style={{ fontSize: 16, fontWeight: "700" }}>
                  RENT
                </GradientText>
              </View>
              <ListingFavoriteButton
                style={{
                  position: "absolute",
                  top: 8,
                  right: 8,
                  zIndex: 2,     
                }}
              />
              <Image
                source={imageSource}
                style={{ width: "100%", height: "100%" }}
                resizeMode={isRemote ? "cover" : "contain"} 
                onError={(e) => {
                  console.log("Image load error:", e.nativeEvent);
                  setImageFailed(true);
                }}
              />
            </View>

            <View style={s.listingCardInfoContainer}>
              <ListingPrice price={item.priceAmount} currencyCode={item.priceCurrency}></ListingPrice>
              <HorizontalDelimiter color="#9e9191" opacity={0.5} />
              <Text ellipsizeMode="tail" style={{ fontSize: 16, fontWeight: "600" }} numberOfLines={2}>
                {item.title}
              </Text>
            </View>
          </View>
        </Animated.View>
      )}
    </Pressable>
  );
}