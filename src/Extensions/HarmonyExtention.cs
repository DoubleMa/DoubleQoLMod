using HarmonyLib;
using System;
using System.Reflection;

namespace DoubleQoL.Extensions {

    internal static class HarmonyExtention {

        public static HarmonyMethod ToHarmonyMethod(this MethodInfo m) => new HarmonyMethod(m);

        public static HarmonyMethod GetHarmonyMethod(this Type t, string method) => new HarmonyMethod(t, method);

        public static HarmonyMethod GetHarmonyMethod<T>(this T t, string method) => new HarmonyMethod(t.GetType(), method);

        public static FieldInfo GetFieldT(this Type t, string field) => AccessTools.Field(t, field);

        public static T GetField<T>(this Type t, object __instance, string field) => (T)t.GetFieldT(field)?.GetValue(__instance);

        public static void SetField<T>(this Type t, object __instance, string field, T value) => t.GetFieldT(field)?.SetValue(__instance, value);

        public static U GetFieldRef<T, U>(this T __instance, string field) => AccessTools.FieldRefAccess<T, U>(__instance, field);
    }
}