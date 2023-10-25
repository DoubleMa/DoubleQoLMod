using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.QoL.Shortcuts;
using Mafi;
using Mafi.Core;
using Mafi.Core.GameLoop;
using Mafi.Core.Input;
using Mafi.Core.Simulation;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using Mafi.Unity.UserInterface.Style;
using System;
using UnityEngine;

namespace DoubleQoL.QoL.Controllers {

    [GlobalDependency(RegistrationMode.AsAllInterfaces, false)]
    internal class GameSpeedUi2 : AController {
        protected override bool IsEnabled => ConfigManager.Instance.QoLs_speed.Value;

        private readonly KeyBindings IncKeyBindings = DoubleQoLShortcutsMap.Instance.IncSpeedKB;
        private readonly KeyBindings DecKeyBindings = DoubleQoLShortcutsMap.Instance.DecSpeedKb;

        private readonly IInputScheduler _inputScheduler;

        private Txt speedLabel;
        private int currentSpeed = -1;
        private int tempSpeed = -1;
        private GameTime _gameTime;

        public GameSpeedUi2(IInputScheduler inputScheduler, IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(statusBar, shortcutsManager) {
            _inputScheduler = inputScheduler;
            if (IsEnabled) {
                gameLoopEvents.InputUpdate.AddNonSaveable(this, InputUpdate);
                gameLoopEvents.SyncUpdate.AddNonSaveable(this, new Action<GameTime>(SyncUpdate));
            }
        }

        protected override void BuildUi(UiBuilder builder) {
            int num = 3;
            UiStyle style = builder.Style;
            Vector2 size = new Vector2(builder.Style.StatusBar.PauseIconSize.x, 30f);
            float offset = style.StatusBar.PlayPauseSpacing;
            Panel container = builder.NewPanel("Speed")
                .SetWidth(num * size.x + (num - 1) * offset + style.StatusBar.PauseButtonOffset.LeftOffset + style.StatusBar.PauseButtonOffset.RightOffset)
                .SetHeight(size.y);
            Panel parent = builder.NewPanel(string.Format("{0} speed", currentSpeed))
                .PutTo(container, style.StatusBar.PauseButtonOffset);
            Btn decBtn = builder.NewBtnGeneral("DecSpeedBtn")
                .SetText("-")
                .SetButtonStyle(style.Global.MinusPrimaryBtn.ExtendText(ColorRgba.Black, FontStyle.Bold, 16, true))
                .OnClick(() => DecSpeed())
                .PutRelativeTo(parent, style.StatusBar.PauseIconSize, HorizontalPosition.Left, VerticalPosition.Middle, GetLeftOffset(0, size, offset));
            speedLabel = builder.NewTxt("0")
                .SetTextStyle(builder.Style.Global.TextControls)
                .SetAlignment(TextAnchor.MiddleCenter)
                .PutRelativeTo(parent, size, HorizontalPosition.Left, VerticalPosition.Middle, GetLeftOffset(1, size, offset));
            Btn incBtn = builder.NewBtnGeneral("IncSpeedBtn")
            .SetText("+")
            .SetButtonStyle(style.Global.MinusPrimaryBtn.Extend(null, null, ColorRgba.Green).ExtendText(ColorRgba.White, FontStyle.Bold, 14, true))
            .OnClick(() => IncSpeed())
            .PutRelativeTo(parent, style.StatusBar.PauseIconSize, HorizontalPosition.Left, VerticalPosition.Middle, GetLeftOffset(2, size, offset));

            _statusBar.AddElementToRight(container, StatusBarRightSideOrder.GAME_SPEED + 1, false);
            container.SetVisibility(true);
            parent.SetVisibility(true);
        }

        private Offset GetLeftOffset(int index, Vector2 size, float offset) => Offset.Left((size.x + offset) * index);

        private void SetSpeed(int i) {
            if (_gameTime != null && _gameTime.IsGamePaused) _inputScheduler.ScheduleInputCmd(new SetSimPauseStateCmd(false));
            if (i == 0) _inputScheduler.ScheduleInputCmd(new SetSimPauseStateCmd(true));
            else {
                int temp = i.Between(0, 10);
                if (temp == tempSpeed) return;
                tempSpeed = temp;
                _inputScheduler.ScheduleInputCmd(new GameSpeedChangeCmd(temp));
            }
        }

        private void IncSpeed() => SetSpeed((tempSpeed + 1).Between(0, 10));

        private void DecSpeed() => SetSpeed((tempSpeed - 1).Between(0, 10));

        private void SyncUpdate(GameTime gameTime) {
            if (_gameTime != gameTime) _gameTime = gameTime;
            if (gameTime.GameSpeedMult == currentSpeed) return;
            currentSpeed = gameTime.GameSpeedMult;
            tempSpeed = currentSpeed;
            speedLabel.SetText(currentSpeed.ToString());
        }

        private void InputUpdate(GameTime time) {
            if (_shortcutsManager.IsDownNone(IncKeyBindings)) IncSpeed();
            else if (_shortcutsManager.IsDownNone(DecKeyBindings)) DecSpeed();
        }
    }
}