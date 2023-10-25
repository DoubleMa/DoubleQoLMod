using DoubleQoL.Config;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;

namespace DoubleQoL.Game.Patcher {

    internal class CollapsePatcher : APatcher<CollapsePatcher> {
        public override bool DefaultState => ConfigManager.Instance.DefaultState_collapse.Value;
        public override bool Enabled => ConfigManager.Instance.QoLs_collapse.Value;

        public CollapsePatcher() : base("Collapse") {
            AddMethod<LayoutEntityBase>("TryCollapseOnUnevenTerrain", PostfixFalse);
            AddMethod<Transport>("TryCollapseOnUnevenTerrain", PostfixFalse);
            AddMethod<TransportPillar>("TryCollapseOnUnevenTerrain", PostfixFalse);
            AddMethod<TransportsManager>("TryCollapseSubTransport", PostfixFalse);
        }
    }
}