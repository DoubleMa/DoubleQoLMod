using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.QoL.Shortcuts;
using Mafi;
using Mafi.Base;
using Mafi.Core.GameLoop;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;
using UnityEngine;

namespace DoubleQoL.QoL.Controllers {

    [GlobalDependency(RegistrationMode.AsAllInterfaces, false)]
    internal class FogController : AToggleController {
        private readonly Mafi.Unity.Weather.FogController _fogController;
        protected override bool IsEnabled => ConfigManager.Instance.QoLs_fog.Value;
        protected override bool DefaultState => ConfigManager.Instance.DefaultState_fog.Value;
        protected override KeyBindings KeyBindings => DoubleQoLShortcutsMap.Instance.DisableFogKb;
        protected override string BtnIcon => Assets.Base.Icons.Weather.Cloudy_svg;
        protected override float Order => StatusBarRightSideOrder.GAME_SPEED + 3;
        public override bool IsActive => _fogController != null && _fogController.GetField<GameObject>("m_fogQuad").gameObject.activeSelf;

        public FogController(Mafi.Unity.Weather.FogController fogController, IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(gameLoopEvents, statusBar, shortcutsManager) {
            _fogController = fogController;
            Init();
        }

        protected override void OnToggle() => SetFogEnabled(!IsActive);

        public void SetFogEnabled(bool enable) => _fogController.InvokeMethod("SetFogRenderingState", enable);
    }
}