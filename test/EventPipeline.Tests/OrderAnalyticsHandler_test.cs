using System.Threading.Tasks;
using EventPipeline.Worker.Clients;
using EventPipeline.Worker.Handlers;
using EventPipeline.Worker.Models;
using Moq;
using Xunit;

namespace EventPipeline.Tests;

public class OrderAnalyticsHandler_test
{
    private static OrderSnapshot Snapshot(long id = 1) => new(id, 2, 1, 100m, 0, 0);
    private static readonly KafkaMessageContext Context = new(0, 0);

    [Fact]
    public async Task HandleAsync_CreateOp_InsertsAfter()
    {
        // Arrange
        var mockCh = new Mock<IClickHouseClient>();
        var handler = new OrderAnalyticsHandler(mockCh.Object);
        var evt = new CdcEvent<OrderSnapshot>(null, Snapshot(1), "c", 123L);

        // Act
        await handler.HandleAsync(evt, Context);

        // Assert
        mockCh.Verify(c => c.InsertAsync(Snapshot(1), "c", 123L, 0, 0), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UpdateOp_InsertsAfter()
    {
        // Arrange
        var mockCh = new Mock<IClickHouseClient>();
        var handler = new OrderAnalyticsHandler(mockCh.Object);
        var after = Snapshot(2);
        var evt = new CdcEvent<OrderSnapshot>(Snapshot(2), after, "u", 456L);

        // Act
        await handler.HandleAsync(evt, Context);

        // Assert
        mockCh.Verify(c => c.InsertAsync(after, "u", 456L, 0, 0), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DeleteOp_UsesBeforeWhenAfterIsNull()
    {
        // Arrange
        var mockCh = new Mock<IClickHouseClient>();
        var handler = new OrderAnalyticsHandler(mockCh.Object);
        var before = Snapshot(3);
        var evt = new CdcEvent<OrderSnapshot>(before, null, "d", 789L);

        // Act
        await handler.HandleAsync(evt, Context);

        // Assert
        mockCh.Verify(c => c.InsertAsync(before, "d", 789L, 0, 0), Times.Once);
    }
}
