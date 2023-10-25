using DoubleQoL.Extensions;
using Mafi.Core;
using Mafi.Core.GameLoop;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using UnityEngine;

namespace DoubleQoL.QoL.Controllers {

    internal abstract class AToggleController : AController {
        private readonly IGameLoopEvents _gameLoopEvents;

        protected abstract KeyBindings KeyBindings { get; }
        protected abstract bool DefaultState { get; }
        protected abstract string BtnIcon { get; }
        protected abstract float Order { get; }
        public abstract bool IsActive { get; }

        private ToggleBtn Btn;

        public AToggleController(IGameLoopEvents gameLoopEvents, StatusBar statusBar, ShortcutsManager shortcutsManager) : base(statusBar, shortcutsManager) {
            _gameLoopEvents = gameLoopEvents;
            if (IsEnabled) _gameLoopEvents.InputUpdate.AddNonSaveable(this, InputUpdate);
        }

        protected void Init() {
            if (IsEnabled && DefaultState != IsActive) Toggle();
        }

        protected void Toggle() {
            OnToggle();
            Btn?.SetIsOn(IsActive);
        }

        protected abstract void OnToggle();

        protected override void BuildUi(UiBuilder builder) {
            Vector2 size = new Vector2(builder.Style.StatusBar.PauseIconSize.x, 30f);
            Offset offset = builder.Style.StatusBar.PauseButtonOffset;
            Panel container = builder.NewPanel("container")
                .SetWidth(size.x + offset.LeftOffset + offset.RightOffset)
                .SetHeight(size.y);
            Panel parent = builder.NewPanel(string.Format("panel"))
                .PutTo(container, offset);

            Btn = builder.NewToggleBtn("toggleBtn")
                .SetBtnIcon(BtnIcon)
                .SetButtonStyleWhenOn(builder.Style.Toolbar.ButtonOn)
                .SetButtonStyleWhenOff(builder.Style.Toolbar.ButtonOff)
                .SetOnToggleAction(s => Toggle())
                .SetIsOn(IsActive)
                .PutRelativeTo(parent, size, HorizontalPosition.Left, VerticalPosition.Middle, Offset.Zero);

            _statusBar.AddElementToRight(container, Order, false);
            container.SetVisibility(true);
            parent.SetVisibility(true);
        }

        protected virtual void InputUpdate(GameTime time) {
            if (_shortcutsManager.IsDownNone(KeyBindings)) Toggle();
        }
    }
}