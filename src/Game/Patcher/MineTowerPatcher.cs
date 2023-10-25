using DoubleQoL.Config;
using DoubleQoL.Extensions;
using Mafi;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DoubleQoL.Game.Patcher {

    internal class MineTowerPatcher : APatcher<MineTowerPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_minetower.Value;
        private static Type Typ;
        private static IOrderedEnumerable<LooseProductProto> _looseProductProtos;
        private static LooseProductProto _selectedLooseProductProto;

        public MineTowerPatcher() : base("MineTower") {
            Typ = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.Inspectors.Buildings.MineTowerWindowView");
            AddMethod(Typ, "AddCustomItems", this.GetHarmonyMethod("MyPostfix"), true);
        }

        public override void OnInit(DependencyResolver resolver) {
            _looseProductProtos = resolver?.GetResolvedInstance<ProtosDb>().Value?.Filter<LooseProductProto>(proto => proto.CanBeLoadedOnTruck && proto.CanBeOnTerrain).OrderBy(x => x);
            _selectedLooseProductProto = _looseProductProtos.ElementAt(0);
        }

        private static void MyPostfix(StaticEntityInspectorBase<MineTower> __instance, ref StackContainer itemContainer) {
            UiBuilder Builder = __instance.GetFieldRef<ItemDetailWindowView, UiBuilder>("Builder");
            if (Builder is null) return;
            var tabContainer = Builder.NewStackContainer("container")
            .SetStackingDirection(StackContainer.Direction.TopToBottom)
            .SetSizeMode(StackContainer.SizeMode.Dynamic)
            .SetInnerPadding(Offset.Zero);
            Builder.AddSectionTitle(tabContainer, "Prioritize resource for all exavators");
            var mineTowerPanel = Builder.NewPanel("mineTowerPanel").SetBackground(Builder.Style.Panel.ItemOverlay);
            mineTowerPanel.AppendTo(tabContainer, size: 45f, Offset.All(0));

            var mineTowerContainer = Builder
                .NewStackContainer("secondRowContainer")
                .SetStackingDirection(StackContainer.Direction.LeftToRight)
                .SetSizeMode(StackContainer.SizeMode.StaticDirectionAligned)
                .SetItemSpacing(10f)
                .PutToLeftOf(mineTowerPanel, 0.0f, Offset.Left(10f));

            var productDropdown = Builder
                .NewDropdown("ProductDropdown")
                .AddOptions(_looseProductProtos.Select(x => x.Id.ToString().Replace("Product_", "")).ToList())
                .OnValueChange(i => _selectedLooseProductProto = _looseProductProtos.ElementAt(i));

            productDropdown.AppendTo(mineTowerContainer, new Vector2(200, 28f), ContainerPosition.MiddleOrCenter);

            var prioritizeBtn = Builder.NewBtnPrimary("button")
                .SetButtonStyle(Builder.Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("prioritize"))
                .AddToolTip("prioritize the product to all the excavators")
                .OnClick(() => SetProductPriority(__instance));
            prioritizeBtn.AppendTo(mineTowerContainer, prioritizeBtn.GetOptimalSize(), ContainerPosition.MiddleOrCenter);

            itemContainer.Add(999, tabContainer, 75f);
        }

        private static void SetProductPriority(StaticEntityInspectorBase<MineTower> __instance) {
            MineTower tower = __instance.InvokeGetter<MineTower>("Entity");
            if (tower == null || _selectedLooseProductProto == null) return;
            foreach (var ex in tower.AllAssignedExcavators) ex.SetPrioritizeProduct(_selectedLooseProductProto);
        }
    }
}