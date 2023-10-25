using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.QoL.UI.Statusbar;
using Mafi.Unity.InputControl.Toolbar.MenuPopup;

namespace DoubleQoL.Game.Patcher {

    internal class ResearchPopupControllerPatcher : APatcher<ResearchPopupControllerPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_statusbar.Value;

        public ResearchPopupControllerPatcher() : base("ResearchPopup") {
            AddMethod<ResearchPopupController>("Activate", this.GetHarmonyMethod("MyPostActivatefix"), true);
            AddMethod<ResearchPopupController>("Deactivate", this.GetHarmonyMethod("MyPostDeactivatefix"), true);
        }

        private static void MyPostActivatefix() {
            GetResolvedInstance<PopulationStatusBarView>()?.InfoTileExp.ToHideListener(true);
            GetResolvedInstance<LogisticsStatusBarView>()?.InfoTileExp.ToHideListener(true);
            GetResolvedInstance<UnityStatusBarView>()?.InfoTileExp.ToHideListener(true);
        }

        private static void MyPostDeactivatefix() {
            GetResolvedInstance<PopulationStatusBarView>()?.InfoTileExp.ToHideListener(false);
            GetResolvedInstance<LogisticsStatusBarView>()?.InfoTileExp.ToHideListener(false);
            GetResolvedInstance<UnityStatusBarView>()?.InfoTileExp.ToHideListener(false);
        }
    }
}