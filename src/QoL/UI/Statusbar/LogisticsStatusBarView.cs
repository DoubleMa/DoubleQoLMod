using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Game.Patcher;
using DoubleQoL.QoL.UI.Statusbar.Component;
using DoubleQoL.QoL.UI.Statusbar.Logistics;
using Mafi;
using Mafi.Base;
using Mafi.Core.Entities;
using Mafi.Core.GameLoop;
using Mafi.Core.Syncers;
using Mafi.Core.Vehicles;
using Mafi.Unity;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UserInterface;

namespace DoubleQoL.QoL.UI.Statusbar {

    [GlobalDependency(RegistrationMode.AsEverything)]
    internal class LogisticsStatusBarView : AStatusBarView {
        protected override float Order => 1f;
        protected override SyncFrequency SyncFreq => SyncFrequency.OncePerSec;
        protected override bool IsEnabled => LogisticsStatusBarPatcher.Instance?.Enabled ?? false;

        private readonly VehiclesManager _vehiclesManager;
        private readonly EntitiesManager _entitiesManager;
        private readonly VehicleBuffersRegistry _vehicleBuffersRegistry;

        public LogisticsStatusBarView(IGameLoopEvents gameLoop, StatusBar statusBar, EntitiesManager entitiesManager, VehiclesManager vehiclesManager, VehicleBuffersRegistry vehicleBuffersRegistry) : base(gameLoop, statusBar) {
            _vehiclesManager = vehiclesManager;
            _entitiesManager = entitiesManager;
            _vehicleBuffersRegistry = vehicleBuffersRegistry;
        }

        /// <summary>
        /// This method is adapted from code provided in <see cref="Mafi.Unity.InputControl.Levelling.LogisticsStatusBarView"/>.
        /// </summary>
        protected override void OnRegisteringUi(UiBuilder builder, UpdaterBuilder updaterBuilder) {
            VehicleDetailedStatsView detailedStatsView = new VehicleDetailedStatsView(_entitiesManager, _vehiclesManager, builder, VehicleExtension.GetVehicleIconPath(Ids.Vehicles.TruckT2.Id))
                .SetProtoIds(ConfigManager.Instance.DefaultState_trucktoshow)
                .Build();
            VehicleDetailedStatsView detailedStatsView2 = new VehicleDetailedStatsView(_entitiesManager, _vehiclesManager, builder, VehicleExtension.GetVehicleIconPath(Ids.Vehicles.ExcavatorT3))
                .SetProtoIds(ConfigManager.Instance.DefaultState_nontrucktoshow)
                .Build();
            InfoTileExp = new InfoTileExpended(builder, "Assets/Unity/UserInterface/Toolbar/Vehicles.svg")
               .Build()
               .AddTooltip(Tr.LogisticsStatus__Tooltip.AsFormatted.Value)
               .MakeAsSingleText()
               .Append(detailedStatsView, detailedStatsView.SizeExtended())
               .Append(detailedStatsView2, detailedStatsView2.SizeExtended());

            updaterBuilder.DoOnSyncPeriodically(() => { detailedStatsView.ToObserve(); detailedStatsView2.ToObserve(); }, Duration.FromTicks(2));
            updaterBuilder.Observe(() => _vehicleBuffersRegistry.GetBalancingLatency()).Do(latency => {
                if (latency > Duration.FromSec(400)) InfoTileExp.PopInfoTile.SetCriticalColor().SetText(Tr.LogisticsStatus__ExtremelyBusy);
                else if (latency > Duration.FromSec(140)) InfoTileExp.PopInfoTile.SetText(Tr.LogisticsStatus__VeryBusy).SetCriticalColor();
                else if (latency > Duration.FromSec(40)) InfoTileExp.PopInfoTile.SetText(Tr.LogisticsStatus__Busy).SetWarningColor();
                else InfoTileExp.PopInfoTile.SetText(Tr.LogisticsStatus__Stable).SetStandardColor();
            });
        }
    }
}