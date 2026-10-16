using System.ComponentModel.DataAnnotations;
namespace ShipmentApi.Models
{
    public class Shipment
    {
        public int Id { get; set; }
        [Required]
        public string Origin { get; set; } = string.Empty;
        [Required]
        public string Destination { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
        [Required]
        [MaxLength(50)]
        public string TrackingNumber { get; set; } = string.Empty;
        public string? Carrier { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? ShippingMethod { get; set; }

        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;
    }
}
