using DoubleQoL.Config;
using DoubleQoL.Extensions;
using Mafi;
using Mafi.Core.Terrain.Designation;
using Mafi.Unity.Mine;
using System;

namespace DoubleQoL.Game.Patcher {

    internal class TerrainDesignationsPatcher : APatcher {
        public static readonly TerrainDesignationsPatcher Instance = new TerrainDesignationsPatcher();
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_terraindesignations.Value;
        private static int MaxSize => ConfigManager.Instance.DefaultState_terraindesignations.Value;
        private static readonly int MAX_AREA_SIZE_ADD = sbyte.MaxValue;
        private static readonly int MAX_AREA_SIZE_REMOVE = 191;

        private TerrainDesignationsPatcher() : base("TerrainDesignations") {
            AddMethod<TerrainDesignationsManager>("GetCanonicalDesignationRange", this.GetHarmonyMethod("MyPostfix"));
        }

        protected override void Patch(bool enable = false) {
            if (!Enabled || IsActive == enable) return;
            Type typ = typeof(TerrainDesignationController);
            typ.SetField(null, "MAX_AREA_SIZE_ADD", new RelTile1i(enable ? MaxSize : MAX_AREA_SIZE_ADD));
            typ.SetField(null, "MAX_AREA_SIZE_REMOVE", new RelTile1i(enable ? MaxSize : MAX_AREA_SIZE_REMOVE));
            base.Patch(enable);
        }

        private static void MyPostfix(Tile2i fromCoord, Tile2i toCoord, ref Tile2i minCoord, ref Tile2i maxCoord) {
            minCoord = TerrainDesignation.GetOrigin(fromCoord.Min(toCoord));
            maxCoord = TerrainDesignation.GetOrigin(fromCoord.Max(toCoord));
            RelTile2i relTile2i = maxCoord - minCoord;

            if (relTile2i.X > MaxSize) {
                Logging.Log.Warning(string.Format("Too large mine area designated {0}, clamping.", relTile2i));
                maxCoord = maxCoord.SetX(minCoord.X + MaxSize);
            }
            if (relTile2i.Y > MaxSize) {
                Logging.Log.Warning(string.Format("Too large mine area designated {0}, clamping.", relTile2i));
                maxCoord = maxCoord.SetY(minCoord.Y + MaxSize);
            }
        }
    }
}