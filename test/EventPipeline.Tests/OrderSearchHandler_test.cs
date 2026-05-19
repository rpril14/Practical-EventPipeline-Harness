using System.Threading.Tasks;
using EventPipeline.Worker.Clients;
using EventPipeline.Worker.Handlers;
using EventPipeline.Worker.Models;
using Moq;
using Xunit;

namespace EventPipeline.Tests;

public class OrderSearchHandler_test
{
    private static OrderSnapshot Snapshot(long id = 1) => new(id, 2, 1, 100m, 0, 0);
    private static readonly KafkaMessageContext Context = new(0, 0);

    [Fact]
    public async Task HandleAsync_CreateOp_CallsUpsert()
    {
        // Arrange
        var mockEs = new Mock<IElasticsearchClient>();
        var handler = new OrderSearchHandler(mockEs.Object);
        var evt = new CdcEvent<OrderSnapshot>(null, Snapshot(1), "c", 0);

        // Act
        await handler.HandleAsync(evt, Context);

        // Assert
        mockEs.Verify(e => e.UpsertAsync(1, Snapshot(1)), Times.Once);
        mockEs.Verify(e => e.DeleteAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UpdateOp_CallsUpsert()
    {
        // Arrange
        var mockEs = new Mock<IElasticsearchClient>();
        var handler = new OrderSearchHandler(mockEs.Object);
        var evt = new CdcEvent<OrderSnapshot>(Snapshot(2), Snapshot(2), "u", 0);

        // Act
        await handler.HandleAsync(evt, Context);

        // Assert
        mockEs.Verify(e => e.UpsertAsync(2, Snapshot(2)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SnapshotOp_CallsUpsert()
    {
        // Arrange
        var mockEs = new Mock<IElasticsearchClient>();
        var handler = new OrderSearchHandler(mockEs.Object);
        var evt = new CdcEvent<OrderSnapshot>(null, Snapshot(3), "r", 0);

        // Act
        await handler.HandleAsync(evt, Context);

        // Assert
        mockEs.Verify(e => e.UpsertAsync(3, Snapshot(3)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DeleteOp_CallsDelete()
    {
        // Arrange
        var mockEs = new Mock<IElasticsearchClient>();
        var handler = new OrderSearchHandler(mockEs.Object);
        var evt = new CdcEvent<OrderSnapshot>(Snapshot(4), null, "d", 0);

        // Act
        await handler.HandleAsync(evt, Context);

        // Assert
        mockEs.Verify(e => e.DeleteAsync(4), Times.Once);
        mockEs.Verify(e => e.UpsertAsync(It.IsAny<long>(), It.IsAny<OrderSnapshot>()), Times.Never);
    }
}
