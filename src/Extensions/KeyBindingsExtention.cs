using DoubleQoL.Global.Utils;
using Mafi.Collections;
using Mafi.Unity.InputControl;
using System.Linq;
using UnityEngine;

namespace DoubleQoL.Extensions {

    public static class KeyBindingsExtention {

        public static bool IsModifier(this KeyCode keyCode) => InputHelper.ModifierKeys.Contains(keyCode);

        public static Lyst<KeyCode> GetModifiers(this KeyBindings keyBindings) => keyBindings.Primary.Keys.Filter(e => e.IsModifier()).ToLyst();

        public static Lyst<KeyCode> GetNonModifiers(this KeyBindings keyBindings) => keyBindings.Primary.Keys.Filter(e => !e.IsModifier()).ToLyst();

        public static bool HasModifiers(this KeyBindings keyBindings) => keyBindings.GetModifiers().Count > 0;

        public static bool IsPrimaryOn(this KeyBindings keyBindings, ShortcutsManager shortcutsManager, bool ignoreModifiers = false) => shortcutsManager.IsOn(keyBindings) && (ignoreModifiers || (keyBindings.HasModifiers() || !InputHelper.IsModifierDown()));

        public static bool IsPrimaryOn(this KeyBindings keyBindings, bool ignoreModifiers = false) {
            if (keyBindings.Primary.IsEmpty) return false;
            if (!ignoreModifiers && !keyBindings.HasModifiers() && InputHelper.IsModifierDown()) return false;
            foreach (var key in keyBindings.Primary.Keys) if (!Input.GetKey(key)) return false;
            return true;
        }
    }
}