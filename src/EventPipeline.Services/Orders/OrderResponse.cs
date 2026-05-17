using System;
using System.Collections.Generic;
using EventPipeline.Data.Entities;

namespace EventPipeline.Services.Orders;

public record OrderResponse(
    long Id,
    long CustomerId,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderItemResponse> Items);

public record OrderItemResponse(
    long Id,
    long OrderId,
    long ProductId,
    int Quantity,
    decimal Price);
