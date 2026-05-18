using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventPipeline.Worker.Models;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventPipeline.Worker.Consumers;

public abstract class KafkaCdcConsumerBase<T>(
    IConsumer<string, string> consumer,
    IProducer<string, string> dlqProducer,
    IOptions<KafkaOptions> options,
    IServiceProvider services,
    ILogger logger) : IHostedService
{
    private readonly KafkaOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected abstract Task HandleAsync(CdcEvent<T> evt, IServiceProvider services);

    private static CdcEvent<T> DeserializeEvent(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var source = root.TryGetProperty("payload", out var payload) ? payload : root;
        return source.Deserialize<CdcEvent<T>>(JsonOptions)!;
    }

    public async Task ProcessMessageAsync(ConsumeResult<string, string> result)
    {
        try
        {
            var evt = DeserializeEvent(result.Message.Value);
            using var scope = services.CreateScope();
            await HandleAsync(evt, scope.ServiceProvider);
            consumer.Commit(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Routing message to DLQ");
            await dlqProducer.ProduceAsync(_options.DlqTopic,
                new Message<string, string> { Value = result.Message.Value });
            consumer.Commit(result);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        consumer.Subscribe(_options.Topic);
        Task.Run(() => ConsumeLoop(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        consumer.Close();
        return Task.CompletedTask;
    }

    private async Task ConsumeLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(ct);
                await ProcessMessageAsync(result);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { logger.LogError(ex, "Unhandled error in consume loop"); }
        }
    }
}
