using DoubleQoL.Config;
using DoubleQoL.Global;
using DoubleQoL.QoL.UI.Statusbar.Component;
using Mafi.Core.GameLoop;
using Mafi.Core.Syncers;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Statusbar {

    internal abstract class AStatusBarView : IUnityUi {
        private readonly IGameLoopEvents _gameLoop;
        private readonly StatusBar _statusBar;
        private IUiUpdater _updater;
        private Action _onClick;
        protected Vector2 Size = Static.InfoTileSize;
        internal InfoTileExpended InfoTileExp;
        protected abstract float Order { get; }
        protected abstract SyncFrequency SyncFreq { get; }
        protected abstract bool IsEnabled { get; }

        public AStatusBarView(IGameLoopEvents gameLoop, StatusBar statusBar) : base() {
            _gameLoop = gameLoop;
            _statusBar = statusBar;
        }

        public void OnClick(Action onClick) {
            _onClick = onClick;
            SetOnClick();
        }

        private void SetOnClick() {
            if (_onClick is null || InfoTileExp is null) return;
            InfoTileExp.OnClick(_onClick);
        }

        protected abstract void OnRegisteringUi(UiBuilder builder, UpdaterBuilder updaterBuilder);

        void IUnityUi.RegisterUi(UiBuilder builder) {
            if (!ConfigManager.Instance.QoLs_statusbar.Value || !IsEnabled) return;
            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();
            OnRegisteringUi(builder, updaterBuilder);
            SetOnClick();
            _updater = updaterBuilder.Build(SyncFreq);
            _gameLoop.SyncUpdate.AddNonSaveable(this, x => _updater.SyncUpdate());
            _gameLoop.RenderUpdate.AddNonSaveable(this, x => _updater.RenderUpdate());
            _statusBar.AddLargeTileToLeft(InfoTileExp, Size.x, Order);
        }
    }
}