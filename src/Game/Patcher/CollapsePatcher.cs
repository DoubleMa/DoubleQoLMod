using DoubleQoL.Config;
using HarmonyLib;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using System.Collections.Generic;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    public class CollapsePatcher : APatcher {
        public static readonly CollapsePatcher Instance = new CollapsePatcher();

        public override bool DefaultState => ConfigManager.Instance.DefaultState_collapse.Value;
        public override bool Enabled => ConfigManager.Instance.QoLs_collapse.Value;

        protected override List<MethodInfo> MethodInfos => new List<MethodInfo>
        {
            AccessTools.Method(typeof(LayoutEntityBase), "TryCollapseOnUnevenTerrain"),
            AccessTools.Method(typeof(Transport), "TryCollapseOnUnevenTerrain"),
            AccessTools.Method(typeof(TransportPillar), "TryCollapseOnUnevenTerrain"),
            AccessTools.Method(typeof(TransportsManager), "TryCollapseSubTransport")
        };

        protected override HarmonyMethod MetPrefix => new HarmonyMethod(AccessTools.Method(typeof(CollapsePatcher), "MyPrefix"));

        protected override HarmonyMethod MetPostfix => new HarmonyMethod(AccessTools.Method(typeof(CollapsePatcher), "MyPostfix"));

        public CollapsePatcher() : base("Collapse") {
        }

        public override void OnInit(object obj) {
        }

        private static bool MyPrefix() => false;

        private static void MyPostfix(ref bool __result) => __result = false;
    }
}