import React from "react";
import { Text, TextProps, StyleProp, TextStyle, ColorValue } from "react-native";
import MaskedView from "@react-native-masked-view/masked-view";
import { LinearGradient } from "expo-linear-gradient";

type GradientColors = readonly [ColorValue, ColorValue, ...ColorValue[]];

type GradientTextProps = Omit<TextProps, "style"> & {
  colors?: GradientColors;
  start?: { x: number; y: number };
  end?: { x: number; y: number };
  style?: StyleProp<TextStyle>;
};

export default function GradientText({
  colors = ["red", "green"] as const,
  start = { x: 0, y: 0 },
  end = { x: 1, y: 0 },
  style,
  ...textProps
}: GradientTextProps) {
  return (
    <MaskedView maskElement={<Text {...textProps} style={style} />}>
      <LinearGradient colors={colors} start={start} end={end}>
        <Text {...textProps} style={[style, { opacity: 0 }]} />
      </LinearGradient>
    </MaskedView>
  );
}