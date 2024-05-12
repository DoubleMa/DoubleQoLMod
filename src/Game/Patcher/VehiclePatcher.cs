using DoubleQoL.Extensions;
using DoubleQoL.XML.config;
using DoubleQoL.XML.lang;
using Mafi.Core.Entities.Dynamic;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;

namespace DoubleQoL.Game.Patcher {

    internal class VehiclePatcher : APatcher<VehiclePatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_vehicle.Value;

        public VehiclePatcher() : base("Vehicle") {
            AddMethod<ItemDetailWindowView>("AddRecoverVehicleBtn", this.GetHarmonyMethod(nameof(MyPostfix)), true);
        }

        private static void MyPostfix(ItemDetailWindowView __instance, ref StackContainer container, ref Func<Vehicle> vehicleProvider) {
            Func<Vehicle> func = vehicleProvider;
            Btn recoverBtn = __instance.GetFieldRef<ItemDetailWindowView, UiBuilder>("Builder").NewBtnDanger("CancelJobs")
                .SetIcon(Assets.Unity.UserInterface.General.Cancel_svg, new Offset?(Offset.All(5f)))
                .AddToolTip(LanguageManager.Instance.tr_cancel_jobs.Value)
                .OnClick(() => func()?.CancelAllJobsAndResetState())
                .AppendTo(container, 40f);
        }
    }
}