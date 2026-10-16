using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipmentApi.Data;
using ShipmentApi.Models;

namespace ShipmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController(ShipmentContext context) : ControllerBase
    {
        // GET: api/shipments?status=InTransit
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetShipments([FromQuery] ShipmentStatus? status)
        {
            var query = context.Shipments.AsNoTracking();
            if (status is not null)
            {
                query = query.Where(s => s.Status == status);
            }
            return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        }

        // GET: api/shipments/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Shipment>> GetShipment(int id)
        {
            var shipment = await context.Shipments.FindAsync(id);
            return shipment is null ? NotFound() : shipment;
        }

        // GET: api/shipments/tracking/1Z999AA10123456784
        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult<Shipment>> GetByTrackingNumber(string trackingNumber)
        {
            var shipment = await context.Shipments
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
            return shipment is null ? NotFound() : shipment;
        }

        // POST: api/shipments
        [HttpPost]
        public async Task<ActionResult<Shipment>> CreateShipment(Shipment shipment)
        {
            // Server-managed fields: ignore whatever the client sent
            shipment.Id = 0;
            shipment.CreatedAt = DateTime.UtcNow;
            shipment.UpdatedAt = shipment.CreatedAt;
            shipment.DeliveredAt = null;

            context.Shipments.Add(shipment);
            if(await context.Shipments.AnyAsync(s => s.TrackingNumber == shipment.TrackingNumber))
            {
                return Conflict($"A shipment with tracking number `{shipment.TrackingNumber}` already exists");
            }

            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShipment), new { id = shipment.Id }, shipment);
        }

        // PUT: api/shipments/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateShipment(int id, Shipment updated)
        {
            var shipment = await context.Shipments.FindAsync(id);
            if (shipment is null)
            {
                return NotFound();
            }
            
            if(await context.Shipments.AnyAsync(s => s.TrackingNumber == shipment.TrackingNumber && s.Id != id))
            {
                return Conflict($"A Shipment with tracking number `{updated.TrackingNumber}` already exists.");
            }

            shipment.Origin = updated.Origin;
            shipment.Destination = updated.Destination;
            shipment.TrackingNumber = updated.TrackingNumber;
            shipment.Carrier = updated.Carrier;
            shipment.EstimatedDeliveryDate = updated.EstimatedDeliveryDate;
            shipment.ShippingMethod = updated.ShippingMethod;
            ApplyStatus(shipment, updated.Status);

            await context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/shipments/5/status
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] ShipmentStatus status)
        {
            var shipment = await context.Shipments.FindAsync(id);
            if (shipment is null)
            {
                return NotFound();
            }

            ApplyStatus(shipment, status);

            await context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/shipments/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteShipment(int id)
        {
            var shipment = await context.Shipments.FindAsync(id);
            if (shipment is null)
            {
                return NotFound();
            }

            context.Shipments.Remove(shipment);
            await context.SaveChangesAsync();
            return NoContent();
        }

        // Keeps DeliveredAt and UpdatedAt consistent with the status
        private static void ApplyStatus(Shipment shipment, ShipmentStatus status)
        {
            if (status == ShipmentStatus.Delivered && shipment.Status != ShipmentStatus.Delivered)
            {
                shipment.DeliveredAt = DateTime.UtcNow;
            }
            else if (status != ShipmentStatus.Delivered)
            {
                shipment.DeliveredAt = null;
            }

            shipment.Status = status;
            shipment.UpdatedAt = DateTime.UtcNow;
        }
    }
}
