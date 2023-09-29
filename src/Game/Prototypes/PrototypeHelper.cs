using DoubleQoL.Config;
using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Buildings.Forestry;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Localization;

namespace DoubleQoL.Game.Prototypes {

    public class PrototypeHelper {
        public static readonly PrototypeHelper Instance = new PrototypeHelper();
        private ProtoRegistrator registrator;

        private PrototypeHelper() {
        }

        public void Init(ProtoRegistrator registrator) {
            this.registrator = registrator;
            if (ConfigManager.Instance.QoLs_towerarea.Value) {
                TryChangeMineTowerArea();
                TryChangeForestryTowerArea();
            }
        }

        private void TryChangeMineTowerArea() {
            try {
                StaticEntityProto.ID mineTowerID = Ids.Buildings.MineTower;
                registrator.PrototypesDb.RemoveOrThrow(mineTowerID);
                registrator.MineTowerProtoBuilder
                    .Start("Mine control tower+", mineTowerID)
                    .Description("Enables assignment of excavators and trucks to designated mine areas. Only designated mining areas within the influence of the tower can be mined.")
                    .SetCost(Costs.Buildings.MineTower)
                    .ShowTerrainDesignatorsOnCreation()
                    .SetLayout("(3)(3)(8)(8)", "(3)(8)(9)(9)", "(3)(8)(9)(9)", "(3)(3)(8)(8)")
                    .SetMineArea(new MineTowerProto.MineArea(new RelTile2i(5, 2), new RelTile2i(60, 60), new RelTile1i(ConfigManager.Instance.DefaultState_towerarea.Value * 128)))
                    .SetCategories(Ids.ToolbarCategories.BuildingsForVehicles)
                    .SetPrefabPath("Assets/Base/Buildings/MineTower.prefab")
                    .BuildAndAdd()
                    .AddParam(new DrawArrowWileBuildingProtoParam(4f));
            }
            catch { }
        }

        private void TryChangeForestryTowerArea() {
            try {
                StaticEntityProto.ID forestryTowerID = Ids.Buildings.ForestryTower;
                registrator.PrototypesDb.RemoveOrThrow(forestryTowerID);
                registrator.PrototypesDb
                    .Add(new ForestryTowerProto(Ids.Buildings.ForestryTower, Proto.CreateStr((Proto.ID)Ids.Buildings.ForestryTower, "Forestry control tower+",
                    Loc.Str(Ids.Buildings.ForestryTower.Value + "__desc",
                        "Enables assignment of tree planters and tree harvesters to designated forestry areas. Only designated forestry areas within the influence of the tower can be used.",
                        "Description of forestry tower.")),
                    registrator.LayoutParser.ParseLayoutOrThrow("(9)(9)(9)(9)", "(9)(9)(9)(9)", "(9)(9)(9)(9)", "(9)(9)(9)(9)"),
                    Costs.Buildings.ForestryTower.MapToEntityCosts(registrator),
                    new ForestryTowerProto.ForestryArea(new RelTile2i(5, 2),
                    new RelTile2i(60, 60), new RelTile1i(ConfigManager.Instance.DefaultState_towerarea.Value * 128)),
                    new LayoutEntityProto.Gfx("Assets/Base/Buildings/ForestryTower.prefab",
                    categories: new ImmutableArray<ToolbarCategoryProto>?(registrator.GetCategoriesProtos(Ids.ToolbarCategories.BuildingsForVehicles)), useInstancedRendering: true)))
                    .AddParam(new DrawArrowWileBuildingProtoParam(4f));
            }
            catch { }
        }
    }
}