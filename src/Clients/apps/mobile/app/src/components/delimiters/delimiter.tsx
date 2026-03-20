import React from "react";
import { View, StyleProp, ViewStyle } from "react-native";

type DividerProps = {
  thickness?: number;        
  color?: string;             
  marginVertical?: number;     
  marginHorizontal?: number;   
  opacity?: number,
  style?: StyleProp<ViewStyle>;
};

export function HorizontalDelimiter({
  thickness = 1,
  color = "rgba(0,0,0,0.08)",
  marginVertical = 0,
  marginHorizontal = 0,
  opacity = 1,
  style,
}: DividerProps) {
  return (
    <View
      style={[
        {
          height: thickness,
          backgroundColor: color,
          marginVertical,
          marginHorizontal,
          alignSelf: "stretch",
          opacity: opacity
        },
        style,
      ]}
    />
  );
}