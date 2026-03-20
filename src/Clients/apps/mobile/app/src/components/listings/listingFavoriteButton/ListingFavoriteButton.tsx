import React, { useState, useRef, useMemo } from "react";
import { Animated, StyleProp, ViewStyle, Pressable, View } from "react-native";
import { listingFavortieButtonStyles } from "./listingFavoriteButtonStyles";

type Props = {
  initial?: boolean;
  onChange?: (value: boolean) => void;
  style?: StyleProp<ViewStyle>;
};

  const filledHeart = require("./../../../../../assets/images/listings/favorite/heart-fill-purple.png");
  const thinHeart = require("./../../../../../assets/images/listings/favorite/heart-thin-purple.png");

export function ListingFavoriteButton({ initial, onChange, style }: Props) {
  const [buttonState, setButtonState] = useState(initial);
  const scale = useRef(new Animated.Value(1)).current;

  const icon = useMemo(() => (buttonState ? filledHeart : thinHeart), [buttonState]);

  const toggle = () => {
    const next = !buttonState;
    setButtonState(next);
    onChange?.(next);
    if(next)
        scale.setValue(0);

    Animated.sequence([
      Animated.spring(scale, { toValue: 1.15, useNativeDriver: true, speed: 30, bounciness: 6 }),
      Animated.spring(scale, { toValue: 1.0, useNativeDriver: true, speed: 30, bounciness: 6 }),
    ]).start();
  }

  return (
    <View style={[listingFavortieButtonStyles.mainListingFavoriteButtonContainer, style]}>
        <Pressable onPress={toggle} hitSlop={10}>
        <Animated.Image
            source={icon}
            style={{ width: "auto", height: 28, transform: [{ scale }] }}
            resizeMode="contain"/>
        </Pressable>
    </View>
  );
}