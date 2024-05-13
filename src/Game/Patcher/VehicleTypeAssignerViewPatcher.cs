using DoubleQoL.Extensions;
using DoubleQoL.Game.UI;
using DoubleQoL.XML.config;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Core.Vehicles.Commands;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components.VehiclesAssigner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DoubleQoL.Game.Patcher {

    internal class VehicleTypeAssignerViewPatcher : APatcher<VehicleTypeAssignerViewPatcher> {
        public override bool DefaultState => true;
        public override bool Enabled => ConfigManager.Instance.QoLs_vehicle.Value;
        private static readonly Dictionary<VehicleTypeAssignerView, Panel> UpgradePanels = new Dictionary<VehicleTypeAssignerView, Panel>();
        private static DependencyResolver _resolver;
        private static InputScheduler _inputScheduler;
        private static ProtosDb _protosDb;
        private static UnlockedProtosDb _unlockedProtosDb;
        private static bool resolved = false;

        public VehicleTypeAssignerViewPatcher() : base("VehicleTypeAssignerView") {
            AddMethod(typeof(VehicleTypeAssignerView), "updateVisibility", this.GetHarmonyMethod(nameof(MyPostfix)), true);
        }

        public override void OnInit(DependencyResolver resolver) {
            _resolver = resolver;
            tryResovleDependencies();
        }

        private static bool tryResovleDependencies() {
            if (resolved) return true;
            if (_inputScheduler == null) _inputScheduler = _resolver?.GetResolvedInstance<InputScheduler>().Value;
            if (_protosDb == null) _protosDb = _resolver?.GetResolvedInstance<ProtosDb>().Value;
            if (_unlockedProtosDb == null) _unlockedProtosDb = _resolver?.GetResolvedInstance<UnlockedProtosDb>().Value;
            resolved = _inputScheduler != null && _protosDb != null && _unlockedProtosDb != null;
            return resolved;
        }

        private static void MyPostfix(VehicleTypeAssignerView __instance) {
            UiBuilder builder = __instance.GetField<UiBuilder>("m_builder");
            Panel container = __instance.GetField<Panel>("m_container");
            DynamicGroundEntityProto m_protoToAssign = __instance.GetField<DynamicGroundEntityProto>("m_protoToAssign");
            Lyst<Vehicle> m_assignedVehicles = __instance.GetField<Lyst<Vehicle>>("m_assignedVehicles");
            if (!UpgradePanels.TryGetValue(__instance, out Panel upgradePanel)) {
                upgradePanel = builder.NewPanel("UpgradePanel").PutToBottomOf(container, 32f, Offset.Top(0));
                upgradePanel.GameObject.SetActive(false);
                UpgradePanels[__instance] = upgradePanel;
            }
            foreach (Transform child in upgradePanel.RectTransform) UnityEngine.Object.Destroy(child.gameObject);
            var (upgradableVersions, equivalentVersion) = FindUpgradesAndEquivalent(m_protoToAssign.Id.ToString());
            float buttonOffset = 0f;
            Vector2 btnSize = new Vector2(32f, 32f);
            float margin = 5f;
            Regex tierRegex = new Regex(@"T(\d+)");
            foreach (var version in upgradableVersions) {
                Match tierMatch = tierRegex.Match(version.Id.ToString());
                string buttonLabel = "T" + (tierMatch.Success ? tierMatch.Groups[1].Value : "X");
                Btn btn = builder.NewBtn("UpgradeButton" + buttonLabel)
                    .SetText(buttonLabel)
                    .SetButtonStyle(builder.Style.Global.PrimaryBtn.Extend(null, null, ColorRgba.Green).ExtendText(ColorRgba.White, FontStyle.Bold, 18, true))
                    .OnClick(() => {
                        foreach (var v in m_assignedVehicles) _inputScheduler.ScheduleInputCmd(new ReplaceVehicleCmd(v.Id, version.Id));
                    })
                    .PutToLeftMiddleOf(upgradePanel, btnSize, Offset.Left(buttonOffset));
                buttonOffset += btnSize.x + margin;
            }
            if (equivalentVersion != null) {
                bool isHydrogen = equivalentVersion.Id.ToString().EndsWith("H");
                //char eqLabel = (isHydrogen ? Loc.Str("Product_Hydrogen__name", "Hydrogen", "").ToString() : Loc.Str("Product_Diesel__name", "Diesel", "").ToString())[0];
                string eqLabel = isHydrogen ? "H" : "D";
                Btn eqBtn = builder.NewBtn("EquivalentButton" + eqLabel)
                    .SetText(eqLabel)
                    .SetButtonStyle(builder.Style.Global.PrimaryBtn.Extend(null, null, isHydrogen ? ColorRgba.White : ColorRgba.Red).ExtendText(isHydrogen ? ColorRgba.Black : ColorRgba.Gold, FontStyle.Bold, 18, true))
                    .OnClick(() => {
                        foreach (var v in m_assignedVehicles) _inputScheduler.ScheduleInputCmd(new ReplaceVehicleCmd(v.Id, equivalentVersion.Id));
                    })
                    .PutToLeftMiddleOf(upgradePanel, btnSize, Offset.Left(buttonOffset));
            }
            container.OnRightClick(() => upgradePanel.GameObject.SetActive(!upgradePanel.GameObject.activeSelf));
        }

        public static (List<DrivingEntityProto> upgradableVersions, DrivingEntityProto equivalentVersion) FindUpgradesAndEquivalent(string protoName) {
            List<DrivingEntityProto> upgradableVersions = new List<DrivingEntityProto>();
            DrivingEntityProto equivalentVersion = null;
            Regex pattern = new Regex(@"(^.+?)(T\d+)(.*?)(H?)$");
            var match = pattern.Match(protoName);
            if (!match.Success || !tryResovleDependencies()) return (upgradableVersions, equivalentVersion);
            try {
                string basePrefix = match.Groups[1].Value;
                string tier = match.Groups[2].Value;
                string basePostfix = match.Groups[3].Value;
                string suffix = match.Groups[4].Value;
                int currentTierNumber = int.Parse(tier.TrimStart('T'));
                var allProtos = _protosDb.All<DrivingEntityProto>().OrderBy(p => p.UIOrder);
                foreach (var proto in allProtos) {
                    string protoId = proto.Id.ToString();
                    var otherMatch = pattern.Match(protoId);
                    if (!otherMatch.Success) continue;
                    string otherPrefix = otherMatch.Groups[1].Value;
                    int otherTierNumber = int.Parse(otherMatch.Groups[2].Value.TrimStart('T'));
                    string otherPostfix = otherMatch.Groups[3].Value;
                    string otherSuffix = otherMatch.Groups[4].Value;
                    if (otherPrefix == basePrefix && otherPostfix == basePostfix && _unlockedProtosDb.IsUnlocked(proto)) {
                        if (otherSuffix == suffix && otherTierNumber > currentTierNumber) upgradableVersions.Add(proto);
                        if (otherSuffix != suffix && otherTierNumber == currentTierNumber) equivalentVersion = proto;
                    }
                }
            }
            catch (Exception ex) {
                Logging.Log.Exception(ex, "Error at FindUpgradesAndEquivalent in VehicleTypeAssignerViewPatcher");
            }
            return (upgradableVersions, equivalentVersion);
        }
    }
}