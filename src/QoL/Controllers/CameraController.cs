﻿using DoubleQoL.Extensions;
using DoubleQoL.QoL.Shortcuts;
using DoubleQoL.XML.config;
using Mafi;
using Mafi.Core.GameLoop;
using Mafi.Unity;
using Mafi.Unity.Camera;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;
using System;

namespace DoubleQoL.QoL.Controllers {

    [GlobalDependency(RegistrationMode.AsAllInterfaces, false)]
    internal class CameraController : AToggleController {
        private readonly OrbitalCameraModel _cameraModel;
        protected override bool IsEnabled => ConfigManager.Instance.QoLs_camera.Value;
        protected override bool DefaultState => ConfigManager.Instance.DefaultState_freeCamera.Value;
        protected override KeyBindings KeyBindings => DoubleQoLShortcutsMap.Instance.FreeCameraKb;
        protected override string BtnIcon => Assets.Unity.UserInterface.Cursors.FreeLook32_png;
        protected override float Order => StatusBarRightSideOrder.GAME_SPEED + 2;
        public override bool IsActive => _cameraModel != null && _cameraModel.CameraMode == CameraMode.Unconstrained;

        public CameraController(OrbitalCameraModel cameraModel, IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(gameLoopEvents, statusBar, shortcutsManager) {
            _cameraModel = cameraModel;
            Init();
        }

        protected override void OnToggle() {
            if (_cameraModel is null) return;
            try {// Lazy Fix, DefaultState may be useless now
                if (IsActive) _cameraModel.SetMode(CameraMode.DefaultGameplay);
                else {
                    _cameraModel.SetMode(CameraMode.Unconstrained);
                    _cameraModel.SetField("m_maxPivotDistance", new RelTile1f(6000));
                    _cameraModel.SetField("m_minHeightAboveTerrain", new RelTile1f(-100));
                }
            }
            catch (Exception ex) {
                Logging.Log.Exception(ex, "SetMode Failed at OnToggle in DoubleQoL.QoL.Controllers.CameraController. Ignore this error if you see it ;)");
            }
        }
    }
}