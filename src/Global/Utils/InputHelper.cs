using UnityEngine;

namespace DoubleQoL.Global.Utils {

    public class InputHelper {
        public static KeyCode[] ModifierKeys = { KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl };

        public static bool IsControlDown() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        public static bool IsShiftDown() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        public static bool IsAltDown() => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        public static bool IsModifierDown() => IsControlDown() || IsShiftDown() || IsAltDown();
    }
}