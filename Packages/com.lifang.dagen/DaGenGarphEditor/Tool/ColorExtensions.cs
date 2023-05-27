using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace DaGenGraph.Editor
{
    public static class ColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        public static Color ColorFrom256(this Color color, float r, float g, float b, float a = 255)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
        public static Color Lighter(this Color color)
        {
            return new Color(color.r + 0.0625f,color.g + 0.0625f,color.b + 0.0625f,color.a);
        }
        public static Color Darker(this Color color)
        {
            return new Color(color.r - 0.0625f,color.g - 0.0625f,color.b - 0.0625f,color.a);
        }
        public static Color FromHex(this Color color, string hexValue, float alpha = 1)
        {
            if (string.IsNullOrEmpty(hexValue)) return Color.clear;

            if (hexValue[0] == '#') hexValue = hexValue.TrimStart('#');
            if (hexValue.Length > 6) hexValue = hexValue.Remove(6, hexValue.Length - 6);

            var value = int.Parse(hexValue, (System.Globalization.NumberStyles) NumberStyles.HexNumber);
            var r = value >> 16 & 255;
            var g = value >> 8 & 255;
            var b = value & 255;
            var a = 255 * alpha;
            return new Color().ColorFrom256(r, g, b, a);
        }
        [Flags]
        [ComVisible(true)]
        [Serializable]
        public enum NumberStyles
        {
            None = 0,
            AllowLeadingWhite = 1,
            AllowTrailingWhite = 2,
            AllowLeadingSign = 4,
            AllowTrailingSign = 8,
            AllowParentheses = 16, // 0x00000010
            AllowDecimalPoint = 32, // 0x00000020
            AllowThousands = 64, // 0x00000040
            AllowExponent = 128, // 0x00000080
            AllowCurrencySymbol = 256, // 0x00000100
            AllowHexSpecifier = 512, // 0x00000200
            Integer = AllowLeadingSign | AllowTrailingWhite | AllowLeadingWhite, // 0x00000007
            HexNumber = AllowHexSpecifier | AllowTrailingWhite | AllowLeadingWhite, // 0x00000203
            Number = Integer | AllowThousands | AllowDecimalPoint | AllowTrailingSign, // 0x0000006F
            Float = Integer | AllowExponent | AllowDecimalPoint, // 0x000000A7
            Currency = Number | AllowCurrencySymbol | AllowParentheses, // 0x0000017F
            Any = Currency | AllowExponent, // 0x000001FF
        }
    }
}

