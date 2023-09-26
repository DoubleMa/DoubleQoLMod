using DoubleQoL.Config;
using HarmonyLib;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using System.Collections.Generic;
using System.Reflection;

namespace DoubleQoL.Game.Patcher
{
    public class CollapsePatcher
    {
        public static readonly CollapsePatcher Instance = new CollapsePatcher();

        public const string CollapsePatcherCategory = "CollapsePatcherCategory";
        public const string CollapsePatcherID = "com.collapse.patch";
        public bool isActive { get; private set; } = false;
        private Harmony harmony;

        private List<MethodInfo> methodInfos;
        private HarmonyMethod mPrefix;
        private HarmonyMethod mPostfix;

        public CollapsePatcher()
        {
            harmony = new Harmony(CollapsePatcherID);
            methodInfos = new List<MethodInfo>
            {
                 AccessTools.Method(typeof(LayoutEntityBase), "TryCollapseOnUnevenTerrain"),
                 AccessTools.Method(typeof(Transport), "TryCollapseOnUnevenTerrain"),
                 AccessTools.Method(typeof(TransportPillar), "TryCollapseOnUnevenTerrain"),
                 AccessTools.Method(typeof(TransportsManager), "TryCollapseSubTransport")
            };
            mPrefix = new HarmonyMethod(AccessTools.Method(typeof(CollapsePatcher), "MyPrefix"));
            mPostfix = new HarmonyMethod(AccessTools.Method(typeof(CollapsePatcher), "MyPostfix"));

            Patch(ConfigManager.Instance.DefaultState_collapse.getBoolValue());
        }

        public void Toggle() => Patch(!isActive);

        public void Activate() => Patch(true);

        public void Disable() => Patch(false);

        private void Patch(bool enable = false)
        {
            if (isActive == enable || !ConfigManager.Instance.QoLs_collapse.getBoolValue()) return;
            foreach (var m in methodInfos)
            {
                harmony.Unpatch(m, HarmonyPatchType.All, CollapsePatcherID);
                if (enable) harmony.Patch(m, mPrefix, mPostfix);
            }
            isActive = enable;
        }

        private static bool MyPrefix() => false;

        private static void MyPostfix(ref bool __result) => __result = false;
    }
}