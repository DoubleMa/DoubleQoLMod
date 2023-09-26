using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Game.Shortcuts;
using DoubleQoL.QoL.Controllers;
using Mafi;
using Mafi.Core.GameLoop;
using Mafi.Unity;
using Mafi.Unity.Camera;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;

namespace DoubleQoL.Cheats.Tools
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false)]
    public class CameraController : AToggleController
    {
        private readonly OrbitalCameraModel _cameraModel;
        protected override bool IsEnabled => ConfigManager.Instance.QoLs_camera.getBoolValue();
        protected override bool DefaultState => ConfigManager.Instance.DefaultState_freeCamera.getBoolValue();
        protected override KeyBindings KeyBindings => DoubleQoLShortcutsMap.Instance.FreeCameraKb;
        protected override string BtnIcon => Assets.Unity.UserInterface.Cursors.FreeLook32_png;
        protected override float Order => StatusBarRightSideOrder.GAME_SPEED + 2;
        public override bool IsActive => _cameraModel != null && _cameraModel.CameraMode == CameraMode.Unconstrained;

        public CameraController(OrbitalCameraModel cameraModel, IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(gameLoopEvents, statusBar, shortcutsManager)
        {
            _cameraModel = cameraModel;
            init();
        }

        protected override void OnToggle()
        {
            if (_cameraModel is null) return;
            bool b = !IsActive;
            _cameraModel.SetMode(b ? CameraMode.Unconstrained : CameraMode.DefaultGameplay);
            if (b) _cameraModel.SetValues();
        }
    }
}