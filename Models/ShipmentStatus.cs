namespace ShipmentApi.Models;

public enum ShipmentStatus
{
    Created,
    Pending,
    InTransit,
    Delivered,
    Cancelled,
    FailedDelivery
}