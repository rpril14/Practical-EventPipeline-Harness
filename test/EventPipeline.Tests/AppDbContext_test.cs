using System;
using System.Threading.Tasks;
using EventPipeline.Data;
using EventPipeline.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventPipeline.Tests;

public class AppDbContext_test
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task SaveOrder_RoundTrip_PreservesAllFields()
    {
        // Arrange
        await using var ctx = CreateContext();
        var order = new OrderEntity
        {
            CustomerId = 42,
            Status = OrderStatus.Processing,
            TotalAmount = 99.99m,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();
        var loaded = await ctx.Orders.FindAsync(order.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(42, loaded.CustomerId);
        Assert.Equal(OrderStatus.Processing, loaded.Status);
        Assert.Equal(99.99m, loaded.TotalAmount);
    }

    [Fact]
    public async Task SaveOrder_DateTimeKind_IsUtcAfterRoundTrip()
    {
        // Arrange
        await using var ctx = CreateContext();
        var order = new OrderEntity
        {
            CustomerId = 1,
            Status = OrderStatus.Pending,
            TotalAmount = 1m,
            CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();
        var loaded = await ctx.Orders.FindAsync(order.Id);

        // Assert
        Assert.Equal(DateTimeKind.Utc, loaded!.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, loaded.UpdatedAt.Kind);
    }

    [Fact]
    public async Task SaveOrderItem_RoundTrip_PreservesAllFields()
    {
        // Arrange
        await using var ctx = CreateContext();
        var order = new OrderEntity
        {
            CustomerId = 1, Status = OrderStatus.Pending,
            TotalAmount = 20m,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();

        var item = new OrderItemEntity { OrderId = order.Id, ProductId = 99, Quantity = 2, Price = 10m };

        // Act
        ctx.OrderItems.Add(item);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();
        var loaded = await ctx.OrderItems.FindAsync(item.Id);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(order.Id, loaded.OrderId);
        Assert.Equal(99, loaded.ProductId);
        Assert.Equal(2, loaded.Quantity);
        Assert.Equal(10m, loaded.Price);
    }
}
