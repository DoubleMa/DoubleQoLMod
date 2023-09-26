using DoubleQoL.Config;
using HarmonyLib;
using Mafi;
using Mafi.Core;
using Mafi.Core.Buildings.Mine;
using Mafi.Core.Input;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Syncers;
using Mafi.Core.Vehicles.Commands;
using Mafi.Core.Vehicles;
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Xml.Linq;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities;

namespace DoubleQoL.Game.Patcher
{
    public class VehiclePatcher
    {
        public static readonly VehiclePatcher Instance = new VehiclePatcher();

        public const string VehiclePatcherCategory = "VehiclePatcherCategory";
        public const string VehiclePatcherID = "com.vehicle.patch";

        public bool isActive { get; private set; } = false;

        private Harmony harmony;

        private static Type tempType = typeof(ItemDetailWindowView);

        private List<MethodInfo> methodInfos;
        private HarmonyMethod mPrefix;
        private HarmonyMethod mPostfix;

        private VehiclePatcher()
        { }

        public void Init()
        {
            harmony = new Harmony(VehiclePatcherID);

            methodInfos = new List<MethodInfo>
            {
                AccessTools.Method(tempType, "AddRecoverVehicleBtn")
            };
            mPrefix = new HarmonyMethod(AccessTools.Method(typeof(VehiclePatcher), "MyPrefix"));
            mPostfix = new HarmonyMethod(AccessTools.Method(typeof(VehiclePatcher), "MyPostfix"));
            Patch(true);
        }

        public void Toggle() => Patch(!isActive);

        public void Activate() => Patch(true);

        public void Disable() => Patch(false);

        private void Patch(bool enable = false)
        {
            if (isActive == enable || !ConfigManager.Instance.QoLs_vehicle.getBoolValue()) return;
            foreach (var m in methodInfos)
            {
                harmony.Unpatch(m, HarmonyPatchType.All, VehiclePatcherID);
                if (enable) harmony.Patch(m, mPrefix, mPostfix);
            }
            isActive = enable;
        }

        private static bool MyPrefix() => true;

        private static void MyPostfix(ItemDetailWindowView __instance, ref StackContainer container, ref Func<Vehicle> vehicleProvider)
        {
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