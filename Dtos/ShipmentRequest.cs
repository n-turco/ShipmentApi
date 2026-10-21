using System.ComponentModel.DataAnnotations;
namespace ShipmentApi.Dtos
{
    // The fields the client is allowed to send when creating or updating a shipment
    public class ShipmentRequest
    {
        [Required]
        public string Origin { get; set; } = string.Empty;
        [Required]
        public string Destination { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string TrackingNumber { get; set; } = string.Empty;
        public string? Carrier { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string? ShippingMethod { get; set; }
    }
}
