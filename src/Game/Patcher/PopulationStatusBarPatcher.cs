using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.QoL.UI.Statusbar;
using Mafi.Unity.UiFramework;
using System;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    internal class PopulationStatusBarPatcher : APatcher<PopulationStatusBarPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_statusbar.Value;
        private static Type Typ;

        public PopulationStatusBarPatcher() : base("PopulationStatusBar") {
            Typ = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.Levelling.PopulationStatusBarView");
            AddMethod(Typ, "Mafi.Unity.UiFramework.IUnityUi.RegisterUi", this.GetHarmonyMethod("MyPostfix"));
        }

        private static void MyPostfix(IUnityUi __instance) {
            Action onCLick = __instance.GetField<Action>("m_onClick");
            GetResolvedInstance<PopulationStatusBarView>()?.OnClick(onCLick);
            GetResolvedInstance<UnityStatusBarView>()?.OnClick(onCLick);
        }
    }
}