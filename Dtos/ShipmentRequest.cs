using System.ComponentModel.DataAnnotations;
namespace ShipmentApi.Dtos
{
    // The fields the client is allowed to send when creating or updating a shipment
    public class ShipmentRequest : IValidatableObject
    {
        [Required]
        [StringLength(200)]
        public string Origin { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 8)]
        [RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Tracking number can only contain letters and digits.")]
        public string TrackingNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Carrier { get; set; }

        public DateTime? EstimatedDeliveryDate { get; set; }

        [StringLength(50)]
        public string? ShippingMethod { get; set; }

        // Rules that involve more than one field
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.Equals(Origin?.Trim(), Destination?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult("Origin and destination cannot be the same.",
                    new[] { nameof(Origin), nameof(Destination) });
            }
        }
    }
}
