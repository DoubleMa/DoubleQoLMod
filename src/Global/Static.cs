using Mafi.Unity.InputControl;
using UnityEngine;

namespace DoubleQoL.Global
{
    public static class Static
    {
        public static Vector2 WindowSize = new Vector2(680f, 400f);

        public static ControllerConfig StandAloneControllerConfig = new ControllerConfig
        {
            IgnoreEscapeKey = true,
            DeactivateOnOtherControllerActive = false,
            AllowInspectorCursor = true,
            BlockShortcuts = false,
            BlockCameraControlIfInputWasProcessed = false,
            PreventSpeedControl = false,
            Group = ControllerGroup.None,
        };
    }

    public static class IconPaths
    {
        public const string ToolbarYinYang = "Assets/DoubleQoL/Toolbar/yin-yang.512x512.png";
        public const string StatusBarFog = "Assets/DoubleQoL/StatusBar/Fog.png";
        public const string StatusBarDev = "Assets/DoubleQoL/StatusBar/Dev.png";
    }
}