using Mafi.Unity.InputControl;
using System;
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

        public static int Between(this int x, int min, int max) => Math.Max(min, Math.Min(max, x));
    }
}