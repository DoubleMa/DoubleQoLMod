using DoubleQoL.Config;
using DoubleQoL.QoL.UI.Blueprint;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;
using Mafi.Core.GameLoop;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.LayoutEntityPlacing;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;
using System;
using System.Linq;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Blueprints.BlueprintsController"/>.
    /// </summary>
    [GlobalDependency(RegistrationMode.AsEverything, false, false)]
    internal class BlueprintsController : IToolbarItemInputController, IUnityInputController, IUnityUi {
        private readonly IUnityInputMgr m_inputManager;
        private readonly UnlockedProtosDb m_unlockedProtosDb;
        private readonly UnlockedProtosDbForUi m_unlockedProtosDbForUi;
        private readonly BlueprintsWindowView m_view;
        private readonly IGameLoopEvents m_gameLoop;
        private readonly ToolbarController m_toolbarController;
        private readonly StaticEntityMassPlacer m_entityPlacer;
        private readonly TechnologyProto m_blueprintsTech;
        private readonly Lyst<EntityConfigData> m_copiedConfigs;

        public event Action<IToolbarItemInputController> VisibilityChanged;

        public bool IsVisible { get; private set; }

        public bool DeactivateShortcutsIfNotVisible => true;

        public ControllerConfig Config => !m_entityPlacer.IsActive ? ControllerConfig.Window : ControllerConfig.Tool;

        public BlueprintsController(NewInstanceOf<StaticEntityMassPlacer> entityPlacer, UnlockedProtosDb unlockedProtosDb, UnlockedProtosDbForUi unlockedProtosDbForUi, ProtosDb protosDb,
          IUnityInputMgr inputManager, BlueprintsWindowView view, IGameLoopEvents gameLoop, ToolbarController toolbarController) {
            m_copiedConfigs = new Lyst<EntityConfigData>();
            BlueprintsController controller = this;
            m_entityPlacer = entityPlacer.Instance;
            m_unlockedProtosDb = unlockedProtosDb;
            m_unlockedProtosDbForUi = unlockedProtosDbForUi;
            m_inputManager = inputManager;
            m_view = view;
            m_gameLoop = gameLoop;
            m_toolbarController = toolbarController;
            m_view.SetOnCloseButtonClickAction(() => inputManager.DeactivateController(controller));
            m_blueprintsTech = protosDb.GetOrThrow<TechnologyProto>(IdsCore.Technology.Blueprints);
        }

        public void RegisterUi(UiBuilder builder) {
            if (ConfigManager.Instance.Blueprint_Servers.Count() == 0) return;
            IsVisible = m_unlockedProtosDb.IsUnlocked(m_blueprintsTech);
            m_toolbarController.AddMainMenuButton(Tr.Blueprints.TranslatedString, this, Assets.Unity.UserInterface.Toolbar.Blueprints_svg, 430f, _ => _.ToggleBlueprints);
            m_view.BuildUi(builder);
            m_entityPlacer.RegisterUi(builder);
            if (IsVisible) return;
            m_unlockedProtosDb.OnUnlockedSetChanged.AddNonSaveable(this, onProtosUnlocked);
            m_inputManager.RemoveGlobalShortcut(this);
        }

        private void onProtosUnlocked() {
            IsVisible = m_unlockedProtosDb.IsUnlocked(m_blueprintsTech);
            if (!IsVisible) return;
            m_unlockedProtosDb.OnUnlockedSetChanged.RemoveNonSaveable(this, onProtosUnlocked);
            m_inputManager.RegisterGlobalShortcut(m => m.ToggleBlueprints, this);
            Action<IToolbarItemInputController> visibilityChanged = VisibilityChanged;
            if (visibilityChanged == null) return;
            visibilityChanged(this);
        }

        public void Activate() {
            m_gameLoop.SyncUpdate.AddNonSaveable(this, syncUpdate);
            m_gameLoop.RenderUpdate.AddNonSaveable(this, renderUpdate);
            m_view.Show();
        }

        public void Deactivate() {
            if (m_entityPlacer.IsActive) m_entityPlacer.Deactivate();
            m_view.Hide();
            m_gameLoop.SyncUpdate.RemoveNonSaveable(this, syncUpdate);
            m_gameLoop.RenderUpdate.RemoveNonSaveable(this, renderUpdate);
        }

        public bool InputUpdate(IInputScheduler inputScheduler) => m_entityPlacer.IsActive ? m_entityPlacer.InputUpdate(inputScheduler) : m_view.InputUpdate(inputScheduler);

        private void renderUpdate(GameTime gameTime) => m_view.RenderUpdate(gameTime);

        private void syncUpdate(GameTime gameTime) => m_view.SyncUpdate(gameTime);

        public void StartBlueprintPlacement(IBlueprint blueprint) {
            m_copiedConfigs.Clear();
            foreach (EntityConfigData entityConfigData in blueprint.Items) {
                if (!entityConfigData.Prototype.IsNone) {
                    Proto proto = entityConfigData.Prototype.Value;
                    if (m_unlockedProtosDbForUi.IsLocked(proto)) {
                        Option<Proto> unlockedDowngradeFor = m_unlockedProtosDbForUi.GetNearestUnlockedDowngradeFor(proto);
                        if (unlockedDowngradeFor.HasValue) m_copiedConfigs.Add(entityConfigData.CreateCopyWithNewProto(unlockedDowngradeFor.Value));
                    }
                    else m_copiedConfigs.Add(entityConfigData.CreateCopy());
                }
            }
            if (m_copiedConfigs.IsEmpty) return;
            m_view.Hide();
            m_entityPlacer.Activate(this, blueprintPlaced, blueprintPlacementCancelled);
            m_entityPlacer.SetEntitiesToClone(m_copiedConfigs, StaticEntityMassPlacer.ApplyConfigMode.ApplyIfNotDisabled, true);
        }

        private void blueprintPlaced() => m_inputManager.DeactivateController(this);

        private void blueprintPlacementCancelled() => m_view.Show();
    }
}