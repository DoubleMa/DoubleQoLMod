using System;

namespace DoubleQoL.Logging {

    public static class Log {
        private const string Prefix = "DoubleQoL_Mod: ";

        public static void Info(string message) => Mafi.Log.Info($"{Prefix}{message}");

        public static void InfoDebug(string message) => Mafi.Log.InfoDebug($"{Prefix}{message}");

        public static void Warning(string message) => Mafi.Log.Warning($"{Prefix}{message}");

        public static void WarningOnce(string message) => Mafi.Log.WarningOnce($"{Prefix}{message}");

        public static void Error(string message) => Mafi.Log.Error($"{Prefix}{message}");

        public static void Exception(Exception e, string message) => Mafi.Log.Exception(e, $"{Prefix}{message}");
    }
}