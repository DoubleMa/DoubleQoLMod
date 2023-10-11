using DoubleQoL.Config;
using DoubleQoL.Game.Patcher.Helper;
using HarmonyLib;
using Mafi.Unity.UiFramework;
using System;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    internal class LogisticsStatusBarPatcher : APatcher {
        public static readonly LogisticsStatusBarPatcher Instance = new LogisticsStatusBarPatcher();
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_statusbar.Value;
        private static Type Typ;

        public LogisticsStatusBarPatcher() : base("LogisticsStatusBar") {
            Typ = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.Levelling.LogisticsStatusBarView");
            AddBlockedMethod(AccessTools.Method(Typ, "Mafi.Unity.UiFramework.IUnityUi.RegisterUi"), AccessTools.Method(GetType(), "MyPostfix"));
        }

        private static void MyPostfix(IUnityUi __instance) {
            PatcherHelper.Instance.LogisticsStatusBarView.OnClick((Action)AccessTools.Field(Typ, "m_onClick").GetValue(__instance));
        }
    }
}