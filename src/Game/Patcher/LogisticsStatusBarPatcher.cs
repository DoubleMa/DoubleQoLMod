using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.QoL.UI.Statusbar;
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
            AddMethod(Typ, "Mafi.Unity.UiFramework.IUnityUi.RegisterUi", this.GetHarmonyMethod("MyPostfix"));
        }

        private static void MyPostfix(IUnityUi __instance) {
            GetInstance<LogisticsStatusBarView>()?.OnClick(Typ.GetField<Action>(__instance, "m_onClick"));
        }
    }
}