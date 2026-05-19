using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventPipeline.Worker.Handlers;
using EventPipeline.Worker.Models;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventPipeline.Worker.Consumers;

public class OrderCdcConsumer(
    IConsumer<string, string> consumer,
    IProducer<string, string> dlqProducer,
    IOptions<KafkaOptions> options,
    IServiceProvider services,
    ILogger<OrderCdcConsumer> logger)
    : KafkaCdcConsumerBase<OrderSnapshot>(consumer, dlqProducer, options, services, logger)
{
    protected override async Task HandleAsync(CdcEvent<OrderSnapshot> evt, KafkaMessageContext context, IServiceProvider services)
    {
        foreach (var handler in services.GetRequiredService<IEnumerable<IOrderEventHandler>>())
            await handler.HandleAsync(evt, context);
    }
}
