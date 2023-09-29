using DoubleQoL.Config;
using HarmonyLib;
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
using Mafi.Unity.UserInterface.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DoubleQoL.Game.Patcher {

    public class MineTowerPatcher : APatcher {
        public static readonly MineTowerPatcher Instance = new MineTowerPatcher();

        private static readonly Type TempType = Assembly.Load("Mafi.Unity").GetType("Mafi.Unity.InputControl.Inspectors.Buildings.MineTowerWindowView");

        public const string MineTowerPatcherCategory = "MineTowerPatcherCategory";
        public const string MineTowerPatcherID = "com.minetower.patch";

        public override bool DefaultState => true;

        public override bool Enabled => ConfigManager.Instance.QoLs_minetower.Value;

        protected override List<MethodInfo> MethodInfos => new List<MethodInfo> { AccessTools.Method(TempType, "AddCustomItems") };

        protected override HarmonyMethod MetPrefix => new HarmonyMethod(AccessTools.Method(typeof(MineTowerPatcher), "MyPrefix"));

        protected override HarmonyMethod MetPostfix => new HarmonyMethod(AccessTools.Method(typeof(MineTowerPatcher), "MyPostfix"));

        private static IOrderedEnumerable<LooseProductProto> _looseProductProtos;
        private static LooseProductProto _selectedLooseProductProto;
        private static ProtosDb _protosDb;

        private MineTowerPatcher() : base("MineTower") {
        }

        public override void OnInit(object obj) {
            if (!(obj is ProtosDb)) throw new ArgumentException("MineTowerPatcher need a ProtosDb argument");
            ProtosDb protosDb = (ProtosDb)obj;
            if (_protosDb != null) return;
            _protosDb = protosDb;
            _looseProductProtos = protosDb.Filter<LooseProductProto>(proto => proto.CanBeLoadedOnTruck && proto.CanBeOnTerrain).OrderBy(x => x);
            _selectedLooseProductProto = _looseProductProtos.ElementAt(0);
        }

        private static bool MyPrefix() => true;

        private static void MyPostfix(StaticEntityInspectorBase<MineTower> __instance, ref StackContainer itemContainer) {
            AccessTools.FieldRef<StaticEntityInspectorBase<MineTower>, UiBuilder> builder = AccessTools.FieldRefAccess<StaticEntityInspectorBase<MineTower>, UiBuilder>("Builder");
            AccessTools.FieldRef<StaticEntityInspectorBase<MineTower>, UiStyle> style = AccessTools.FieldRefAccess<StaticEntityInspectorBase<MineTower>, UiStyle>("Style");

            UiBuilder Builder = builder(__instance);
            UiStyle Style = style(__instance);
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
                .SetButtonStyle(Style.Global.PrimaryBtn)
                .SetText(new LocStrFormatted("prioritize"))
                .AddToolTip("prioritize the product to all the excavators")
                .OnClick(() => SetProductPriority(__instance));
            prioritizeBtn.AppendTo(mineTowerContainer, prioritizeBtn.GetOptimalSize(), ContainerPosition.MiddleOrCenter);

            itemContainer.Add(999, tabContainer, 75f);
        }

        private static void SetProductPriority(StaticEntityInspectorBase<MineTower> __instance) {
            MineTower tower = (MineTower)AccessTools.PropertyGetter(TempType, "Entity").Invoke(__instance, null);
            if (tower == null || _selectedLooseProductProto == null) return;
            foreach (var ex in tower.AllAssignedExcavators) ex.SetPrioritizeProduct(_selectedLooseProductProto);
        }
    }
}