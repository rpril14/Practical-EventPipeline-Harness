namespace EventPipeline.Data.Entities;

public enum OrderStatus
{
    Pending = 1,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
