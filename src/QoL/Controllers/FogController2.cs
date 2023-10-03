using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Game.Shortcuts;
using DoubleQoL.Global;
using Mafi;
using Mafi.Core.GameLoop;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.Weather;

namespace DoubleQoL.QoL.Controllers {

    [GlobalDependency(RegistrationMode.AsAllInterfaces, false)]
    internal class FogController2 : AToggleController {
        private readonly FogController _fogController;
        protected override bool IsEnabled => ConfigManager.Instance.QoLs_fog.Value;
        protected override bool DefaultState => ConfigManager.Instance.DefaultState_fog.Value;
        protected override KeyBindings KeyBindings => DoubleQoLShortcutsMap.Instance.DisableFogKb;
        protected override string BtnIcon => IconPaths.Status_BarFog;
        protected override float Order => StatusBarRightSideOrder.GAME_SPEED + 3;
        public override bool IsActive => _fogController != null && _fogController.IsFogEnabled();

        public FogController2(FogController fogController, IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(gameLoopEvents, statusBar, shortcutsManager) {
            _fogController = fogController;
            Init();
        }

        protected override void OnToggle() => _fogController.ToggleFog();
    }
}