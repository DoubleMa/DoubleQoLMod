using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Prototypes;

namespace DoubleQoL.Extensions {

    internal static class VehicleExtension {

        public static Proto.ID GetProtoID(this Vehicle vehicle) => vehicle.Prototype.Id;

        public static bool IsProtoEquals(this Vehicle vehicle, Proto.ID id) => vehicle.GetProtoID().ToString().Equals(id.ToString());

        public static string GetVehicleIconPath(this Proto.ID id) => $"Assets/Unity/Generated/Icons/Vehicle/{id}.png";

        public static string GetIconPath(this Vehicle vehicle) => GetVehicleIconPath(vehicle.Prototype.Id);
    }
}