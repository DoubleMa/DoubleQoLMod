namespace DoubleQoL.Config
{
    internal class ConfigManager
    {
        public static readonly ConfigManager Instance = new ConfigManager();

        private readonly string[] AcceptedKeyCodes = {
            "Backspace", "Delete", "Tab", "Return", "Pause", "Escape", "Space",
            "Keypad0", "Keypad1", "Keypad2", "Keypad3", "Keypad4", "Keypad5", "Keypad6", "Keypad7", "Keypad8", "Keypad9", "KeypadPeriod", "KeypadDivide", "KeypadMultiply", "KeypadMinus", "KeypadPlus", "KeypadEnter", "KeypadEquals",
            "UpArrow", "DownArrow", "RightArrow", "LeftArrow", "Insert", "Home", "End", "PageUp", "PageDown",
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15",
            "Alpha0", "Alpha1", "Alpha2", "Alpha3", "Alpha4", "Alpha5", "Alpha6", "Alpha7", "Alpha8", "Alpha9",
            "Exclaim", "DoubleQuote", "Hash", "Dollar", "Percent", "Ampersand", "Quote", "LeftParen", "RightParen", "Asterisk", "Plus", "Comma", "Minus", "Period", "Slash", "Colon", "Semicolon", "Less", "Equals", "Greater", "Question", "At",
            "LeftBracket", "Backslash", "RightBracket", "Caret", "Underscore", "BackQuote",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "LeftCurlyBracket", "RightCurlyBracket", "Tilde", "Numlock", "CapsLock", "ScrollLock",
            "RightShift", "LeftShift", "RightControl", "LeftControl", "RightAlt", "LeftAlt", "LeftMeta", "LeftCommand", "LeftApple", "LeftWindows", "RightMeta", "RightCommand", "RightApple", "RightWindows",
            "AltGr", "Help", "Print", "Break", "Menu",
            "Mouse0", "Mouse1", "Mouse2", "Mouse3", "Mouse4", "Mouse5", "Mouse6"
        };

        private readonly string[] AcceptedBool = { "true", "false" };

        private readonly XSectionWithComment AppSettings;
        private readonly XSectionWithComment QoLs;
        private readonly XSectionWithComment KeyCodes;
        private readonly XSectionWithComment DefaultState;

        public XKeyWithComment AppSettings_isDev { get; }
        public XKeyWithComment QoLs_speed { get; }
        public XKeyWithComment QoLs_camera { get; }
        public XKeyWithComment QoLs_fog { get; }
        public XKeyWithComment QoLs_collapse { get; }
        public XKeyWithComment QoLs_minetower { get; }
        public XKeyWithComment QoLs_vehicle { get; }
        public XKeyWithComment QoLs_towerarea { get; }

        public XKeyWithComment KeyCodes_incSpeed { get; }
        public XKeyWithComment KeyCodes_decSpeed { get; }
        public XKeyWithComment KeyCodes_freeCamera { get; }
        public XKeyWithComment KeyCodes_fog { get; }
        public XKeyWithComment KeyCodes_collapse { get; }
        public XKeyWithComment KeyCodes_Dev { get; }

        public XKeyWithComment DefaultState_freeCamera { get; }
        public XKeyWithComment DefaultState_fog { get; }
        public XKeyWithComment DefaultState_collapse { get; }
        public XKeyWithComment DefaultState_towerarea { get; }

        private ConfigManager()
        {
            AppSettings = new XSectionWithComment("AppSettings");
            QoLs = new XSectionWithComment("QoLs", "\r\n\t\tEnable/Disable a qol feature.\r\n\t\tIf you disable a feature here even if you press its keycode it will not be activated in game.\r\n\r\n\t\tAccepted values true or false. Default: true\r\n\t");
            KeyCodes = new XSectionWithComment("KeyCodes", "\r\n\t\tSet the keycode for each feature.\r\n\t\tAccepted values (it's case-sensitive so make sure to copy paste the key name from this list, otherwise the default value will be selected):\r\n\t\t\tBackspace, Delete, Tab, Return, Pause, Escape, Space,\r\n\t\t\tKeypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals,\r\n\t\t\tUpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown,\r\n\t\t\tF1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15,\r\n\t\t\tAlpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,\r\n\t\t\tExclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Colon, Semicolon, Less, Equals, Greater, Question, At,\r\n\t\t\tLeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote,\r\n\t\t\tA, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,\r\n\t\t\tLeftCurlyBracket, RightCurlyBracket, Tilde, Numlock, CapsLock, ScrollLock,\r\n\t\t\tRightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, LeftMeta, LeftCommand, LeftApple, LeftWindows, RightMeta, RightCommand, RightApple, RightWindows,\r\n\t\t\tAltGr, Help, Print, Break, Menu,\r\n\t\t\tMouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6\r\n\t");
            DefaultState = new XSectionWithComment("DefaultState", "\r\n\t\tSet the default state of each feature when you start the game (if it's enabled or disabled)\r\n\t\tEnabling or disabling a feature here will not disable its keycode, you can always change its state in game\r\n\t\tSome features don't have a default state and can only be disabled or enabled in QoLs section (Like: incSpeed, decSpeed...)\r\n\r\n\t\tAccepted values true or false.\r\n\t");

            AppSettings_isDev = new XKeyWithComment(AppSettings, "isDev", AcceptedBool, "false", "Enable/Disable dev mode (ignore this, it's only for testing)");

            QoLs_speed = new XKeyWithComment(QoLs, "speed", AcceptedBool, "true", "Add Game Speed controller");
            QoLs_camera = new XKeyWithComment(QoLs, "camera", AcceptedBool, "true", "Free Camera with a 6000 max Pivot Distance");
            QoLs_fog = new XKeyWithComment(QoLs, "fog", AcceptedBool, "true", "Enable/disable fog");
            QoLs_collapse = new XKeyWithComment(QoLs, "collapse", AcceptedBool, "true", "Enable/disable building and transports collapse");
            QoLs_minetower = new XKeyWithComment(QoLs, "minetower", AcceptedBool, "true", "Add a field in the MineTower view where you can prioritize a specific resource for all excavators");
            QoLs_vehicle = new XKeyWithComment(QoLs, "vehicle", AcceptedBool, "true", "Add a field in the vehicle view where you can cancel all jobs");
            QoLs_towerarea = new XKeyWithComment(QoLs, "towerarea", AcceptedBool, "true", "Change the area size of towers like the MineTower and the ForestryTower");

            KeyCodes_incSpeed = new XKeyWithComment(KeyCodes, "incSpeed", AcceptedKeyCodes, "Alpha9", "\r\n\t\tKeyCode to increase game speed (note make sure you choose different keys from the one in game like plus and minus)\r\n\t\tDefault: Alpha9\r\n\t");
            KeyCodes_decSpeed = new XKeyWithComment(KeyCodes, "decSpeed", AcceptedKeyCodes, "Alpha8", "\r\n\t\tKeyCode to decrease game speed (note make sure you choose different keys from the one in game like plus and minus)\r\n\t\tDefault: Alpha8\r\n\t");
            KeyCodes_freeCamera = new XKeyWithComment(KeyCodes, "freeCamera", AcceptedKeyCodes, "F9", "\r\n\t\tKeyCode to enable/disable freeCamera\r\n\t\tDefault: F9\r\n\t");
            KeyCodes_fog = new XKeyWithComment(KeyCodes, "fog", AcceptedKeyCodes, "F10", "\r\n\t\tKeyCode to enable/disable fog\r\n\t\tDefault: F10\r\n\t");
            KeyCodes_collapse = new XKeyWithComment(KeyCodes, "collapse", AcceptedKeyCodes, "Keypad9", "\r\n\t\tKeyCode to enable/disable building and transport collapse\r\n\t\tDefault: Keypad9\r\n\t");
            KeyCodes_Dev = new XKeyWithComment(KeyCodes, "dev", AcceptedKeyCodes, "F6", "Enable/Disable dev mode (ignore this, it's only for testing)");

            DefaultState_freeCamera = new XKeyWithComment(DefaultState, "freeCamera", AcceptedBool, "false", "Default: false");
            DefaultState_fog = new XKeyWithComment(DefaultState, "fog", AcceptedBool, "true", "Default: true");
            DefaultState_collapse = new XKeyWithComment(DefaultState, "collapse", AcceptedBool, "false", "Default: false");
            DefaultState_towerarea = new XKeyWithComment(DefaultState, "towerarea", new int[] { 1, 24 }, "2", "\r\n\t\tAccepted values: 1 - 24\r\n\t\tDefault: 2\r\n\t");
        }
    }
}