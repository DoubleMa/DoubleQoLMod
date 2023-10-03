using DoubleQoL.Config;
using HarmonyLib;
using Mafi;
using Mafi.Core.Terrain.Designation;
using Mafi.Unity.Mine;
using System.Collections.Generic;
using System.Reflection;

namespace DoubleQoL.Game.Patcher {

    public class TerrainDesignationsPatcher : APatcher {
        public static readonly TerrainDesignationsPatcher Instance = new TerrainDesignationsPatcher();
        public override bool DefaultState => true;

        public override bool Enabled => ConfigManager.Instance.QoLs_terraindesignations.Value;

        protected override List<MethodInfo> MethodInfos => new List<MethodInfo> { AccessTools.Method(typeof(TerrainDesignationsManager), "GetCanonicalDesignationRange") };

        protected override HarmonyMethod MetPrefix => new HarmonyMethod(AccessTools.Method(typeof(TerrainDesignationsPatcher), "MyPrefix"));

        protected override HarmonyMethod MetPostfix => new HarmonyMethod(AccessTools.Method(typeof(TerrainDesignationsPatcher), "MyPostfix"));

        private static int MaxSize => ConfigManager.Instance.DefaultState_terraindesignations.Value;
        private static readonly int MAX_AREA_SIZE_ADD = sbyte.MaxValue;
        private static readonly int MAX_AREA_SIZE_REMOVE = 191;

        private TerrainDesignationsPatcher() : base("TerrainDesignations") {
        }

        protected override void Patch(bool enable = false) {
            if (!Enabled || IsActive == enable) return;

            AccessTools.Field(typeof(TerrainDesignationController), "MAX_AREA_SIZE_ADD")?.SetValue(null, new RelTile1i(enable ? MaxSize : MAX_AREA_SIZE_ADD));
            AccessTools.Field(typeof(TerrainDesignationController), "MAX_AREA_SIZE_REMOVE")?.SetValue(null, new RelTile1i(enable ? MaxSize : MAX_AREA_SIZE_REMOVE));
            base.Patch(enable);
        }

        public override void OnInit(object obj) {
        }

        private static bool MyPrefix() => false;

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