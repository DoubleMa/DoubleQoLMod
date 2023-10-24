using Mafi.Unity;
using Mafi.Unity.Weather;
using System;
using System.Reflection;
using UnityEngine;

namespace DoubleQoL.Extensions {

    internal static class FogControllerExtention {

        private static Type GetFogControllerType() => typeof(UnityMod).Assembly.GetType("Mafi.Unity.Weather.FogController") ?? throw new Exception("Couldn't find the FogController type.");

        public static bool IsFogEnabled(this FogController fogController) => GetFogGameObject(fogController).gameObject.activeSelf;

        private static GameObject GetFogGameObject(this FogController fogController) => (GameObject)GetFogControllerType().GetField("m_fogQuad", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(fogController);

        public static void SetFogEnabled(this FogController fogController, bool enable) => GetFogControllerType().GetMethod("SetFogRenderingState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(fogController, new object[] { enable });

        public static void ToggleFog(this FogController fogController) => SetFogEnabled(fogController, !IsFogEnabled(fogController));
    }
}