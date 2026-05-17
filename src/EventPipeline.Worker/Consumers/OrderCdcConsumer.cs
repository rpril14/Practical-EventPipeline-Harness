using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventPipeline.Worker.Handlers;
using EventPipeline.Worker.Models;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventPipeline.Worker.Consumers;

public class OrderCdcConsumer(
    IConsumer<string, string> consumer,
    IProducer<string, string> dlqProducer,
    IOptions<KafkaOptions> options,
    IEnumerable<IOrderEventHandler> handlers,
    ILogger<OrderCdcConsumer> logger)
    : KafkaCdcConsumerBase<OrderSnapshot>(consumer, dlqProducer, options, logger)
{
    protected override async Task HandleAsync(CdcEvent<OrderSnapshot> evt)
    {
        foreach (var handler in handlers)
            await handler.HandleAsync(evt);
    }
}
