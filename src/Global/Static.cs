using Mafi.Unity.InputControl;
using UnityEngine;

namespace DoubleQoL.Global {

    public static class Static {
        public static Vector2 WindowSize = new Vector2(680f, 400f);
        public static int MaxMapHeight = 2748;
        public static int MaxMapWidth = 2492;
        public static int MaxMapSize = Mathf.Max(MaxMapHeight, MaxMapWidth);

        public static ControllerConfig StandAloneControllerConfig = new ControllerConfig {
            IgnoreEscapeKey = true,
            DeactivateOnOtherControllerActive = false,
            AllowInspectorCursor = true,
            BlockShortcuts = false,
            BlockCameraControlIfInputWasProcessed = false,
            PreventSpeedControl = false,
            Group = ControllerGroup.None,
        };

        public static ControllerConfig BlockShortcutsToolConfig = new ControllerConfig {
            DeactivateOnNonUiClick = false,
            AllowInspectorCursor = false,
            DeactivateOnOtherControllerActive = true,
            BlockShortcuts = true,
            Group = ControllerGroup.Tool
        };
    }

    public static class IconPaths {
        public const string Toolbar_YinYang = "Assets/DoubleQoL/Toolbar/yinyang.png";
        public const string Status_BarFog = "Assets/DoubleQoL/StatusBar/Fog.png";
        public const string Status_BarDev = "Assets/DoubleQoL/StatusBar/Dev.png";
        public const string Tool_Location = "Assets/DoubleQoL/Tool/Location.png";
        public const string Tool_Select = "Assets/DoubleQoL/Tool/Select.png";
        public const string Tool_MovingTruck = "Assets/DoubleQoL/Tool/MovingTruck.png";
    }
}