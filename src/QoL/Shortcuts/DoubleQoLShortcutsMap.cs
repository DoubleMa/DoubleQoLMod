using DoubleQoL.Config;
using Mafi.Unity.InputControl;
using UnityEngine;

namespace DoubleQoL.QoL.Shortcuts {

    internal class DoubleQoLShortcutsMap {
        public static readonly DoubleQoLShortcutsMap Instance = new DoubleQoLShortcutsMap();

        [Kb(KbCategory.Speed, "IncSpeed", "Increase Game Speed", null, true, false)]
        public KeyBindings IncSpeedKB { get; set; }

        [Kb(KbCategory.Speed, "DecSpeed", "Decrease Game Speed", null, true, false)]
        public KeyBindings DecSpeedKb { get; set; }

        [Kb(KbCategory.Camera, "FreeCamera", "Disable/Enable Free Camera", null, true, false)]
        public KeyBindings FreeCameraKb { get; set; }

        [Kb(KbCategory.Camera, "DisableFog", "Disable/Enable Fog", null, true, false)]
        public KeyBindings DisableFogKb { get; set; }

        [Kb(KbCategory.Tools, "VehicleTool", "Activate Vehicle Tool", null, true, false)]
        public KeyBindings VehicleToolKb { get; set; }

        [Kb(KbCategory.Tools, "VehicleExcavatorsTool", "Move Excavators", null, true, false)]
        public KeyBindings VehicleExcavatorToolKb { get; set; }

        [Kb(KbCategory.Tools, "VehicleDumpTool", "Move Dump trucks", null, true, false)]
        public KeyBindings VehicleDumpToolKb { get; set; }

        [Kb(KbCategory.Tools, "VehicleTankTool", "Move Tank Trucks", null, true, false)]
        public KeyBindings VehicleTankToolKb { get; set; }

        [Kb(KbCategory.Tools, "VehicleTrucksTool", "Move Trucks", null, true, false)]
        public KeyBindings VehicleTrucksToolKb { get; set; }

        [Kb(KbCategory.Tools, "ClearTruckCargoTool", "Clear Truck Cargo", null, true, false)]
        public KeyBindings ClearTruckCargoToolKb { get; set; }

        [Kb(KbCategory.Tools, "RecoverVehicleToolKb", "Recover Vehicle", null, true, false)]
        public KeyBindings RecoverVehicleToolKb { get; set; }

        [Kb(KbCategory.General, "DevMode", "Disable/Enable Dev Mode", null, true, false)]
        public KeyBindings DevKeyKb { get; set; }

        private DoubleQoLShortcutsMap() : base() {
            IncSpeedKB = KeyBindings.FromKey(KbCategory.Speed, ConfigManager.Instance.KeyCodes_incSpeed.Value);
            DecSpeedKb = KeyBindings.FromKey(KbCategory.Speed, ConfigManager.Instance.KeyCodes_decSpeed.Value);
            FreeCameraKb = KeyBindings.FromKey(KbCategory.Camera, ConfigManager.Instance.KeyCodes_freeCamera.Value);
            DisableFogKb = KeyBindings.FromKey(KbCategory.Camera, ConfigManager.Instance.KeyCodes_fog.Value);
            VehicleToolKb = KeyBindings.FromKey(KbCategory.Tools, ConfigManager.Instance.KeyCodes_vehicletool.Value);

            VehicleTrucksToolKb = KeyBindings.FromKey(KbCategory.Tools, KeyCode.Q);
            VehicleExcavatorToolKb = KeyBindings.FromKey(KbCategory.Tools, KeyCode.E);
            VehicleTankToolKb = KeyBindings.FromKey(KbCategory.Tools, KeyCode.R);
            VehicleDumpToolKb = KeyBindings.FromKey(KbCategory.Tools, KeyCode.F);
            RecoverVehicleToolKb = KeyBindings.FromPrimaryKeys(KbCategory.Tools, KeyCode.LeftShift, KeyCode.E);
            ClearTruckCargoToolKb = KeyBindings.FromPrimaryKeys(KbCategory.Tools, KeyCode.LeftShift, KeyCode.R);

            DevKeyKb = KeyBindings.FromKey(KbCategory.General, ConfigManager.Instance.KeyCodes_Dev.Value);
        }
    }
}