using DoubleQoL.Game.Patcher.Helper;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    internal abstract class APatcher {
        public string Category { get; }
        public string PatcherID { get; }
        public bool IsActive { get; private set; } = false;
        public abstract bool DefaultState { get; }
        public abstract bool Enabled { get; }

        private Harmony harmony;
        protected List<MethodToPatch> MethodInfos { get; }

        public static readonly MethodInfo PrefixAllow = AccessTools.Method(typeof(APatcher), "MyPrefixAllow");
        public static readonly MethodInfo PrefixBlock = AccessTools.Method(typeof(APatcher), "MyPrefixBlock");

        private static bool MyPrefixAllow() => true;

        private static bool MyPrefixBlock() => false;

        public APatcher(string name) {
            Category = $"{name}PatcherCategory";
            PatcherID = $"com.{name.ToLower()}.patch";
            MethodInfos = new List<MethodToPatch>();
        }

        public void Init() {
            harmony = new Harmony(PatcherID);
            OnInit();
            Patch(DefaultState);
        }

        public virtual void OnInit() {
        }

        public void Toggle() => Patch(!IsActive);

        public void Activate() => Patch(true);

        public void Disable() => Patch(false);

        protected virtual void Patch(bool enable = false) {
            if (!Enabled || IsActive == enable) return;
            foreach (var m in MethodInfos) {
                if (m.MethodInfo is null) continue;
                harmony.Unpatch(m.MethodInfo, HarmonyPatchType.All, PatcherID);
                if (enable) harmony.Patch(m.MethodInfo, m.Prefix, m.Postfix);
            }
            IsActive = enable;
        }

        protected void AddMethod(MethodInfo methodInfo, MethodInfo prefix, MethodInfo postfix) => MethodInfos.Add(new MethodToPatch(methodInfo, new HarmonyMethod(prefix), new HarmonyMethod(postfix)));

        protected void AddMethod(MethodInfo methodInfo, MethodInfo postfix, bool allow) => AddMethod(methodInfo, allow ? PrefixAllow : PrefixBlock, postfix);

        protected void AddAllowedMethod(MethodInfo methodInfo, MethodInfo postfix) => AddMethod(methodInfo, postfix, true);

        protected void AddBlockedMethod(MethodInfo methodInfo, MethodInfo postfix) => AddMethod(methodInfo, postfix, false);
    }
}