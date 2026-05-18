using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EventPipeline.Worker.Models;
using EventPipeline.Worker.Options;
using Microsoft.Extensions.Options;

namespace EventPipeline.Worker.Clients;

public class ElasticsearchClient(HttpClient http, IOptions<ElasticsearchOptions> options) : IElasticsearchClient
{
    private readonly string _index = options.Value.Index;

    public async Task UpsertAsync(long id, OrderSnapshot document)
    {
        var response = await http.PutAsJsonAsync($"{_index}/_doc/{id}", document);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(long id)
    {
        var response = await http.DeleteAsync($"{_index}/_doc/{id}");
        response.EnsureSuccessStatusCode();
    }
}
