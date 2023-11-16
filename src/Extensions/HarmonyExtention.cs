﻿using HarmonyLib;
using System;
using System.Reflection;

namespace DoubleQoL.Extensions {

    internal static class HarmonyExtention {

        public static HarmonyMethod ToHarmonyMethod(this MethodInfo m) => new HarmonyMethod(m);

        public static HarmonyMethod GetHarmonyMethod(this Type t, string method) => new HarmonyMethod(t, method);

        public static HarmonyMethod GetHarmonyMethod<T>(this T t, string method) => GetHarmonyMethod(t.GetType(), method);

        public static FieldInfo ATGetField(this Type type, string fieldName) => AccessTools.Field(type, fieldName) ?? throw new MissingFieldException($"Field '{fieldName}' not found in type '{type.FullName}'.");

        public static MethodInfo ATGetMethod(this Type type, string methodName) => AccessTools.Method(type, methodName) ?? throw new MissingMethodException($"Method '{methodName}' not found in type '{type.FullName}'.");

        public static MethodInfo ATGetPropertyGetter(this Type type, string propertyName) => AccessTools.PropertyGetter(type, propertyName) ?? throw new MissingFieldException($"Property getter for '{propertyName}' not found in type '{type.FullName}'.");

        public static MethodInfo ATGetPropertySetter(this Type type, string propertyName) => AccessTools.PropertySetter(type, propertyName) ?? throw new MissingFieldException($"Property setter for '{propertyName}' not found in type '{type.FullName}'.");

        public static T GetField<T>(this Type type, object instance, string fieldName) => (T)type.ATGetField(fieldName).GetValue(instance);

        public static T GetField<T>(this object instance, string fieldName) => instance.GetType().GetField<T>(instance, fieldName);

        public static void SetField<T>(this Type type, object instance, string fieldName, T value) => type.ATGetField(fieldName).SetValue(instance, value);

        public static void SetField<T>(this object instance, string fieldName, T value) => instance.GetType().SetField(instance, fieldName, value);

        public static U GetFieldRef<T, U>(this T instance, string fieldName) => AccessTools.FieldRefAccess<T, U>(instance, fieldName);

        public static void InvokeMethod(this object instance, string methodName, params object[] parameters) => instance.GetType().ATGetMethod(methodName).Invoke(instance, parameters);

        public static U InvokeMethod<U>(this object instance, string methodName, params object[] parameters) => (U)instance.GetType().ATGetMethod(methodName).Invoke(instance, parameters);

        public static void InvokeStaticMethod(this Type type, string methodName, params object[] parameters) => type.ATGetMethod(methodName).Invoke(null, parameters);

        public static U InvokeStaticMethod<U>(this Type type, string methodName, params object[] parameters) => (U)type.ATGetMethod(methodName).Invoke(null, parameters);

        public static T InvokeGetter<T>(this object instance, string propertyName) => (T)instance.GetType().ATGetPropertyGetter(propertyName).Invoke(instance, null);

        public static void InvokeSetter<T>(this object instance, string propertyName, T value) => instance.GetType().ATGetPropertySetter(propertyName).Invoke(instance, new object[] { value });
    }
}