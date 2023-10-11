using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Global;
using DoubleQoL.QoL.UI.Statusbar.Component;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Core.Vehicles;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using UnityEngine;

namespace DoubleQoL.QoL.UI.Statusbar.Logistics {

    internal class VehicleDetailedStatsView : IFixedSizeUiElement {
        private readonly StackContainer _container;
        public GameObject GameObject => _container.GameObject;
        public RectTransform RectTransform => _container.RectTransform;
        public Vector2 Size { get; private set; } = Static.InfoTileSize.Extend(y: 0.5f);
        private readonly UiBuilder _builder;
        private Proto.ID[] _protoIds;
        private readonly string _icon;
        private readonly VehiclesManager _vehiclesManager;
        private readonly EntitiesManager _entitiesManager;
        private readonly StackContainer _detailsContainer;
        private VehicleTypeHelperParent ParentHelper;
        private VehicleCountView MainView;

        public Vector2 SizeExtended() => Size.Extend(x: _protoIds.Length + 1);

        private readonly Dict<string, Proto.ID> StringOfIds = new Dict<string, Proto.ID> {
            {"TruckT1",   Ids.Vehicles.TruckT1.Id},
            {"TruckT2",   Ids.Vehicles.TruckT2.Id},
            {"TruckT3Loose",   Ids.Vehicles.TruckT3Loose.Id},
            {"TruckT3Fluid",   Ids.Vehicles.TruckT3Fluid.Id},
            {"ExcavatorT1",   Ids.Vehicles.ExcavatorT1},
            {"ExcavatorT2",   Ids.Vehicles.ExcavatorT2},
            {"ExcavatorT3",   Ids.Vehicles.ExcavatorT3},
            {"TreeHarvesterT1",   Ids.Vehicles.TreeHarvesterT1},
            {"TreeHarvesterT2",   Ids.Vehicles.TreeHarvesterT2},
            {"TreePlanterT1",   Ids.Vehicles.TreePlanterT1},
        };

        public VehicleDetailedStatsView(EntitiesManager entitiesManager, VehiclesManager vehiclesManager, UiBuilder builder, string icon, params Proto.ID[] protoIds) : base() {
            _container = builder.NewStackContainer(nameof(VehicleDetailedStatsView)).SetStackingDirection(StackContainer.Direction.LeftToRight);
            _detailsContainer = builder.NewStackContainer("detailsContainer").SetStackingDirection(StackContainer.Direction.LeftToRight).SetSizeMode(StackContainer.SizeMode.Dynamic);
            _builder = builder;
            _protoIds = protoIds ?? new Proto.ID[] { };
            _icon = icon;
            _entitiesManager = entitiesManager;
            _vehiclesManager = vehiclesManager;
        }

        public VehicleDetailedStatsView SetProtoIds(XKeyWithComment<string> xKey) {
            Lyst<Proto.ID> truckIds = new Lyst<Proto.ID>();
            foreach (var item in xKey.ConvertToStringArray()) if (StringOfIds.ContainsKey(item)) truckIds.Add(StringOfIds[item]);
            _protoIds = truckIds.ToArray();
            return this;
        }

        public VehicleDetailedStatsView Build(Vector2? vector = null) {
            Size = vector ?? Size;
            MainView = new VehicleCountView(_builder, _icon).Build(Size)
                .OnClick(() => { MainView.IgnoreHover = !MainView.IgnoreHover; })
                .SetOnMouseEnterLeaveActions(() => _detailsContainer.SetVisibility(true), () => _detailsContainer.SetVisibility(false))
                .AppendTo(_container, Size, ContainerPosition.LeftOrTop, Offset.Zero);
            ParentHelper = new VehicleTypeHelperParent(_entitiesManager, _vehiclesManager, MainView);
            BuildDetailsContainer();
            _detailsContainer.AppendTo(_container, Size.Extend(x: _protoIds.Length), ContainerPosition.LeftOrTop, Offset.Zero);
            _detailsContainer.SetVisibility(false);
            return this;
        }

        private void BuildDetailsContainer() {
            _detailsContainer.ClearAll();
            _detailsContainer.StartBatchOperation();
            ParentHelper.ClearAll();
            foreach (var id in _protoIds) ParentHelper.AddChild(new VehicleCountView(_builder, id).Build(Size).AppendTo(_detailsContainer, Size, ContainerPosition.LeftOrTop, Offset.Zero));
            _detailsContainer.FinishBatchOperation();
        }

        public void ToObserve() => ParentHelper.ToObserve();
    }
}