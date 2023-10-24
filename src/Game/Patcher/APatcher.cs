using DoubleQoL.Extensions;
using DoubleQoL.Game.Patcher.Helper;
using HarmonyLib;
using Mafi;
using System;
using System.Collections.Generic;

namespace DoubleQoL.Game.Patcher {

    internal abstract class APatcher {
        public string Category { get; }
        public string PatcherID { get; }
        public bool IsActive { get; private set; } = false;
        public abstract bool DefaultState { get; }
        public abstract bool Enabled { get; }

        private Harmony harmony;
        protected List<MethodToPatch> MethodInfos { get; }

        public static readonly HarmonyMethod PrefixAllow = typeof(APatcher).GetHarmonyMethod("MyPrefixAllow");
        public static readonly HarmonyMethod PrefixBlock = typeof(APatcher).GetHarmonyMethod("MyPrefixBlock");
        private static DependencyResolver Resolver;

        private static bool MyPrefixAllow() => true;

        private static bool MyPrefixBlock() => false;

        public APatcher(string name) {
            Category = $"{name}PatcherCategory";
            PatcherID = $"com.{name.ToLower()}.patch";
            MethodInfos = new List<MethodToPatch>();
        }

        public void Init(DependencyResolver resolver) {
            Resolver = resolver;
            harmony = new Harmony(PatcherID);
            OnInit(resolver);
            Patch(DefaultState);
        }

        protected static T GetInstance<T>() where T : class => Resolver?.GetResolvedInstance<T>().Value;

        public virtual void OnInit(DependencyResolver resolver) {
        }

        public void Toggle() => Patch(!IsActive);

        public void Activate() => Patch(true);

        public void Disable() => Patch(false);

        protected virtual void Patch(bool enable = false) {
            if (!Enabled || IsActive == enable) return;
            foreach (var m in MethodInfos) {
                var mt = m?.ToPatch?.method;
                if (mt is null) continue;
                harmony.Unpatch(mt, HarmonyPatchType.All, PatcherID);
                if (enable) harmony.Patch(mt, m.Prefix, m.Postfix);
            }
            IsActive = enable;
        }

        protected void AddMethod(HarmonyMethod methodInfo, HarmonyMethod prefix, HarmonyMethod postfix) => MethodInfos.Add(new MethodToPatch(methodInfo, prefix, postfix));

        protected void AddMethod(Type t, string method, HarmonyMethod prefix, HarmonyMethod postfix) => AddMethod(t.GetHarmonyMethod(method), prefix, postfix);

        protected void AddMethod<T>(string method, HarmonyMethod prefix, HarmonyMethod postfix) => AddMethod(typeof(T), method, prefix, postfix);

        protected void AddMethod(HarmonyMethod methodInfo, HarmonyMethod postfix, bool allow = false) => AddMethod(methodInfo, allow ? PrefixAllow : PrefixBlock, postfix);

        protected void AddMethod(Type t, string method, HarmonyMethod postfix, bool allow = false) => AddMethod(t.GetHarmonyMethod(method), postfix, allow);

        protected void AddMethod<T>(string method, HarmonyMethod postfix, bool allow = false) => AddMethod(typeof(T), method, postfix, allow);
    }
}