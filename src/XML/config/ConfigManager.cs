using DoubleQoL.Global;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleQoL.XML.config {

    internal class ConfigManager {
        public static readonly ConfigManager Instance = new ConfigManager();

        private readonly KeyCode[] AcceptedKeyCodes = {
              KeyCode.None, KeyCode.Backspace, KeyCode.Delete, KeyCode.Tab, KeyCode.Return, KeyCode.Pause, KeyCode.Escape, KeyCode.Space,
              KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.KeypadPeriod, KeyCode.KeypadDivide, KeyCode.KeypadMultiply, KeyCode.KeypadMinus, KeyCode.KeypadPlus, KeyCode.KeypadEnter, KeyCode.KeypadEquals,
              KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.Insert, KeyCode.Home, KeyCode.End, KeyCode.PageUp, KeyCode.PageDown,
              KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12, KeyCode.F13, KeyCode.F14, KeyCode.F15,
              KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
              KeyCode.Exclaim, KeyCode.DoubleQuote, KeyCode.Hash, KeyCode.Dollar, KeyCode.Percent, KeyCode.Ampersand, KeyCode.Quote, KeyCode.LeftParen, KeyCode.RightParen, KeyCode.Asterisk, KeyCode.Plus, KeyCode.Comma, KeyCode.Minus, KeyCode.Period, KeyCode.Slash, KeyCode.Colon, KeyCode.Semicolon, KeyCode.Less, KeyCode.Equals, KeyCode.Greater, KeyCode.Question, KeyCode.At,
              KeyCode.LeftBracket, KeyCode.Backslash, KeyCode.RightBracket, KeyCode.Caret, KeyCode.Underscore, KeyCode.BackQuote,
              KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,
              KeyCode.LeftCurlyBracket, KeyCode.RightCurlyBracket, KeyCode.Tilde, KeyCode.Numlock, KeyCode.CapsLock, KeyCode.ScrollLock,
              KeyCode.RightShift, KeyCode.LeftShift, KeyCode.RightControl, KeyCode.LeftControl, KeyCode.RightAlt, KeyCode.LeftAlt, KeyCode.LeftMeta, KeyCode.LeftCommand, KeyCode.LeftApple, KeyCode.LeftWindows, KeyCode.RightMeta, KeyCode.RightCommand, KeyCode.RightApple, KeyCode.RightWindows,
              KeyCode.AltGr, KeyCode.Help, KeyCode.Print, KeyCode.Break, KeyCode.Menu,
              KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6
        };

        private readonly bool[] AcceptedBool = { true, false };
        private readonly string[] AcceptedTrucks = { "TruckT1", "TruckT2", "TruckT2H", "TruckT3Loose", "TruckT3LooseH", "TruckT3Fluid", "TruckT3FluidH" };
        private readonly string[] AcceptedNonTrucks = { "ExcavatorT1", "ExcavatorT2", "ExcavatorT2H", "ExcavatorT3", "ExcavatorT3H", "TreeHarvesterT1", "TreeHarvesterT2", "TreeHarvesterT2H", "TreePlanterT1", "TreePlanterT1H" };

        private readonly XSectionWithComment AppSettings;
        private readonly XSectionWithComment QoLs;
        private readonly XSectionWithComment KeyCodes;
        private readonly XSectionWithComment DefaultState;
        public XKeyWithComment<bool> AppSettings_isDev { get; }
        public XKeyWithComment<bool> QoLs_speed { get; }
        public XKeyWithComment<bool> QoLs_camera { get; }
        public XKeyWithComment<bool> QoLs_fog { get; }
        public XKeyWithComment<bool> QoLs_minetower { get; }
        public XKeyWithComment<bool> QoLs_vehicle { get; }
        public XKeyWithComment<bool> QoLs_vehicletool { get; }
        public XKeyWithComment<bool> QoLs_edgesizelimit { get; }
        public XKeyWithComment<bool> QoLs_statusbar { get; }

        public XKeyWithComment<KeyCode> KeyCodes_incSpeed { get; }
        public XKeyWithComment<KeyCode> KeyCodes_decSpeed { get; }
        public XKeyWithComment<KeyCode> KeyCodes_freeCamera { get; }
        public XKeyWithComment<KeyCode> KeyCodes_fog { get; }
        public XKeyWithComment<KeyCode> KeyCodes_Dev { get; }
        public XKeyWithComment<KeyCode> KeyCodes_vehicletool { get; }

        public XKeyWithComment<bool> DefaultState_freeCamera { get; }
        public XKeyWithComment<bool> DefaultState_fog { get; }
        public XKeyWithComment<int> DefaultState_edgesizelimit { get; }
        public XKeyWithComment<bool> DefaultState_statusbar { get; }
        public XKeyWithComment<string> DefaultState_trucktoshow { get; }
        public XKeyWithComment<string> DefaultState_nontrucktoshow { get; }
        public IEnumerable<ServerInfo> Blueprint_Servers { get; }

        private ConfigManager() {
            var loader = ConfigLoader.Instance;
            AppSettings = new XSectionWithComment(loader, "AppSettings");
            QoLs = new XSectionWithComment(loader, "QoLs", "\r\n\t\tEnable/Disable a qol feature.\r\n\t\tIf you disable a feature here even if you press its keycode it will not be activated in game.\r\n\r\n\t\tAccepted values true or false. Default: true\r\n\t");
            KeyCodes = new XSectionWithComment(loader, "KeyCodes", "\r\n\t\tSet the keycode for each feature.\r\n\t\tAccepted values (it's case-sensitive so make sure to copy paste the key name from this list, otherwise the default value will be selected):\r\n\t\t\tNone, Backspace, Delete, Tab, Return, Pause, Escape, Space,\r\n\t\t\tKeypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals,\r\n\t\t\tUpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown,\r\n\t\t\tF1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15,\r\n\t\t\tAlpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,\r\n\t\t\tExclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Colon, Semicolon, Less, Equals, Greater, Question, At,\r\n\t\t\tLeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote,\r\n\t\t\tA, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,\r\n\t\t\tLeftCurlyBracket, RightCurlyBracket, Tilde, Numlock, CapsLock, ScrollLock,\r\n\t\t\tRightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, LeftMeta, LeftCommand, LeftApple, LeftWindows, RightMeta, RightCommand, RightApple, RightWindows,\r\n\t\t\tAltGr, Help, Print, Break, Menu,\r\n\t\t\tMouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6\r\n\t");
            DefaultState = new XSectionWithComment(loader, "DefaultState", "\r\n\t\tSet the default state of each feature when you start the game (if it's enabled or disabled)\r\n\t\tEnabling or disabling a feature here will not disable its keycode, you can always change its state in game\r\n\t\tSome features don't have a default state and can only be disabled or enabled in QoLs section (Like: incSpeed, decSpeed...)\r\n\r\n\t\tAccepted values true or false.\r\n\t");

            AppSettings_isDev = new XKeyWithComment<bool>(loader, AppSettings, "isDev", AcceptedBool, false, "Enable/Disable dev mode (ignore this, it's only for testing)");

            QoLs_speed = new XKeyWithComment<bool>(loader, QoLs, "speed", AcceptedBool, true, "Add Game Speed controller");
            QoLs_camera = new XKeyWithComment<bool>(loader, QoLs, "camera", AcceptedBool, true, "Free Camera with a 6000 max Pivot Distance");
            QoLs_fog = new XKeyWithComment<bool>(loader, QoLs, "fog", AcceptedBool, true, "Enable/disable fog");
            QoLs_minetower = new XKeyWithComment<bool>(loader, QoLs, "minetower", AcceptedBool, true, "Add a field in the MineTower view where you can prioritize a specific resource for all excavators");
            QoLs_vehicle = new XKeyWithComment<bool>(loader, QoLs, "vehicle", AcceptedBool, true, "Add a field in the vehicle view where you can cancel all jobs, and add buttons to upgrade the assigned vehicles in most windows");
            QoLs_edgesizelimit = new XKeyWithComment<bool>(loader, QoLs, "terraindesignations", AcceptedBool, true, "Change the area size of terrain designations, and the size limit for tools");
            QoLs_vehicletool = new XKeyWithComment<bool>(loader, QoLs, "vehicletool", AcceptedBool, true, "A tool to select and move vehicles to one location");
            QoLs_statusbar = new XKeyWithComment<bool>(loader, QoLs, "statusbar", AcceptedBool, true, "\r\n\t\tAdd more info under the statusbar.\r\n\t\tRight-click any of the health, unity, or truck icons in the statusbar to activate it\r\n\t");

            KeyCodes_incSpeed = new XKeyWithComment<KeyCode>(loader, KeyCodes, "incSpeed", AcceptedKeyCodes, KeyCode.Alpha9, "\r\n\t\tKeyCode to increase game speed (note make sure you choose different keys from the one in game like plus and minus)\r\n\t\tDefault: Alpha9\r\n\t");
            KeyCodes_decSpeed = new XKeyWithComment<KeyCode>(loader, KeyCodes, "decSpeed", AcceptedKeyCodes, KeyCode.Alpha8, "\r\n\t\tKeyCode to decrease game speed (note make sure you choose different keys from the one in game like plus and minus)\r\n\t\tDefault: Alpha8\r\n\t");
            KeyCodes_freeCamera = new XKeyWithComment<KeyCode>(loader, KeyCodes, "freeCamera", AcceptedKeyCodes, KeyCode.F9, "\r\n\t\tKeyCode to enable/disable freeCamera\r\n\t\tDefault: F9\r\n\t");
            KeyCodes_fog = new XKeyWithComment<KeyCode>(loader, KeyCodes, "fog", AcceptedKeyCodes, KeyCode.F10, "\r\n\t\tKeyCode to enable/disable fog\r\n\t\tDefault: F10\r\n\t");
            KeyCodes_vehicletool = new XKeyWithComment<KeyCode>(loader, KeyCodes, "vehicletool", AcceptedKeyCodes, KeyCode.Comma, "\r\n\t\tKeyCode to enable/disable vehicle tool\r\n\t\tDefault: Comma\r\n\t");
            KeyCodes_Dev = new XKeyWithComment<KeyCode>(loader, KeyCodes, "dev", AcceptedKeyCodes, KeyCode.F6, "Enable/Disable dev mode (ignore this, it's only for testing)");

            DefaultState_freeCamera = new XKeyWithComment<bool>(loader, DefaultState, "freeCamera", AcceptedBool, false, "Default: false");
            DefaultState_fog = new XKeyWithComment<bool>(loader, DefaultState, "fog", AcceptedBool, true, "Default: true");
            DefaultState_edgesizelimit = new XKeyWithComment<int>(loader, DefaultState, "terraindesignations", new int[] { 10, Static.MaxMapSize }, Static.MaxMapSize, $"\r\n\t\tAccepted values: 10 - {Static.MaxMapSize}\r\n\t\tDefault: {Static.MaxMapSize}\r\n\t");
            DefaultState_statusbar = new XKeyWithComment<bool>(loader, DefaultState, "statusbar", AcceptedBool, false, "\r\n\t\tDefault state of the statusbar details\r\n\t\tDefault: false\r\n\t");
            var allTrucks = string.Join(", ", AcceptedTrucks);
            var allOthers = string.Join(", ", AcceptedNonTrucks);
            DefaultState_trucktoshow = new XKeyWithComment<string>(loader, DefaultState, "trucktoshow", AcceptedTrucks, allTrucks, $"\r\n\t\tType of trucks to show in the statusbar.\r\n\t\tAccepted values: {allTrucks}\r\n\t\tDefault: {allTrucks}\r\n\t");
            DefaultState_nontrucktoshow = new XKeyWithComment<string>(loader, DefaultState, "nontrucktoshow", AcceptedNonTrucks, allOthers, $"\r\n\t\tType of non-trucks to show in the statusbar.\r\n\t\tAccepted values: {allOthers}\r\n\t\tDefault: {allOthers}\r\n\t");

            Blueprint_Servers = ConfigLoader.Instance.GetAllServers();
        }
    }

    internal class ServerInfo {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }
}