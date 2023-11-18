using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Factory;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.InputControl.Tools;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using System;
using System.Linq;
using UnityEngine;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Blueprints.BlueprintCreationController"/>.
    /// </summary>
    [GlobalDependency(RegistrationMode.AsEverything, false, false)]
    internal class BlueprintCreationController : BaseEntityCursorInputController<IStaticEntity> {
        private static readonly ColorRgba HOVER_HIGHLIGHT = new ColorRgba(16766738);

        private readonly EntitiesCloneConfigHelper m_configCloneHelper;
        private readonly IUnityInputMgr m_inputManager;
        private readonly BlueprintToolbox m_toolbox;
        private AudioSource m_selectSound;
        private Option<Action<ImmutableArray<EntityConfigData>>> m_onSelectionDone;

        public BlueprintCreationController(
          ProtosDb protosDb,
          UnlockedProtosDbForUi unlockedProtosDb,
          EntitiesCloneConfigHelper configCloneHelper,
          ShortcutsManager shortcutsManager,
          ToolbarController toolbarController,
          IUnityInputMgr inputManager,
          CursorPickingManager cursorPickingManager,
          CursorManager cursorManager,
          AreaSelectionToolFactory areaSelectionToolFactory,
          NewInstanceOf<EntityHighlighter> highlighter,
          NewInstanceOf<TransportTrajectoryHighlighter> transportTrajectoryHighlighter,
          EntitiesManager entitiesManager) : base(protosDb, unlockedProtosDb, shortcutsManager, inputManager, cursorPickingManager, cursorManager, areaSelectionToolFactory, entitiesManager, highlighter, transportTrajectoryHighlighter, new Proto.ID?()) {
            m_configCloneHelper = configCloneHelper;
            m_inputManager = inputManager;
            m_toolbox = new BlueprintToolbox(toolbarController);
            SetPartialTransportsSelection(true);
            SetInstaActionDisabled(true);
            SetEdgeSizeLimit(new RelTile1i(350));
        }

        public override void RegisterUi(UiBuilder builder) {
            m_selectSound = builder.AudioDb.GetSharedAudio(builder.Audio.EntitySelect);
            m_toolbox.RegisterUi(builder);
            InitializeUi(builder, builder.Style.Cursors.SelectArea, builder.Audio.EntitySelect, HOVER_HIGHLIGHT, HOVER_HIGHLIGHT);
            base.RegisterUi(builder);
        }

        protected override bool Matches(IStaticEntity entity, bool isAreaSelection, bool isLeftCLick) => isAreaSelection && BlueprintsLibrary.CanBeInBlueprint(entity.Prototype);

        public override void Deactivate() {
            base.Deactivate();
            m_toolbox.Hide();
        }

        public override void Activate() {
            base.Activate();
            m_toolbox.Show();
        }

        public void ActivateForSelection(Action<ImmutableArray<EntityConfigData>> onSelectionDone) {
            m_onSelectionDone = onSelectionDone.SomeOption();
            m_inputManager.ActivateNewController(this);
        }

        public override bool InputUpdate(IInputScheduler inputScheduler) {
            if (!IsActive) {
                Logging.Log.Error("Input update for non-active controller!");
                return false;
            }
            m_toolbox.PrimaryActionBtn.SetIsOn(ShortcutsManager.IsPrimaryActionOn);
            return base.InputUpdate(inputScheduler);
        }

        protected override bool OnFirstActivated(IStaticEntity hoveredEntity, Lyst<IStaticEntity> selectedEntities, Lyst<SubTransport> selectedPartialTransports) => false;

        protected override void OnEntitiesSelected(IIndexable<IStaticEntity> selectedEntities, IIndexable<SubTransport> selectedPartialTransports, bool isAreaSelection, bool isLeftClick) {
            m_toolbox.Hide();
            if (selectedEntities.Count + selectedPartialTransports.Count - selectedEntities.AsEnumerable().Count(x => x is TransportPillar) <= 0) return;
            m_selectSound.Play();
            Lyst<EntityConfigData> lyst = new Lyst<EntityConfigData>();
            foreach (IStaticEntity selectedEntity in selectedEntities) lyst.Add(m_configCloneHelper.CreateConfigFrom(selectedEntity));
            foreach (SubTransport partialTransport in selectedPartialTransports) {
                EntityConfigData configFrom = m_configCloneHelper.CreateConfigFrom(partialTransport.OriginalTransport);
                configFrom.Trajectory = partialTransport.SubTrajectory;
                lyst.Add(configFrom);
            }
            m_inputManager.DeactivateController(this);
            Action<ImmutableArray<EntityConfigData>> valueOrNull = m_onSelectionDone.ValueOrNull;
            if (valueOrNull == null) return;
            valueOrNull(lyst.ToImmutableArray());
        }

        private class BlueprintToolbox : Toolbox, IUnityUi {
            public ToggleBtn PrimaryActionBtn;

            public BlueprintToolbox(ToolbarController toolbar) : base(toolbar) {
            }

            protected override void BuildCustomItems(UiBuilder builder) {
                PrimaryActionBtn = AddToggleButton("SelectArea", Assets.Unity.UserInterface.Toolbox.SelectArea_svg, _ => { }, m => m.PrimaryAction, Tr.Blueprint_NewFromSelectionTooltip);
                Toolbar.AddToolbox(this, this.GetWidth());
            }
        }
    }
}