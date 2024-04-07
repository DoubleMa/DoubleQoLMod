using DoubleQoL.Config;
using DoubleQoL.Global;
using DoubleQoL.QoL.UI.Statusbar.Component;
using Mafi.Core.GameLoop;
using Mafi.Core.Syncers;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Statusbar {

    internal abstract class AStatusBarView : IStatusBarItem {
        private readonly IGameLoopEvents _gameLoop;
        private readonly StatusBar _statusBar;
        protected IUiUpdater _updater;
        protected UpdaterBuilder _updaterBuilder;
        protected UiBuilder _builder;
        private Action _onClick;
        protected Vector2 Size = Static.InfoTileSize;
        internal InfoTileExpended InfoTileExp;
        protected abstract float Order { get; }
        protected abstract SyncFrequency SyncFreq { get; }
        protected abstract bool IsEnabled { get; }

        public AStatusBarView(IGameLoopEvents gameLoop, StatusBar statusBar, UiBuilder builder) : base() {
            _gameLoop = gameLoop;
            _statusBar = statusBar;
            _builder = builder;
        }

        public void OnClick(Action onClick) {
            _onClick = onClick;
            SetOnClick();
        }

        private void SetOnClick() {
            if (_onClick is null || InfoTileExp is null) return;
            InfoTileExp.OnClick(_onClick);
        }

        protected abstract void RegisterIntoStatusBar(StatusBar statusBar);

        void IStatusBarItem.RegisterIntoStatusBar(StatusBar statusBar) {
            if (!ConfigManager.Instance.QoLs_statusbar.Value || !IsEnabled) return;
            RegisterIntoStatusBar(statusBar);
            SetOnClick();
            _updater = _updaterBuilder.Build(SyncFreq);
            _gameLoop.SyncUpdate.AddNonSaveable(this, x => _updater.SyncUpdate());
            _gameLoop.RenderUpdate.AddNonSaveable(this, x => _updater.RenderUpdate());
            _statusBar.AddLargeTileToLeft(InfoTileExp, Size.x, Order);
        }
    }
}