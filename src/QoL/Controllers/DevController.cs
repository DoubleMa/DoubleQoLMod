using DoubleQoL.Config;
using DoubleQoL.Game.Shortcuts;
using DoubleQoL.Global;
using DoubleQoL.QoL.Controllers;
using Mafi;
using Mafi.Core.GameLoop;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;

namespace DoubleQoL.Cheats.Tools
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, true)]
    public class DevController : AToggleController
    {
        protected override bool IsEnabled => ConfigManager.Instance.AppSettings_isDev.getBoolValue();
        protected override bool DefaultState => false;
        protected override KeyBindings KeyBindings => DoubleQoLShortcutsMap.Instance.DevKeyKb;
        protected override string BtnIcon => IconPaths.StatusBarDev;
        protected override float Order => StatusBarRightSideOrder.GAME_SPEED + 5;
        public override bool IsActive => false;

        public DevController(IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(gameLoopEvents, statusBar, shortcutsManager)
        {
            init();
        }

        protected override void OnToggle()
        { }
    }
}