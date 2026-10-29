using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipmentApi.Data;
using ShipmentApi.Models;
using ShipmentApi.Dtos;
using Microsoft.Data.SqlClient;

namespace ShipmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController(ShipmentContext context, ILogger<ShipmentsController> logger) : ControllerBase
    {
        // GET: api/shipments?status=InTransit
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShipmentResponse>>> GetShipments([FromQuery] ShipmentStatus? status)
        {
            var query = context.Shipments.AsNoTracking();
            if (status is not null)
            {
                query = query.Where(s => s.Status == status);
            }
            return await query.OrderByDescending(s => s.CreatedAt).Select(s => ShipmentResponse.FromEntity(s)).ToListAsync();
        }

        // GET: api/shipments/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShipmentResponse>> GetShipment(int id)
        {
            var shipment = await context.Shipments.FindAsync(id);
            return shipment is null ? NotFound() : ShipmentResponse.FromEntity(shipment);
        }

        // GET: api/shipments/tracking/1Z999AA10123456784
        [HttpGet("tracking/{trackingNumber}")]
        public async Task<ActionResult<ShipmentResponse>> GetByTrackingNumber(string trackingNumber)
        {
            var shipment = await context.Shipments
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
            return shipment is null ? NotFound() : ShipmentResponse.FromEntity(shipment);
        }

        // POST: api/shipments
        [HttpPost]
        public async Task<ActionResult<ShipmentResponse>> CreateShipment(ShipmentRequest request)
        {
            if (request.EstimatedDeliveryDate is not null && request.EstimatedDeliveryDate.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(nameof(request.EstimatedDeliveryDate), "Estimated delivery date cannot be in the past.");
                return ValidationProblem(ModelState);
            }

            // Check if the tracking number on the new shipment is already being used by another shipment
            if (await context.Shipments.AnyAsync(s => s.TrackingNumber == request.TrackingNumber))
            {
                return Conflict($"A Shipment with tracking number '{request.TrackingNumber}' already exists.");
            }

            var shipment = new Shipment
            {
                Origin = request.Origin,
                Destination = request.Destination,
                TrackingNumber = request.TrackingNumber,
                Carrier = request.Carrier,
                EstimatedDeliveryDate = request.EstimatedDeliveryDate,
                ShippingMethod = request.ShippingMethod
            };

            context.Shipments.Add(shipment);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                logger.LogWarning(ex, "Duplicate tracking number {TrackingNumber} rejected by the database.", request.TrackingNumber);
                // Another request saved the same tracking number after our AnyAsync check
                return Conflict($"A Shipment with tracking number '{request.TrackingNumber}' already exists.");
            }

            return CreatedAtAction(nameof(GetShipment), new { id = shipment.Id }, ShipmentResponse.FromEntity(shipment));
        }

        // PUT: api/shipments/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateShipment(int id, ShipmentRequest request)
        {
            // Search the database for matching shipment ID, return not found if no match is found
            var shipment = await context.Shipments.FindAsync(id);
            if (shipment is null)
            {
                return NotFound();
            }

            // Return 409 Conflict if another shipment already uses the new tracking number
            if (await context.Shipments.AnyAsync(s => s.TrackingNumber == request.TrackingNumber && s.Id != id))
            {
                return Conflict($"A Shipment with tracking number '{request.TrackingNumber}' already exists.");
            }

            shipment.Origin = request.Origin;
            shipment.Destination = request.Destination;
            shipment.TrackingNumber = request.TrackingNumber;
            shipment.Carrier = request.Carrier;
            shipment.EstimatedDeliveryDate = request.EstimatedDeliveryDate;
            shipment.ShippingMethod = request.ShippingMethod;
            shipment.UpdatedAt = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                logger.LogWarning(ex, "Duplicate tracking number {TrackingNumber} rejected by the database.", request.TrackingNumber);
                // Another request saved the same tracking number after our AnyAsync check
                return Conflict($"A Shipment with tracking number '{request.TrackingNumber}' already exists.");
            }

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

            // Same status: nothing to change, and UpdatedAt stays as it is
            if (shipment.Status == status)
            {
                return NoContent();
            }

            // Return 409 Conflict if the workflow doesn't allow this change
            if (!ShipmentStatusTransitions.CanTransition(shipment.Status, status))
            {
                return Conflict($"Cannot change status from '{shipment.Status}' to '{status}'.");
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

        private static bool IsUniqueViolation(DbUpdateException ex) =>
            ex.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
