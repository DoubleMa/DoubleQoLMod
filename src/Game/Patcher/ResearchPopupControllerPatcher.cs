using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.QoL.UI.Statusbar;
using Mafi.Unity.InputControl.Toolbar.MenuPopup;

namespace DoubleQoL.Game.Patcher {

    internal class ResearchPopupControllerPatcher : APatcher {
        public static readonly ResearchPopupControllerPatcher Instance = new ResearchPopupControllerPatcher();
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_statusbar.Value;

        public ResearchPopupControllerPatcher() : base("ResearchPopup") {
            AddMethod<ResearchPopupController>("Activate", this.GetHarmonyMethod("MyPostActivatefix"), true);
            AddMethod<ResearchPopupController>("Deactivate", this.GetHarmonyMethod("MyPostDeactivatefix"), true);
        }

        private static void MyPostActivatefix() {
            GetInstance<PopulationStatusBarView>()?.InfoTileExp.ToHideListener(true);
            GetInstance<LogisticsStatusBarView>()?.InfoTileExp.ToHideListener(true);
            GetInstance<UnityStatusBarView>()?.InfoTileExp.ToHideListener(true);
        }

        private static void MyPostDeactivatefix() {
            GetInstance<PopulationStatusBarView>()?.InfoTileExp.ToHideListener(false);
            GetInstance<LogisticsStatusBarView>()?.InfoTileExp.ToHideListener(false);
            GetInstance<UnityStatusBarView>()?.InfoTileExp.ToHideListener(false);
        }
    }
}