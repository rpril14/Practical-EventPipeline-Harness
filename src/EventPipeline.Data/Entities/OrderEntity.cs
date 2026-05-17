using System;

namespace EventPipeline.Data.Entities;

public class OrderEntity
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
