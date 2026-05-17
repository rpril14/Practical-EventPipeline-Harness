using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventPipeline.Worker.Consumers;
using EventPipeline.Worker.Handlers;
using EventPipeline.Worker.Models;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EventPipeline.Tests;

public class OrderCdcConsumer_test
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static OrderCdcConsumer CreateConsumer(
        IConsumer<string, string>? consumer = null,
        IProducer<string, string>? producer = null,
        IEnumerable<IOrderEventHandler>? handlers = null)
    {
        var opts = Options.Create(new KafkaOptions
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test",
            Topic = "test-topic",
            DlqTopic = "test-topic.dlq"
        });
        return new OrderCdcConsumer(
            consumer ?? Mock.Of<IConsumer<string, string>>(),
            producer ?? Mock.Of<IProducer<string, string>>(),
            opts,
            handlers ?? new[] { Mock.Of<IOrderEventHandler>() },
            NullLogger<OrderCdcConsumer>.Instance);
    }

    private static ConsumeResult<string, string> MakeResult(CdcEvent<OrderSnapshot> evt) =>
        new() { Message = new Message<string, string> { Value = JsonSerializer.Serialize(evt, JsonOptions) } };

    [Fact]
    public async Task ProcessMessageAsync_ValidMessage_CallsBothHandlers()
    {
        // Arrange
        var mockSearch = new Mock<IOrderEventHandler>();
        var mockAnalytics = new Mock<IOrderEventHandler>();
        var consumer = CreateConsumer(handlers: new[] { mockSearch.Object, mockAnalytics.Object });
        var evt = new CdcEvent<OrderSnapshot>(null, new OrderSnapshot(1, 2, 1, 50m, 0, 0), "c", 100L);

        // Act
        await consumer.ProcessMessageAsync(MakeResult(evt));

        // Assert
        mockSearch.Verify(h => h.HandleAsync(It.IsAny<CdcEvent<OrderSnapshot>>()), Times.Once);
        mockAnalytics.Verify(h => h.HandleAsync(It.IsAny<CdcEvent<OrderSnapshot>>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_HandlerThrows_PublishesToDlq()
    {
        // Arrange
        var mockHandler = new Mock<IOrderEventHandler>();
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<CdcEvent<OrderSnapshot>>()))
            .ThrowsAsync(new System.ArgumentException("poison"));
        var mockProducer = new Mock<IProducer<string, string>>();
        var consumer = CreateConsumer(producer: mockProducer.Object, handlers: new[] { mockHandler.Object });

        // Act
        await consumer.ProcessMessageAsync(MakeResult(new CdcEvent<OrderSnapshot>(null, new OrderSnapshot(1, 2, 1, 50m, 0, 0), "c", 0L)));

        // Assert
        mockProducer.Verify(p => p.ProduceAsync(
            "test-topic.dlq",
            It.IsAny<Message<string, string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_HandlerThrows_StillCommits()
    {
        // Arrange
        var mockConsumer = new Mock<IConsumer<string, string>>();
        var mockHandler = new Mock<IOrderEventHandler>();
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<CdcEvent<OrderSnapshot>>()))
            .ThrowsAsync(new System.ArgumentException("poison"));
        var consumer = CreateConsumer(consumer: mockConsumer.Object, handlers: new[] { mockHandler.Object });
        var result = MakeResult(new CdcEvent<OrderSnapshot>(null, new OrderSnapshot(1, 2, 1, 50m, 0, 0), "c", 0L));

        // Act
        await consumer.ProcessMessageAsync(result);

        // Assert
        mockConsumer.Verify(c => c.Commit(result), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_Success_CommitsOffset()
    {
        // Arrange
        var mockConsumer = new Mock<IConsumer<string, string>>();
        var consumer = CreateConsumer(consumer: mockConsumer.Object);
        var result = MakeResult(new CdcEvent<OrderSnapshot>(null, new OrderSnapshot(1, 2, 1, 50m, 0, 0), "c", 0L));

        // Act
        await consumer.ProcessMessageAsync(result);

        // Assert
        mockConsumer.Verify(c => c.Commit(result), Times.Once);
    }
}
