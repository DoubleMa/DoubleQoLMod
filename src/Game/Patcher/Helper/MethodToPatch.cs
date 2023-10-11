using HarmonyLib;
using System.Reflection;

namespace DoubleQoL.Game.Patcher.Helper {

    internal class MethodToPatch {
        public MethodInfo MethodInfo { get; }
        public HarmonyMethod Prefix { get; }
        public HarmonyMethod Postfix { get; }

        public MethodToPatch(MethodInfo methodInfo, HarmonyMethod prefix, HarmonyMethod postfix) {
            MethodInfo = methodInfo;
            Prefix = prefix;
            Postfix = postfix;
        }
    }
}