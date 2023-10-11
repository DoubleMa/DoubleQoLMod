using Mafi;
using Mafi.Collections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core.Entities;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;
using Mafi.Unity;
using Mafi.Unity.Audio;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Factory;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Styles;
using Mafi.Unity.UserInterface;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoubleQoL.QoL.Tools {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Tools.BaseEntityCursorInputController{T}"/>.
    /// </summary>
    internal abstract class ABaseEntityCursorInputController<T> : IToolbarItemInputController, IUnityInputController, IUnityUi where T : class, IRenderedEntity, IAreaSelectableEntity {
        protected virtual RelTile1i MAX_AREA_EDGE_SIZE => new RelTile1i(300);
        protected readonly ShortcutsManager ShortcutsManager;
        private readonly UnlockedProtosDbForUi m_unlockedProtosDb;
        private readonly IUnityInputMgr m_inputManager;
        private readonly CursorPickingManager m_picker;
        private readonly CursorManager m_cursorManager;
        private readonly IEntitiesManager m_entitiesManager;
        protected readonly EntityHighlighter _highlighter;
        protected readonly TerrainCursor m_terrainCursor;
        private readonly Option<TransportTrajectoryHighlighter> m_transportTrajectoryHighlighter;
        private readonly Lyst<AudioSource> m_sounds;
        private UiBuilder m_builder;
        protected Option<Cursoor> m_cursor;
        private AudioSource m_invalidSound;
        private Option<T> m_toToggle;
        private Option<T> m_hoveredEntity;
        private readonly Lyst<InputCommand<bool>> m_pendingCmds;
        private readonly AreaSelectionTool m_areaSelectionTool;
        protected readonly Lyst<T> m_selectedEntities;
        private readonly Lyst<SubTransport> m_selectedPartialTransports;
        private readonly Lyst<TransportTrajectory> m_partialTrajsTmp;
        private AudioInfo m_successSound;
        private ColorRgba m_colorHighlight;
        private ColorRgba m_colorConfirm;
        private ColorRgba? m_rightClickAreaColor;
        private bool m_isFirstUpdate;
        private bool m_enablePartialTransportsSelection;
        private bool m_isInstaActionDisabled;
        private bool m_clearSelectionOnDeactivateOnly;
        private readonly Option<Proto> m_lockedByProto;

        public event Action<IToolbarItemInputController> VisibilityChanged;

        public virtual ControllerConfig Config => ControllerConfig.Tool;
        public bool IsVisible { get; private set; }
        public virtual bool DeactivateShortcutsIfNotVisible => false;
        public bool IsActive { get; private set; }
        protected bool WasPrimaryActionDown { get; private set; }

        protected bool IsClick = false;

        protected ABaseEntityCursorInputController(
          ProtosDb protosDb,
          UnlockedProtosDbForUi unlockedProtosDb,
          ShortcutsManager shortcutsManager,
          IUnityInputMgr inputManager,
          CursorPickingManager cursorPickingManager,
          CursorManager cursorManager,
          AreaSelectionToolFactory areaSelectionToolFactory,
          IEntitiesManager entitiesManager,
          NewInstanceOf<EntityHighlighter> highlighter,
            TerrainCursor terrainCursor,
          Option<NewInstanceOf<TransportTrajectoryHighlighter>> transportTrajectoryHighlighter,
          Proto.ID? lockByProto) : base() {
            m_sounds = new Lyst<AudioSource>();
            m_pendingCmds = new Lyst<InputCommand<bool>>();
            m_selectedEntities = new Lyst<T>();
            m_selectedPartialTransports = new Lyst<SubTransport>();
            m_partialTrajsTmp = new Lyst<TransportTrajectory>();
            m_unlockedProtosDb = unlockedProtosDb;
            ShortcutsManager = shortcutsManager;
            m_inputManager = inputManager;
            m_picker = cursorPickingManager;
            m_cursorManager = cursorManager;
            m_entitiesManager = entitiesManager;
            _highlighter = highlighter.Instance;
            m_terrainCursor = terrainCursor;
            m_transportTrajectoryHighlighter = (Option<TransportTrajectoryHighlighter>)transportTrajectoryHighlighter.ValueOrNull?.Instance;
            m_lockedByProto = lockByProto.HasValue ? (Option<Proto>)protosDb.GetOrThrow<Proto>(lockByProto.Value) : Option<Proto>.None;
            m_areaSelectionTool = areaSelectionToolFactory.CreateInstance(new Action<RectangleTerrainArea2i, bool>(UpdateSelectionSync), new Action<RectangleTerrainArea2i, bool>(SelectionDone), new Action(ClearSelection), new Action(DeactivateSelf));
            m_areaSelectionTool.SetEdgeSizeLimit(MAX_AREA_EDGE_SIZE);
        }

        protected void InitializeUi(UiBuilder builder, CursorStyle? cursorStyle, AudioInfo successSound, ColorRgba colorHighlight, ColorRgba colorConfirm) {
            m_builder = builder;
            m_successSound = successSound;
            m_colorHighlight = colorHighlight;
            m_colorConfirm = colorConfirm;
            if (cursorStyle.HasValue) m_cursor = (Option<Cursoor>)m_cursorManager.RegisterCursor(cursorStyle.Value);
            m_invalidSound = builder.AudioDb.GetSharedAudio(builder.Audio.InvalidOp);
            m_areaSelectionTool.SetLeftClickColor(colorHighlight);
        }

        protected void SetEdgeSizeLimit(RelTile1i limit) => m_areaSelectionTool.SetEdgeSizeLimit(limit);

        protected void ClearSelectionOnDeactivateOnly() => m_clearSelectionOnDeactivateOnly = true;

        protected void SetPartialTransportsSelection(bool isEnabled) {
            if (m_transportTrajectoryHighlighter.IsNone) Logging.Log.Error("Transports trajectory highlighter must be set to allow partial transports.");
            else {
                m_enablePartialTransportsSelection = isEnabled;
                m_areaSelectionTool.ForceSelectionChanged();
            }
        }

        protected void SetUpRightClickAreaSelection(ColorRgba color) {
            m_rightClickAreaColor = new ColorRgba?(color);
            m_areaSelectionTool.SetRightClickColor(color);
        }

        protected abstract bool Matches(T entity, bool isAreaSelection, bool isLeftClick);

        public virtual void RegisterUi(UiBuilder builder) {
            IsVisible = m_lockedByProto.IsNone || m_unlockedProtosDb.IsUnlocked((IProto)m_lockedByProto.Value);
            if (!IsVisible) m_unlockedProtosDb.OnUnlockedSetChangedForUi += new Action(UpdateIsVisible);
            else {
                Action<IToolbarItemInputController> visibilityChanged = VisibilityChanged;
                if (visibilityChanged == null) return;
                visibilityChanged(this);
            }
        }

        private void UpdateIsVisible() {
            bool flag = m_unlockedProtosDb.IsUnlocked(m_lockedByProto.Value);
            if (IsVisible == flag) return;
            IsVisible = flag;
            Action<IToolbarItemInputController> visibilityChanged = VisibilityChanged;
            if (visibilityChanged == null) return;
            visibilityChanged(this);
        }

        protected void SetInstaActionDisabled(bool isDisabled) => m_isInstaActionDisabled = isDisabled;

        public virtual void Activate() {
            if (IsActive) return;
            IsActive = true;
            m_cursor.ValueOrNull?.Show();
            m_areaSelectionTool.TerrainCursor.Activate();
            m_isFirstUpdate = !m_isInstaActionDisabled;
        }

        public virtual void Deactivate() {
            if (!IsActive) return;
            m_cursor.ValueOrNull?.Hide();
            m_picker.ClearPicked();
            m_pendingCmds.Clear();
            m_toToggle = (Option<T>)Option.None;
            m_areaSelectionTool.TerrainCursor.Deactivate();
            m_areaSelectionTool.Deactivate();
            ClearSelection();
            m_isFirstUpdate = false;
            IsActive = false;
        }

        protected void HideCursor() => m_cursor.ValueOrNull?.Hide();

        protected virtual bool OnSecondaryActionUp() {
            DeactivateSelf();
            return true;
        }

        public virtual bool InputUpdate(IInputScheduler inputScheduler) {
            if (!IsActive) {
                Logging.Log.Error("Input update for non-active controller!");
                return false;
            }
            if (ShortcutsManager.IsSecondaryActionUp && !m_rightClickAreaColor.HasValue && OnSecondaryActionUp()) return true;

            bool isFirstUpdate = m_isFirstUpdate;
            m_isFirstUpdate = false;
            if (m_pendingCmds.IsNotEmpty) {
                if (!HandleCurrentCommand()) return false;
                m_pendingCmds.Clear();
            }
            if (m_toToggle.IsNone) {
                if (m_areaSelectionTool.IsActive) return true;
                T obj = m_selectedEntities.Count == 1 ? m_selectedEntities.First : default;
                m_selectedEntities.Clear();
                m_selectedPartialTransports.Clear();
                m_hoveredEntity = m_picker.PickEntityAndSelect(new CursorPickingManager.EntityPredicateReturningColor<T>(AnyEntityMatcher));
                if (m_hoveredEntity.HasValue) {
                    if (isFirstUpdate && !EventSystem.current.IsPointerOverGameObject()) {
                        m_selectedEntities.Clear();
                        m_selectedPartialTransports.Clear();
                        if (OnFirstActivated(m_hoveredEntity.Value, m_selectedEntities, m_selectedPartialTransports) && (m_selectedEntities.IsNotEmpty || m_selectedPartialTransports.IsNotEmpty)) {
                            OnEntitiesSelected(m_selectedEntities, m_selectedPartialTransports, false, true);
                            return true;
                        }
                    }
                    m_selectedEntities.Add(m_hoveredEntity.Value);
                    if (obj != m_hoveredEntity) OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, true);
                    if (ShortcutsManager.IsPrimaryActionDown && !EventSystem.current.IsPointerOverGameObject()) {
                        m_toToggle = m_hoveredEntity;
                        return true;
                    }
                }
                else {
                    if (ShortcutsManager.IsPrimaryActionDown) {
                        m_areaSelectionTool.Activate(true);
                        return true;
                    }
                    if (m_rightClickAreaColor.HasValue && ShortcutsManager.IsSecondaryActionDown) {
                        m_areaSelectionTool.Activate(false);
                        return true;
                    }
                    OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, true);
                    return false;
                }
            }
            else {
                Option<T> option = m_picker.PickEntityAndSelect(new CursorPickingManager.EntityPredicateReturningColor<T>(EntityMatcher));
                if (ShortcutsManager.IsPrimaryActionUp) {
                    m_toToggle = (Option<T>)Option.None;
                    m_selectedEntities.Clear();
                    m_selectedPartialTransports.Clear();
                    if (option.IsNone || EventSystem.current.IsPointerOverGameObject()) {
                        OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, true);
                        return true;
                    }
                    m_picker.ClearPicked();
                    m_selectedEntities.Add(option.Value);
                    if (m_selectedEntities.IsNotEmpty || m_selectedPartialTransports.IsNotEmpty) OnEntitiesSelected(m_selectedEntities, m_selectedPartialTransports, false, true);
                    return true;
                }
            }
            WasPrimaryActionDown = ShortcutsManager.IsPrimaryActionDown;
            return false;
        }

        protected abstract bool OnFirstActivated(T hoveredEntity, Lyst<T> selectedEntities, Lyst<SubTransport> selectedPartialTransports);

        protected abstract void OnEntitiesSelected(IIndexable<T> selectedEntities, IIndexable<SubTransport> selectedPartialTransports, bool isAreaSelection, bool isLeftMouse);

        protected virtual void OnHoverChanged(IIndexable<T> hoveredEntities, IIndexable<SubTransport> hoveredPartialTransports, bool isLeftClick) {
        }

        protected virtual void OnClick(Tile3f position) {
        }

        protected void RegisterPendingCommand(InputCommand cmd) => m_pendingCmds.Add(cmd);

        private void DeactivateSelf() {
            if (m_clearSelectionOnDeactivateOnly) ClearSelection();
            m_inputManager.DeactivateController(this);
        }

        private bool HandleCurrentCommand() {
            bool flag = false;
            foreach (InputCommand<bool> pendingCmd in m_pendingCmds) {
                if (!pendingCmd.IsProcessedAndSynced) return false;
                flag |= pendingCmd.Result;
            }
            if (flag) PlaySuccessSound();
            else m_invalidSound.Play();
            return true;
        }

        private void PlaySuccessSound() {
            AudioSource audioSource = null;
            foreach (AudioSource sound in m_sounds) {
                if (!sound.isPlaying)
                    audioSource = sound;
            }
            if (audioSource == null) {
                audioSource = m_builder.AudioDb.GetClonedAudio(m_successSound);
                m_sounds.Add(audioSource);
            }
            audioSource.Play();
        }

        private bool AnyEntityMatcher(T entity, out ColorRgba color) {
            if (!Matches(entity, false, true)) {
                color = ColorRgba.Empty;
                return false;
            }
            color = m_colorHighlight;
            return true;
        }

        private bool EntityMatcher(T entity, out ColorRgba color) {
            if (entity != m_toToggle || !Matches(entity, false, true)) {
                color = ColorRgba.Empty;
                return false;
            }
            color = m_colorConfirm;
            return true;
        }

        private void SelectionDone(RectangleTerrainArea2i area, bool isLeftClick) {
            if (m_selectedEntities.IsNotEmpty || m_selectedPartialTransports.IsNotEmpty) OnEntitiesSelected(m_selectedEntities, m_selectedPartialTransports, true, isLeftClick);
            if (!m_clearSelectionOnDeactivateOnly) ClearSelection();
            m_areaSelectionTool.Deactivate();
            OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, isLeftClick);
        }

        protected virtual void ClearSelection() {
            m_selectedEntities.Clear();
            _highlighter.ClearAllHighlights();
            m_transportTrajectoryHighlighter.ValueOrNull?.ClearAllHighlights();
            m_selectedPartialTransports.Clear();
        }

        private void UpdateSelectionSync(RectangleTerrainArea2i area, bool leftClick) {
            IsClick = area.AreaTiles <= 1;
            if (!IsClick) ClearSelection();
            foreach (T entity in m_entitiesManager.GetAllEntitiesOfType<T>()) {
                if (entity.IsSelected(area) && Matches(entity, true, leftClick)) {
                    if (m_enablePartialTransportsSelection && entity is Transport originalTransport) {
                        m_partialTrajsTmp.Clear();
                        originalTransport.Trajectory.GetSubTrajectoriesInArea(area, m_partialTrajsTmp, out bool entireTrajectoryIsInArea);
                        if (entireTrajectoryIsInArea) {
                            addEntity(entity);
                            Assert.That(m_partialTrajsTmp).IsEmpty();
                        }
                        else
                            foreach (TransportTrajectory transportTrajectory in m_partialTrajsTmp) {
                                m_selectedPartialTransports.Add(new SubTransport(originalTransport, transportTrajectory));
                                m_transportTrajectoryHighlighter.ValueOrNull?.HighlightTrajectory(transportTrajectory, m_colorHighlight);
                            }
                    }
                    else addEntity(entity);
                }
            }
            m_partialTrajsTmp.Clear();
            OnHoverChanged(m_selectedEntities, m_selectedPartialTransports, leftClick);

            void addEntity(T entity) {
                m_selectedEntities.Add(entity);
                _highlighter.Highlight(entity, m_colorHighlight);
            }
        }
    }
}