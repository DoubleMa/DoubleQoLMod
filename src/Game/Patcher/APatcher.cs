using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    public abstract class APatcher {
        public string Category { get; }
        public string PatcherID { get; }
        public bool IsActive { get; private set; } = false;
        public abstract bool DefaultState { get; }
        public abstract bool Enabled { get; }

        private Harmony harmony;
        protected abstract List<MethodInfo> MethodInfos { get; }
        protected abstract HarmonyMethod MetPrefix { get; }
        protected abstract HarmonyMethod MetPostfix { get; }

        public APatcher(string name) {
            Category = $"{name}PatcherCategory";
            PatcherID = $"com.{name.ToLower()}.patch"; ;
        }

        public void Init(object obj = null) {
            harmony = new Harmony(PatcherID);
            OnInit(obj);
            Patch(DefaultState);
        }

        public abstract void OnInit(object obj);

        public void Toggle() => Patch(!IsActive);

        public void Activate() => Patch(true);

        public void Disable() => Patch(false);

        protected virtual void Patch(bool enable = false) {
            if (!Enabled || IsActive == enable) return;
            foreach (var m in MethodInfos) {
                harmony.Unpatch(m, HarmonyPatchType.All, PatcherID);
                if (enable) harmony.Patch(m, MetPrefix, MetPostfix);
            }
            IsActive = enable;
        }
    }
}