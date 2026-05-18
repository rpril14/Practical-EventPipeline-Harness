using Confluent.Kafka;
using EventPipeline.Worker.Clients;
using EventPipeline.Worker.Consumers;
using EventPipeline.Worker.Handlers;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<ElasticsearchOptions>(builder.Configuration.GetSection("Elasticsearch"));
builder.Services.Configure<ClickHouseOptions>(builder.Configuration.GetSection("ClickHouse"));

builder.Services.AddHttpClient<IElasticsearchClient, ElasticsearchClient>(
    (sp, http) =>
    {
        var opts = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
        http.BaseAddress = new Uri(opts.Uri);
    }
);
builder.Services.AddSingleton<IClickHouseClient, ClickHouseClient>();
builder.Services.AddScoped<IOrderEventHandler, OrderSearchHandler>();
builder.Services.AddScoped<IOrderEventHandler, OrderAnalyticsHandler>();
builder.Services.AddSingleton(sp =>
{
    var opts = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
    return new ConsumerBuilder<string, string>(
        new ConsumerConfig
        {
            BootstrapServers = opts.BootstrapServers,
            GroupId = opts.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        }
    ).Build();
});

builder.Services.AddSingleton(sp =>
{
    var opts = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
    return new ProducerBuilder<string, string>(
        new ProducerConfig { BootstrapServers = opts.BootstrapServers }
    ).Build();
});

builder.Services.AddHostedService<OrderCdcConsumer>();

var host = builder.Build();
host.Run();
