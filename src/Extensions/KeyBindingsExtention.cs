using DoubleQoL.Global.Utils;
using Mafi.Collections;
using Mafi.Unity.InputControl;
using System.Linq;
using UnityEngine;

namespace DoubleQoL.Extensions {

    internal static class KeyBindingsExtention {

        public static bool IsModifier(this KeyCode keyCode) => InputHelper.ModifierKeys.Contains(keyCode);

        public static Lyst<KeyCode> GetModifiers(this KeyBindings keyBindings) => keyBindings.Primary.Keys.Filter(e => e.IsModifier()).ToLyst();

        public static Lyst<KeyCode> GetNonModifiers(this KeyBindings keyBindings) => keyBindings.Primary.Keys.Filter(e => !e.IsModifier()).ToLyst();

        public static bool HasModifiers(this KeyBindings keyBindings) => keyBindings.GetModifiers().Count > 0;

        public static bool IsNone(this KeyBinding keyBinding) => keyBinding.IsEmpty || keyBinding.IsCode(KeyCode.None);

        public static bool IsNone(this KeyBindings keyBindings) => keyBindings.Primary.IsNone() && keyBindings.Secondary.IsNone();

        public static bool IsPrimaryOn(this KeyBindings keyBindings, ShortcutsManager shortcutsManager, bool ignoreModifiers = false) => !keyBindings.Primary.IsNone() && shortcutsManager.IsOn(keyBindings) && (ignoreModifiers || (keyBindings.HasModifiers() || !InputHelper.IsModifierDown()));

        public static bool IsPrimaryOn(this KeyBindings keyBindings, bool ignoreModifiers = false) {
            if (keyBindings.Primary.IsEmpty || keyBindings.Primary.IsNone()) return false;
            if (!ignoreModifiers && !keyBindings.HasModifiers() && InputHelper.IsModifierDown()) return false;
            foreach (var key in keyBindings.Primary.Keys) if (!Input.GetKey(key)) return false;
            return true;
        }

        public static bool IsDownNone(this ShortcutsManager shortcutsManager, KeyBindings keyBindings) => !keyBindings.IsNone() && shortcutsManager.IsDown(keyBindings);
    }
}