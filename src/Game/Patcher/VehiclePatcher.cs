using DoubleQoL.Config;
using HarmonyLib;
using Mafi.Core.Entities.Dynamic;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using Mafi.Unity.UserInterface.Style;
using System;

namespace DoubleQoL.Game.Patcher {

    internal class VehiclePatcher : APatcher {
        public static readonly VehiclePatcher Instance = new VehiclePatcher();
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_vehicle.Value;

        private VehiclePatcher() : base("Vehicle") {
            AddAllowedMethod(AccessTools.Method(typeof(ItemDetailWindowView), "AddRecoverVehicleBtn"), AccessTools.Method(GetType(), "MyPostfix"));
        }

        private static void MyPostfix(ItemDetailWindowView __instance, ref StackContainer container, ref Func<Vehicle> vehicleProvider) {
            AccessTools.FieldRef<ItemDetailWindowView, UiBuilder> builder = AccessTools.FieldRefAccess<ItemDetailWindowView, UiBuilder>("Builder");
            AccessTools.FieldRef<ItemDetailWindowView, UiStyle> style = AccessTools.FieldRefAccess<ItemDetailWindowView, UiStyle>("Style");
            UiBuilder Builder = builder(__instance);
            UiStyle Style = style(__instance);

            Func<Vehicle> func = vehicleProvider;
            Btn recoverBtn = Builder.NewBtnDanger("CancelJobs")
                .SetIcon(Assets.Unity.UserInterface.General.Cancel_svg, new Offset?(Offset.All(5f)))
                .AddToolTip("Cancel All Jobs")
                .OnClick(() => func()?.CancelAllJobsAndResetState())
                .AppendTo(container, 40f);
        }
    }
}