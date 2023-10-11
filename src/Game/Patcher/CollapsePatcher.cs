using DoubleQoL.Config;
using HarmonyLib;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;

namespace DoubleQoL.Game.Patcher {

    internal class CollapsePatcher : APatcher {
        public static readonly CollapsePatcher Instance = new CollapsePatcher();
        public override bool DefaultState => ConfigManager.Instance.DefaultState_collapse.Value;
        public override bool Enabled => ConfigManager.Instance.QoLs_collapse.Value;

        public CollapsePatcher() : base("Collapse") {
            var Postfix = AccessTools.Method(GetType(), "MyPostfix");
            AddBlockedMethod(AccessTools.Method(typeof(LayoutEntityBase), "TryCollapseOnUnevenTerrain"), Postfix);
            AddBlockedMethod(AccessTools.Method(typeof(Transport), "TryCollapseOnUnevenTerrain"), Postfix);
            AddBlockedMethod(AccessTools.Method(typeof(TransportPillar), "TryCollapseOnUnevenTerrain"), Postfix);
            AddBlockedMethod(AccessTools.Method(typeof(TransportsManager), "TryCollapseSubTransport"), Postfix);
        }

        private static void MyPostfix(ref bool __result) => __result = false;
    }
}