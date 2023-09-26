using DoubleQoL.Config;
using DoubleQoL.Game.Patcher;
using DoubleQoL.Game.Shortcuts;
using Mafi;
using Mafi.Core.GameLoop;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;

namespace DoubleQoL.QoL.Controllers
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false)]
    public class CollapseController : AToggleController
    {
        protected override bool IsEnabled => ConfigManager.Instance.QoLs_collapse.getBoolValue();
        protected override bool DefaultState => ConfigManager.Instance.DefaultState_collapse.getBoolValue();
        protected override KeyBindings KeyBindings => DoubleQoLShortcutsMap.Instance.DisableCollapseKb;
        protected override string BtnIcon => Assets.Unity.UserInterface.EntityIcons.BuildingCollapse_svg;
        protected override float Order => StatusBarRightSideOrder.GAME_SPEED + 4;
        public override bool IsActive => CollapsePatcher.Instance.isActive;

        public CollapseController(IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(gameLoopEvents, statusBar, shortcutsManager)
        {
            init();
        }

        protected override void OnToggle() => CollapsePatcher.Instance.Toggle();
    }
}