using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventPipeline.Data;
using EventPipeline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventPipeline.Services.Orders;

public class OrderService(AppDbContext db) : IOrderService
{
    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request)
    {
        var totalAmount = request.Items.Sum(i => i.Quantity * i.Price);
        var now = DateTime.UtcNow;

        var order = new OrderEntity
        {
            CustomerId = request.CustomerId,
            Status = OrderStatus.Pending,
            TotalAmount = totalAmount,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var items = request.Items.Select(i => new OrderItemEntity
        {
            OrderId = order.Id,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList();
        db.OrderItems.AddRange(items);
        await db.SaveChangesAsync();

        return Map(order, items);
    }

    public async Task<OrderResponse?> UpdateStatusAsync(long id, UpdateOrderStatusRequest request)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return null;

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var items = await db.OrderItems.Where(i => i.OrderId == id).ToListAsync();
        return Map(order, items);
    }

    public async Task<OrderResponse?> GetAsync(long id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return null;

        var items = await db.OrderItems.Where(i => i.OrderId == id).ToListAsync();
        return Map(order, items);
    }

    private static OrderResponse Map(OrderEntity order, List<OrderItemEntity> items) =>
        new(order.Id, order.CustomerId, order.Status, order.TotalAmount,
            order.CreatedAt, order.UpdatedAt,
            items.Select(i => new OrderItemResponse(i.Id, i.OrderId, i.ProductId, i.Quantity, i.Price)).ToList());
}
