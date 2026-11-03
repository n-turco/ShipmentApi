using ShipmentApi.Models;
using System.ComponentModel.DataAnnotations;

namespace ShipmentApi.Dtos
{
    public class ShipmentQuery
    {
        public ShipmentStatus? Status { get; set; }
        [Range(1, 100000)]
        public int Page { get; set; } = 1;
        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }
}
