using System.Collections.Generic;
using EventPipeline.Data.Entities;

namespace EventPipeline.Services.Orders;

public record CreateOrderRequest(long CustomerId, IReadOnlyList<OrderItemRequest> Items);
public record OrderItemRequest(long ProductId, int Quantity, decimal Price);
public record UpdateOrderStatusRequest(OrderStatus Status);
