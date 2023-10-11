using DoubleQoL.Extensions;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;

namespace DoubleQoL.QoL.Tools {

    internal abstract class ASelectableEntityTool : ABaseEntityCursorInputController<IAreaSelectableEntity> {
        protected readonly ToolbarController _toolbarController;
        protected AToolbox Toolbox { get; private set; }
        protected virtual bool IgnoreModifiers => true;

        protected ASelectableEntityTool(
            ProtosDb protosDb,
            UnlockedProtosDbForUi unlockedProtosDb,
            ShortcutsManager shortcutsManager,
            IUnityInputMgr inputManager,
            CursorPickingManager cursorPickingManager,
            CursorManager cursorManager,
            AreaSelectionToolFactory areaSelectionToolFactory,
            IEntitiesManager entitiesManager,
            NewInstanceOf<EntityHighlighter> highlighter,
            ToolbarController toolbarController,
            TerrainCursor terrainCursor,
            Lyst<ToolToggleBtn> toolToggleBtns = null) :
            base(protosDb, unlockedProtosDb, shortcutsManager, inputManager, cursorPickingManager, cursorManager, areaSelectionToolFactory, entitiesManager, highlighter, terrainCursor, Option.None, null) {
            _toolbarController = toolbarController;
            SetToolbox(toolToggleBtns);
        }

        protected void SetToolbox(Lyst<ToolToggleBtn> toolToggleBtns) {
            if (!(toolToggleBtns is null || toolToggleBtns.IsEmpty)) Toolbox = new AToolbox(_toolbarController, toolToggleBtns);
        }

        public override void Activate() {
            base.Activate();
            Toolbox?.Show();
        }

        public override void Deactivate() {
            base.Deactivate();
            Toolbox?.Hide();
        }

        public override void RegisterUi(UiBuilder builder) {
            Toolbox?.RegisterUi(builder);
            base.RegisterUi(builder);
        }

        public override bool InputUpdate(IInputScheduler inputScheduler) {
            Toolbox?.CheckIsOn(IgnoreModifiers);
            return base.InputUpdate(inputScheduler);
        }

        protected override bool OnFirstActivated(IAreaSelectableEntity hoveredEntity, Lyst<IAreaSelectableEntity> selectedEntities, Lyst<SubTransport> selectedPartialTransports) => false;

        protected class ToolToggleBtn {
            public string Name { get; }
            public string Icon { get; }
            public bool ForceIgnoreModifier { get; }
            public Action<bool> OnClick { get; }
            public KeyBindings KeyBindings { get; }
            public string Tooltip { get; }
            public ToggleBtn ToggleBtn { get; set; }

            public ToolToggleBtn(string name, string icon, Action<bool> onClick, KeyBindings keyBindings, string tooltip, bool forceIgnoreModifier = false) {
                Name = name;
                Icon = icon;
                OnClick = onClick;
                KeyBindings = keyBindings;
                Tooltip = tooltip;
                ForceIgnoreModifier = forceIgnoreModifier;
            }
        }

        protected class AToolbox : Toolbox, IUnityUi {
            private readonly Lyst<ToolToggleBtn> ToolToggleBtns;

            public AToolbox(ToolbarController toolbar, Lyst<ToolToggleBtn> toolToggleBtns) : base(toolbar) {
                ToolToggleBtns = toolToggleBtns;
            }

            protected override void BuildCustomItems(UiBuilder builder) {
                ToolToggleBtns.ForEach(b => b.ToggleBtn = AddToggleButton(b.Name, b.Icon, b.OnClick, m => b.KeyBindings, new LocStrFormatted(b.Tooltip)));
                Toolbar.AddToolbox(this, this.GetWidth());
            }

            public void CheckIsOn(bool ignoreModifier = false) => ToolToggleBtns.ForEach(b => b.ToggleBtn?.SetIsOn(b.KeyBindings.IsPrimaryOn(ignoreModifier || b.ForceIgnoreModifier)));
        }
    }
}