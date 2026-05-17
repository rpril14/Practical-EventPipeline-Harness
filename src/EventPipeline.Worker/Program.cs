using Confluent.Kafka;
using EventPipeline.Worker;
using EventPipeline.Worker.Clients;
using EventPipeline.Worker.Consumers;
using EventPipeline.Worker.Handlers;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<ElasticsearchOptions>(builder.Configuration.GetSection("Elasticsearch"));
builder.Services.Configure<ClickHouseOptions>(builder.Configuration.GetSection("ClickHouse"));

builder.Services.AddSingleton<IElasticsearchClient, ElasticsearchClient>();
builder.Services.AddSingleton<IClickHouseClient, ClickHouseClient>();
builder.Services.AddSingleton<IOrderEventHandler, OrderSearchHandler>();
builder.Services.AddSingleton<IOrderEventHandler, OrderAnalyticsHandler>();
builder.Services.AddSingleton<RetryPolicy>();

builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
    return new ConsumerBuilder<string, string>(new ConsumerConfig
    {
        BootstrapServers = opts.BootstrapServers,
        GroupId = opts.GroupId,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
    }).Build();
});

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
    return new ProducerBuilder<string, string>(
        new ProducerConfig { BootstrapServers = opts.BootstrapServers }).Build();
});

builder.Services.AddHostedService<OrderCdcConsumer>();

var host = builder.Build();
host.Run();
