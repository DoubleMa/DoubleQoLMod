using DoubleQoL.Config;
using DoubleQoL.Global;
using DoubleQoL.QoL.Shortcuts;
using Mafi;
using Mafi.Core.GameLoop;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;

namespace DoubleQoL.QoL.Controllers {

    [GlobalDependency(RegistrationMode.AsAllInterfaces, true)]
    internal class DevController : AToggleController {
        protected override bool IsEnabled => ConfigManager.Instance.AppSettings_isDev.Value;
        protected override bool DefaultState => false;
        protected override KeyBindings KeyBindings => DoubleQoLShortcutsMap.Instance.DevKeyKb;
        protected override string BtnIcon => IconPaths.Status_BarDev;
        protected override float Order => StatusBarRightSideOrder.GAME_SPEED + 5;
        public override bool IsActive => false;

        public DevController(IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(gameLoopEvents, statusBar, shortcutsManager) {
            Init();
        }

        protected override void OnToggle() {
        }
    }
}