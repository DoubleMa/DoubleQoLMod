using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Game.Shortcuts;
using DoubleQoL.Global;
using DoubleQoL.QoL.Controllers;
using Mafi;
using Mafi.Core.GameLoop;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.Weather;

namespace DoubleQoL.Cheats.Tools
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false)]
    public class FogController2 : AToggleController
    {
        private readonly FogController _fogController;
        protected override bool IsEnabled => ConfigManager.Instance.QoLs_fog.getBoolValue();
        protected override bool DefaultState => ConfigManager.Instance.DefaultState_fog.getBoolValue();
        protected override KeyBindings KeyBindings => DoubleQoLShortcutsMap.Instance.DisableFogKb;
        protected override string BtnIcon => IconPaths.StatusBarFog;
        protected override float Order => StatusBarRightSideOrder.GAME_SPEED + 3;
        public override bool IsActive => _fogController != null && _fogController.IsFogEnabled();

        public FogController2(FogController fogController, IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(gameLoopEvents, statusBar, shortcutsManager)
        {
            _fogController = fogController;
            init();
        }

        protected override void OnToggle() => _fogController.ToggleFog();
    }
}