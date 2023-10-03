using DoubleQoL.Config;
using Mafi.Unity.InputControl;
using UnityEngine;

namespace DoubleQoL.Game.Shortcuts {

    public class DoubleQoLShortcutsMap : ShortcutsMap {
        public static readonly DoubleQoLShortcutsMap Instance = new DoubleQoLShortcutsMap();
        public const KbCategory DoubleQoLKB = new KbCategory();

        [Kb(DoubleQoLKB, "IncSpeed", "Increase Game Speed", null, true, false)]
        public KeyBindings IncSpeedKB { get; set; }

        [Kb(DoubleQoLKB, "DecSpeed", "Decrease Game Speed", null, true, false)]
        public KeyBindings DecSpeedKb { get; set; }

        [Kb(DoubleQoLKB, "FreeCamera", "Disable/Enable Free Camera", null, true, false)]
        public KeyBindings FreeCameraKb { get; set; }

        [Kb(DoubleQoLKB, "DisableFog", "Disable/Enable Fog", null, true, false)]
        public KeyBindings DisableFogKb { get; set; }

        [Kb(DoubleQoLKB, "DisableCollapse", "Disable/Enable Collapse", null, true, false)]
        public KeyBindings DisableCollapseKb { get; set; }

        [Kb(DoubleQoLKB, "VehicleTool", "Activate Vehicle Tool", null, true, false)]
        public KeyBindings VehicleToolKb { get; set; }

        [Kb(DoubleQoLKB, "VehicleExcavatorsTool", "Move Excavators", null, true, false)]
        public KeyBindings VehicleExcavatorToolKb { get; set; }

        [Kb(DoubleQoLKB, "VehicleDumpTool", "Move Dump trucks", null, true, false)]
        public KeyBindings VehicleDumpToolKb { get; set; }

        [Kb(DoubleQoLKB, "VehicleTankTool", "Move Tank trucks", null, true, false)]
        public KeyBindings VehicleTankToolKb { get; set; }

        [Kb(DoubleQoLKB, "VehicleTrucksTool", "Move trucks", null, true, false)]
        public KeyBindings VehicleTrucksToolKb { get; set; }

        [Kb(DoubleQoLKB, "DevMode", "Disable/Enable Dev Mode", null, true, false)]
        public KeyBindings DevKeyKb { get; set; }

        public DoubleQoLShortcutsMap() : base() {
            IncSpeedKB = KeyBindings.FromKey(DoubleQoLKB, ConfigManager.Instance.KeyCodes_incSpeed.Value);
            DecSpeedKb = KeyBindings.FromKey(DoubleQoLKB, ConfigManager.Instance.KeyCodes_decSpeed.Value);
            FreeCameraKb = KeyBindings.FromKey(DoubleQoLKB, ConfigManager.Instance.KeyCodes_freeCamera.Value);
            DisableFogKb = KeyBindings.FromKey(DoubleQoLKB, ConfigManager.Instance.KeyCodes_fog.Value);
            DisableCollapseKb = KeyBindings.FromKey(DoubleQoLKB, ConfigManager.Instance.KeyCodes_collapse.Value);
            VehicleToolKb = KeyBindings.FromKey(DoubleQoLKB, ConfigManager.Instance.KeyCodes_vehicletool.Value);

            VehicleTrucksToolKb = KeyBindings.FromKey(DoubleQoLKB, KeyCode.Q);
            VehicleExcavatorToolKb = KeyBindings.FromKey(DoubleQoLKB, KeyCode.E);
            VehicleTankToolKb = KeyBindings.FromKey(DoubleQoLKB, KeyCode.R);
            VehicleDumpToolKb = KeyBindings.FromKey(DoubleQoLKB, KeyCode.F);

            DevKeyKb = KeyBindings.FromKey(DoubleQoLKB, ConfigManager.Instance.KeyCodes_Dev.Value);
        }
    }
}