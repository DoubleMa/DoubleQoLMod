using DoubleQoL.Config;
using DoubleQoL.Game.Patcher.Helper;
using HarmonyLib;
using Mafi.Unity.InputControl.Toolbar.MenuPopup;

namespace DoubleQoL.Game.Patcher {

    internal class ResearchPopupControllerPatcher : APatcher {
        public static readonly ResearchPopupControllerPatcher Instance = new ResearchPopupControllerPatcher();
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_statusbar.Value;

        public ResearchPopupControllerPatcher() : base("ResearchPopup") {
            AddAllowedMethod(AccessTools.Method(typeof(ResearchPopupController), "Activate"), AccessTools.Method(GetType(), "MyPostActivatefix"));
            AddAllowedMethod(AccessTools.Method(typeof(ResearchPopupController), "Deactivate"), AccessTools.Method(GetType(), "MyPostDeactivatefix"));
        }

        private static void MyPostActivatefix() {
            PatcherHelper.Instance.PopulationStatusBarView.InfoTileExp.ToHideListener(true);
            PatcherHelper.Instance.LogisticsStatusBarView.InfoTileExp.ToHideListener(true);
            PatcherHelper.Instance.UnityStatusBarView.InfoTileExp.ToHideListener(true);
        }

        private static void MyPostDeactivatefix() {
            PatcherHelper.Instance.PopulationStatusBarView.InfoTileExp.ToHideListener(false);
            PatcherHelper.Instance.LogisticsStatusBarView.InfoTileExp.ToHideListener(false);
            PatcherHelper.Instance.UnityStatusBarView.InfoTileExp.ToHideListener(false);
        }
    }
}