using DoubleQoL.Config;
using DoubleQoL.Extensions;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;

namespace DoubleQoL.Game.Patcher {

    internal class CollapsePatcher : APatcher {
        public static readonly CollapsePatcher Instance = new CollapsePatcher();
        public override bool DefaultState => ConfigManager.Instance.DefaultState_collapse.Value;
        public override bool Enabled => ConfigManager.Instance.QoLs_collapse.Value;

        public CollapsePatcher() : base("Collapse") {
            var Postfix = this.GetHarmonyMethod("MyPostfix");
            AddMethod<LayoutEntityBase>("TryCollapseOnUnevenTerrain", Postfix);
            AddMethod<Transport>("TryCollapseOnUnevenTerrain", Postfix);
            AddMethod<TransportPillar>("TryCollapseOnUnevenTerrain", Postfix);
            AddMethod<TransportsManager>("TryCollapseSubTransport", Postfix);
        }

        private static void MyPostfix(ref bool __result) => __result = false;
    }
}