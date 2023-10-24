using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DoubleQoL.Extensions {

    internal static class RandomExtention {

        public static int Between(this int x, int min, int max) => Math.Max(min, Math.Min(max, x));

        public static string GetSignIfPositive(this float x) => x > 0 ? "+" : "";

        public static string AddSign(this float x) => x.GetSignIfPositive() + x;

        public static string AddSignAndFormat(this float x, int decimals = 2) => x.GetSignIfPositive() + x.ToString($"F{decimals}");

        public static string CleanString(this string str) => Regex.Replace(str, "[^a-zA-Z0-9]", "");

        public static string AbbreviateWithDot(this string str, int maxLength) => string.IsNullOrEmpty(str) || str.Length <= Math.Max(2, maxLength) ? str : str.Substring(0, Math.Max(2, maxLength - 1)) + ".";

        public static Vector2 Extend(this Vector2 v, float? x = null, float? y = null) => new Vector2(v.x * (x ?? 1), v.y * (y ?? 1));

        public static Vector2 Modify(this Vector2 v, float? x = null, float? y = null) => new Vector2(x ?? v.x, y ?? v.y);
    }
}