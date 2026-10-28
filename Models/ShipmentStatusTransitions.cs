namespace ShipmentApi.Models
{
    public class ShipmentStatusTransitions
    {
        private static readonly Dictionary<ShipmentStatus, ShipmentStatus[]> Allowed = new()
        {
            [ShipmentStatus.Created] = [ShipmentStatus.Pending, ShipmentStatus.Cancelled],
            [ShipmentStatus.Pending] = [ShipmentStatus.InTransit, ShipmentStatus.Cancelled],
            [ShipmentStatus.InTransit] = [ShipmentStatus.Delivered, ShipmentStatus.FailedDelivery],
            [ShipmentStatus.FailedDelivery] = [ShipmentStatus.InTransit, ShipmentStatus.Cancelled],
            [ShipmentStatus.Delivered] = [],
            [ShipmentStatus.Cancelled] = [],
        };

        public static bool CanTransition(ShipmentStatus from, ShipmentStatus to) =>
            Allowed.TryGetValue(from, out var next) && next.Contains(to);
    }
}
