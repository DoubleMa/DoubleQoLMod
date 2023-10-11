using DoubleQoL.Extensions;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Vehicles;
using System;

namespace DoubleQoL.QoL.UI.Statusbar.Logistics {

    internal class VehicleTypeHelperParent {
        private readonly EntitiesManager EntitiesManager;
        private readonly VehiclesManager VehiclesManager;
        private readonly VehicleCountView VehicleCountView;
        private readonly Lyst<VehicleTypeHelper> childrens;
        private readonly Action<int> Update;
        public int Count { get; private set; }
        public int Idle { get; private set; }

        public VehicleTypeHelperParent(EntitiesManager entitiesManager, VehiclesManager vehiclesManager, VehicleCountView vehicleCountView) {
            EntitiesManager = entitiesManager;
            VehiclesManager = vehiclesManager;
            VehicleCountView = vehicleCountView;
            childrens = new Lyst<VehicleTypeHelper>();
            Update = (e) => UpdateView();
        }

        public void AddChild(VehicleCountView vehicleCountView) {
            VehicleTypeHelper vehicleTypeHelper = new VehicleTypeHelper(EntitiesManager, VehiclesManager, vehicleCountView);
            vehicleTypeHelper.OnCountChanged.AddNonSaveable(this, Update);
            vehicleTypeHelper.OnIdleChanged.AddNonSaveable(this, Update);
            childrens.Add(vehicleTypeHelper);
            UpdateView();
        }

        public void RemoveChild(VehicleTypeHelper vehicleTypeHelper, bool ignore = false) {
            vehicleTypeHelper.OnCountChanged.RemoveNonSaveable(this, Update);
            vehicleTypeHelper.OnIdleChanged.RemoveNonSaveable(this, Update);
            if (ignore) return;
            childrens.Remove(vehicleTypeHelper);
            UpdateView();
        }

        public void ClearAll() {
            childrens.ForEach(e => RemoveChild(e, true));
            childrens.Clear();
            UpdateView();
        }

        public void ToObserve() => childrens.ForEach(child => child.ToObserve());

        private void UpdateView() {
            int count = 0;
            int idle = 0;
            foreach (var item in childrens) {
                count += item.Count;
                idle += item.Idle;
            }
            if (count == Count && Idle == idle) return;
            Count = count;
            Idle = idle;
            VehicleCountView.SetCounts(count, idle);
        }

        public class VehicleTypeHelper {
            private readonly VehiclesManager VehiclesManager;
            private readonly VehicleCountView VehicleCountView;
            public Lyst<Vehicle> Vehicles { get; private set; }
            public int Count { get; private set; } = 0;
            public int Idle { get; private set; } = 0;

            public Event<int> OnCountChanged { get; private set; }
            public Event<int> OnIdleChanged { get; private set; }

            public VehicleTypeHelper(EntitiesManager entitiesManager, VehiclesManager vehiclesManager, VehicleCountView vehicleCountView) {
                VehicleCountView = vehicleCountView;
                VehiclesManager = vehiclesManager;
                Vehicles = new Lyst<Vehicle>();
                OnCountChanged = new Event<int>();
                OnIdleChanged = new Event<int>();
                entitiesManager.EntityAdded.AddNonSaveable(this, OnEntityAddedOrRemoved);
                entitiesManager.EntityRemoved.AddNonSaveable(this, OnEntityAddedOrRemoved);
                UpdateList();
            }

            public void ToObserve() => SetIdle(Vehicles.FindAll(v => v.IsIdle).Count);

            private void OnEntityAddedOrRemoved(IEntity entity) {
                if (entity is Vehicle vehicle && vehicle.IsProtoEquals(VehicleCountView.ProtoId)) UpdateList();
            }

            private void UpdateList() {
                Vehicles.Clear();
                Vehicles.AddRange(VehiclesManager.AllVehicles.ToLyst().FindAll(v => v.IsProtoEquals(VehicleCountView.ProtoId)));
                SetCount(Vehicles.Count);
            }

            public void SetCount(int count) {
                if (Count == count) return;
                Count = count;
                UpdateView();
                OnCountChanged.Invoke(Count);
            }

            public void SetIdle(int idle) {
                if (Idle == idle) return;
                Idle = idle;
                UpdateView();
                OnIdleChanged.Invoke(idle);
            }

            private void UpdateView() => VehicleCountView.SetCounts(Count, Idle);
        }
    }
}