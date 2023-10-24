using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Global;
using DoubleQoL.Global.Utils;
using DoubleQoL.QoL.Shortcuts;
using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Core.Vehicles;
using Mafi.Core.Vehicles.Commands;
using Mafi.Core.Vehicles.Trucks;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Toolbar;
using Mafi.Unity.UserInterface;
using System;

namespace DoubleQoL.QoL.Tools {

    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    internal class VehicleTool : ASelectableEntityTool {
        protected override RelTile1i MAX_AREA_EDGE_SIZE => new RelTile1i(Static.MaxMapSize);
        private static readonly ColorRgba COLOR_HIGHLIGHT_MOVING = ColorRgba.Green;
        private static readonly ColorRgba COLOR_HIGHLIGHT = new ColorRgba(16770068, 192);
        private static readonly ColorRgba COLOR_HIGHLIGHT_CONFIRM = ColorRgba.Blue;

        private readonly IVehiclesManager _vehiclesManager;
        private readonly Lyst<Vehicle> Vehicles;
        private readonly Lyst<Vehicle> MovedVehicles;
        protected override bool IgnoreModifiers => false;

        private readonly Lyst<VehicleTypeInfo> vehicleTypeInfos = new Lyst<VehicleTypeInfo>()
        {
            new VehicleTypeInfo("Truck", DoubleQoLShortcutsMap.Instance.VehicleTrucksToolKb, Ids.Vehicles.TruckT1.Id, Ids.Vehicles.TruckT2.Id),
            new VehicleTypeInfo("Excavator", DoubleQoLShortcutsMap.Instance.VehicleExcavatorToolKb, Ids.Vehicles.ExcavatorT1, Ids.Vehicles.ExcavatorT2, Ids.Vehicles.ExcavatorT3),
            new VehicleTypeInfo("Tank Truck", DoubleQoLShortcutsMap.Instance.VehicleTankToolKb, Ids.Vehicles.TruckT3Loose.Id),
            new VehicleTypeInfo("Dump Truck", DoubleQoLShortcutsMap.Instance.VehicleDumpToolKb, Ids.Vehicles.TruckT3Fluid.Id)
        };

        public VehicleTool(
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
            IVehiclesManager vehiclesManager) :
            base(protosDb, unlockedProtosDb, shortcutsManager, inputManager, cursorPickingManager, cursorManager, areaSelectionToolFactory, entitiesManager, highlighter, toolbarController, terrainCursor) {
            _vehiclesManager = vehiclesManager;
            Vehicles = new Lyst<Vehicle>();
            MovedVehicles = new Lyst<Vehicle>();
            ClearSelectionOnDeactivateOnly();
            Lyst<ToolToggleBtn> toolToggleBtns = new Lyst<ToolToggleBtn>() { new ToolToggleBtn("Select", IconPaths.Tool_Select, _ => { }, shortcutsManager.PrimaryAction, "Select and move vehicles", true) };
            vehicleTypeInfos.ForEach(vti => toolToggleBtns.Add(new ToolToggleBtn(vti.Name, vti.IconPath, _ => { }, vti.Trigger, $"Select/Move {vti.Name}s")));
            toolToggleBtns.Add(new ToolToggleBtn("Clear truck cargo", Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png, _ => { }, DoubleQoLShortcutsMap.Instance.ClearTruckCargoToolKb, "Try to clear the cargo of selected trucks"));
            toolToggleBtns.Add(new ToolToggleBtn("Recover vehicle", Mafi.Unity.Assets.Unity.UserInterface.General.RecoverVehicle_svg, _ => { }, DoubleQoLShortcutsMap.Instance.RecoverVehicleToolKb, "Recover the selected vehicles"));
            SetToolbox(toolToggleBtns);
        }

        private void HighLightVehicleConfirm(Vehicle v) => HighLightVehicle(v, COLOR_HIGHLIGHT_CONFIRM);

        private void HighLightVehicleMoved(Vehicle v) => HighLightVehicle(v, COLOR_HIGHLIGHT_MOVING);

        private void HighLightVehicle(Vehicle v, ColorRgba colorRgba) => _highlighter.Highlight(v, colorRgba);

        private void HighLightAllConfirm() => Vehicles.ForEach(v => HighLightVehicleConfirm(v));

        public void AddVehicle(Vehicle v) {
            Vehicles.Add(v);
            HighLightVehicleConfirm(v);
        }

        public void AddVehicles(IIndexable<IAreaSelectableEntity> selectedEntities, bool clear = true) {
            if (clear) Clear();
            foreach (var v in selectedEntities) AddVehicle((Vehicle)v);
            HighLightAllConfirm();
        }

        public void HandleInput(IInputScheduler inputScheduler) {
            if (ShortcutsManager.IsPrimaryActionUp && IsClick && m_terrainCursor.TryComputeCurrentPosition(out var position)) HandleMoveAllVehicles(inputScheduler, position);
            if (DoubleQoLShortcutsMap.Instance.ClearTruckCargoToolKb.IsPrimaryOn(IgnoreModifiers)) HandleCLearAllCargos(inputScheduler);
            if (DoubleQoLShortcutsMap.Instance.RecoverVehicleToolKb.IsPrimaryOn(IgnoreModifiers)) HandleRecoverAllVehicles(inputScheduler);
        }

        private void HandleMoveAllVehicles(IInputScheduler inputScheduler, Tile3f position) {
            ClearMoved();
            GetVehicles().ForEach(v => MoveVehicle(inputScheduler, v, position));
        }

        private void HandleCLearAllCargos(IInputScheduler inputScheduler) => GetVehicles().ForEach(v => TryClearCargo(inputScheduler, v));

        private void HandleRecoverAllVehicles(IInputScheduler inputScheduler) => GetVehicles().ForEach(v => TryRecoverVehicle(inputScheduler, v));

        private void MoveVehicle(IInputScheduler inputScheduler, Vehicle v, Tile3f position) {
            inputScheduler.ScheduleInputCmd(new NavigateVehicleToPositionCmd(v, position.Xy.Tile2i));
            HighLightVehicleMoved(v);
            MovedVehicles.Add(v);
        }

        public void TryClearCargo(IInputScheduler inputScheduler, Vehicle v) {
            if (v is Truck) inputScheduler.ScheduleInputCmd(new DiscardVehicleCargoCmd(v));
        }

        public void TryRecoverVehicle(IInputScheduler inputScheduler, Vehicle v) => inputScheduler.ScheduleInputCmd(new RecoverVehicleCmd(v));

        public void Clear() {
            _highlighter.ClearAllHighlights();
            Vehicles.Clear();
            MovedVehicles.Clear();
        }

        private void ClearMoved() {
            MovedVehicles.ForEach(v => {
                if (Vehicles.Contains(v)) _highlighter.Highlight(v, COLOR_HIGHLIGHT_CONFIRM);
                else _highlighter.RemoveHighlight(v);
            });
            MovedVehicles.Clear();
        }

        protected override void ClearSelection() {
            base.ClearSelection();
            HighLightAllConfirm();
        }

        public override void Deactivate() {
            base.Deactivate();
            Clear();
        }

        private Lyst<VehicleTypeInfo> GetActiveVehicleTypeInfo() => vehicleTypeInfos.FindAll(e => e.Trigger.IsPrimaryOn(IgnoreModifiers));

        protected override bool Matches(IAreaSelectableEntity entity, bool isAreaSelection, bool isLeftClick) {
            if (!(entity is Vehicle)) return false;
            Lyst<VehicleTypeInfo> activeVehicleTypeInfo = GetActiveVehicleTypeInfo();
            if (activeVehicleTypeInfo.IsEmpty) return IgnoreModifiers || !InputHelper.IsModifierDown();
            Vehicle v = (Vehicle)entity;
            foreach (var vti in activeVehicleTypeInfo) if (vti.CheckID(v)) return true;
            return false;
        }

        private Lyst<Vehicle> GetVehicles() {
            Lyst<VehicleTypeInfo> activeVehicleTypeInfo = GetActiveVehicleTypeInfo();
            if (activeVehicleTypeInfo.IsEmpty) return Vehicles;
            Lyst<Vehicle> result = new Lyst<Vehicle>();
            foreach (var vti in activeVehicleTypeInfo) {
                if (Vehicles.Count > 0) result.AddRange(Vehicles.FindAll(v => vti.CheckID(v)));
                else result.AddRange(_vehiclesManager.AllVehicles.ToLyst().FindAll(v => vti.CheckID(v)));
            }
            return result;
        }

        protected override void OnEntitiesSelected(IIndexable<IAreaSelectableEntity> selectedEntities, IIndexable<SubTransport> selectedPartialTransports, bool isAreaSelection, bool isLeftClick) {
            if (selectedEntities.IsNotEmpty()) AddVehicles(selectedEntities);
        }

        protected override bool OnSecondaryActionUp() {
            if (Vehicles.IsEmpty) return base.OnSecondaryActionUp();
            Clear();
            return false;
        }

        public override bool InputUpdate(IInputScheduler inputScheduler) {
            HandleInput(inputScheduler);
            return base.InputUpdate(inputScheduler);
        }

        public override void RegisterUi(UiBuilder builder) {
            if (ConfigManager.Instance.QoLs_vehicletool.Value) {
                _toolbarController
                    .AddLeftMenuButton(Loc.Str("VehicleTool", "Vehicle Tool", "title of a tool that is used to select and move vehicles").TranslatedString, this, Mafi.Unity.Assets.Unity.UserInterface.Toolbar.Vehicles_svg, 80f, m => DoubleQoLShortcutsMap.Instance.VehicleToolKb)
                    .AddTooltip(new LocStrFormatted("Select One or multiple vehicles to move them"));
                InitializeUi(builder, new Mafi.Unity.UiFramework.Styles.CursorStyle(nameof(Mafi.Unity.Assets.Unity.UserInterface.Toolbar.Vehicles_svg), Mafi.Unity.Assets.Unity.UserInterface.Toolbar.Vehicles_svg, new UnityEngine.Vector2(14f, 14f)), builder.Audio.ButtonClick, COLOR_HIGHLIGHT, COLOR_HIGHLIGHT_CONFIRM);
                base.RegisterUi(builder);
            }
        }

        private class VehicleTypeInfo {
            public string Name { get; }
            public Proto.ID[] Ids { get; }
            public KeyBindings Trigger { get; }

            public VehicleTypeInfo(string name, KeyBindings trigger, params Proto.ID[] ids) {
                if (ids == null || ids.Length == 0) throw new ArgumentNullException(nameof(ids));
                Name = name;
                Ids = ids;
                Trigger = trigger;
            }

            public string IconPath => Ids[Ids.Length - 1].GetVehicleIconPath();

            public bool CheckID(Vehicle vehicle) {
                foreach (var id in Ids) if (vehicle.IsProtoEquals(id)) return true;
                return false;
            }
        }
    }
}