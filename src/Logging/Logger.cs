using System;

namespace DoubleQoL.Logging {

    public static class Log {
        private const string Prefix = "DoubleQoL_Mod: ";

        public static string addPrefix(this string message) => $"{Prefix}{message}";

        private static void log(object message, Action<string> callback = null) {
            Console.WriteLine(message?.ToString().addPrefix());
            callback?.Invoke(message?.ToString().addPrefix());
        }

        private static void log(Exception e, object message, Action<Exception, string> callback = null) {
            Console.WriteLine(message?.ToString().addPrefix());
            callback?.Invoke(e, message?.ToString().addPrefix());
        }

        public static void Info(object message) => log(message, Mafi.Log.Info);

        public static void InfoDebug(object message) => log(message, Mafi.Log.InfoDebug);

        public static void Warning(object message) => log(message, Mafi.Log.Warning);

        public static void WarningOnce(object message) => log(message, Mafi.Log.WarningOnce);

        public static void Error(object message) => log(message, Mafi.Log.Error);

        public static void Exception(Exception e, object message) => log(e, message, Mafi.Log.Exception);
    }
}