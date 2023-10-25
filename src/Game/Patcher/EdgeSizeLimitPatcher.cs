using DoubleQoL.Config;
using DoubleQoL.Extensions;
using Mafi;
using Mafi.Core.Terrain.Designation;
using Mafi.Unity.InputControl.AreaTool;
using Mafi.Unity.Mine;
using System;

namespace DoubleQoL.Game.Patcher {

    internal class EdgeSizeLimitPatcher : APatcher<EdgeSizeLimitPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_edgesizelimit.Value;
        private static int MaxSize => ConfigManager.Instance.DefaultState_edgesizelimit.Value;
        private static readonly int DEFAULT_MAX_AREA_SIZE_ADD = sbyte.MaxValue;
        private static readonly int DEFAULT_MAX_AREA_SIZE_REMOVE = 191;

        public EdgeSizeLimitPatcher() : base("EdgeSizeLimit") {
            AddMethod<TerrainDesignationsManager>("GetCanonicalDesignationRange", this.GetHarmonyMethod("MyPostfix"));
            AddMethod(typeof(AreaSelectionTool), "SetEdgeSizeLimit", this.GetHarmonyMethod("MyEdgePostfix"));
        }

        protected override void Patch(bool enable = false) {
            if (!Enabled || IsActive == enable) return;
            Type typ = typeof(TerrainDesignationController);
            typ.SetField(null, "MAX_AREA_SIZE_ADD", new RelTile1i(enable ? MaxSize : DEFAULT_MAX_AREA_SIZE_ADD));
            typ.SetField(null, "MAX_AREA_SIZE_REMOVE", new RelTile1i(enable ? MaxSize : DEFAULT_MAX_AREA_SIZE_REMOVE));
            base.Patch(enable);
        }

        private static void MyPostfix(Tile2i fromCoord, Tile2i toCoord, ref Tile2i minCoord, ref Tile2i maxCoord) {
            minCoord = TerrainDesignation.GetOrigin(fromCoord.Min(toCoord));
            maxCoord = TerrainDesignation.GetOrigin(fromCoord.Max(toCoord));
            RelTile2i relTile2i = maxCoord - minCoord;
            if (relTile2i.X > MaxSize) maxCoord = maxCoord.SetX(minCoord.X + MaxSize);
            if (relTile2i.Y > MaxSize) maxCoord = maxCoord.SetY(minCoord.Y + MaxSize);
        }

        private static void MyEdgePostfix(AreaSelectionTool __instance) => __instance.SetField("m_maxEdgeSize", new RelTile1i(MaxSize));
    }
}