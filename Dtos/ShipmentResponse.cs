using ShipmentApi.Models;

namespace ShipmentApi.Dtos
{
    public class ShipmentResponse
    {
        public int Id { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string? Carrier { get; set; }
        public string? ShippingMethod { get; set; }
        public ShipmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // Copies an entity into a response DTO
        public static ShipmentResponse FromEntity(Shipment s) => new()
        {
            Id = s.Id,
            Origin = s.Origin,
            Destination = s.Destination,
            TrackingNumber = s.TrackingNumber,
            Carrier = s.Carrier,
            ShippingMethod = s.ShippingMethod,
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            EstimatedDeliveryDate = s.EstimatedDeliveryDate,
            DeliveredAt = s.DeliveredAt
        };
    }
}
