using System;
using System.Threading.Tasks;
using EventPipeline.Worker.Models;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.Options;
using Nest;

namespace EventPipeline.Worker.Clients;

public class ElasticsearchClient(IOptions<ElasticsearchOptions> options) : IElasticsearchClient
{
    private readonly IElasticClient _client = new ElasticClient(
        new ConnectionSettings(new Uri(options.Value.Uri))
            .DefaultIndex(options.Value.Index));

    private readonly string _index = options.Value.Index;

    public async Task UpsertAsync(long id, OrderSnapshot document) =>
        await _client.IndexAsync(document, i => i.Id(id).Index(_index));

    public async Task DeleteAsync(long id) =>
        await _client.DeleteAsync<OrderSnapshot>(id, d => d.Index(_index));
}
