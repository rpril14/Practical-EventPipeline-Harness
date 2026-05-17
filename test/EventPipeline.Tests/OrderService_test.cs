using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventPipeline.Data;
using EventPipeline.Data.Entities;
using EventPipeline.Services.Orders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventPipeline.Tests;

public class OrderService_test
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task CreateAsync_ValidRequest_ComputesTotalAmount()
    {
        // Arrange
        await using var ctx = CreateContext();
        var service = new OrderService(ctx);
        var request = new CreateOrderRequest(CustomerId: 1, Items: new List<OrderItemRequest>
        {
            new(ProductId: 10, Quantity: 2, Price: 15.00m),
            new(ProductId: 11, Quantity: 1, Price: 30.00m)
        });

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.Equal(60.00m, result.TotalAmount);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SetsStatusToPending()
    {
        // Arrange
        await using var ctx = CreateContext();
        var service = new OrderService(ctx);
        var request = new CreateOrderRequest(CustomerId: 1, Items: new List<OrderItemRequest>
        {
            new(ProductId: 10, Quantity: 1, Price: 10m)
        });

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.Equal(OrderStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsItems()
    {
        // Arrange
        await using var ctx = CreateContext();
        var service = new OrderService(ctx);
        var request = new CreateOrderRequest(CustomerId: 1, Items: new List<OrderItemRequest>
        {
            new(ProductId: 10, Quantity: 3, Price: 5m),
            new(ProductId: 20, Quantity: 1, Price: 50m)
        });

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, i => i.ProductId == 10 && i.Quantity == 3);
        Assert.Contains(result.Items, i => i.ProductId == 20 && i.Price == 50m);
    }

    [Fact]
    public async Task UpdateStatusAsync_ExistingOrder_UpdatesStatus()
    {
        // Arrange
        await using var ctx = CreateContext();
        var service = new OrderService(ctx);
        var created = await service.CreateAsync(new CreateOrderRequest(1, new List<OrderItemRequest>
        {
            new(10, 1, 10m)
        }));

        // Act
        var result = await service.UpdateStatusAsync(created.Id, new UpdateOrderStatusRequest(OrderStatus.Shipped));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Shipped, result.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_ExistingOrder_UpdatesUpdatedAt()
    {
        // Arrange
        await using var ctx = CreateContext();
        var service = new OrderService(ctx);
        var created = await service.CreateAsync(new CreateOrderRequest(1, new List<OrderItemRequest>
        {
            new(10, 1, 10m)
        }));
        var before = created.UpdatedAt;

        // Act
        await Task.Delay(10);
        var result = await service.UpdateStatusAsync(created.Id, new UpdateOrderStatusRequest(OrderStatus.Processing));

        // Assert
        Assert.True(result!.UpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExistentOrder_ReturnsNull()
    {
        // Arrange
        await using var ctx = CreateContext();
        var service = new OrderService(ctx);

        // Act
        var result = await service.UpdateStatusAsync(999, new UpdateOrderStatusRequest(OrderStatus.Shipped));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_ExistingOrder_ReturnsOrderWithItems()
    {
        // Arrange
        await using var ctx = CreateContext();
        var service = new OrderService(ctx);
        var created = await service.CreateAsync(new CreateOrderRequest(5, new List<OrderItemRequest>
        {
            new(10, 2, 25m)
        }));

        // Act
        var result = await service.GetAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.CustomerId);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetAsync_NonExistentOrder_ReturnsNull()
    {
        // Arrange
        await using var ctx = CreateContext();
        var service = new OrderService(ctx);

        // Act
        var result = await service.GetAsync(999);

        // Assert
        Assert.Null(result);
    }
}
