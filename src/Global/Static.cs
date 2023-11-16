using Mafi.Unity.InputControl;
using UnityEngine;

namespace DoubleQoL.Global {

    public static class Static {
        public static readonly Vector2 WindowSize = new Vector2(680f, 400f);
        public static readonly int MaxMapHeight = 2748;
        public static readonly int MaxMapWidth = 2492;
        public static readonly int MaxMapSize = Mathf.Max(MaxMapHeight, MaxMapWidth);

        public static readonly Vector2 InfoTileSize = new Vector2(100f, 80f);

        public static readonly ControllerConfig StandAloneControllerConfig = new ControllerConfig {
            IgnoreEscapeKey = true,
            DeactivateOnOtherControllerActive = false,
            AllowInspectorCursor = true,
            BlockShortcuts = false,
            BlockCameraControlIfInputWasProcessed = false,
            PreventSpeedControl = false,
            Group = ControllerGroup.None,
        };

        public static readonly ControllerConfig BlockShortcutsToolConfig = new ControllerConfig {
            DeactivateOnNonUiClick = false,
            AllowInspectorCursor = false,
            DeactivateOnOtherControllerActive = true,
            BlockShortcuts = true,
            Group = ControllerGroup.Tool
        };
    }

    public static class IconPaths {
        public const string Status_BarDev = "Assets/DoubleQoL/StatusBar/Dev.png";
        public const string Tool_Select = "Assets/DoubleQoL/Tool/Select.png";
        public const string General_Download = "Assets/DoubleQoL/General/download.png";
        public const string General_Upload = "Assets/DoubleQoL/General/upload.png";
        public const string General_Star = "Assets/DoubleQoL/General/star.png";
    }
}