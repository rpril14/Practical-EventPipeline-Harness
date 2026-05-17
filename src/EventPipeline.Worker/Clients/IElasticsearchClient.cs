using System.Threading.Tasks;
using EventPipeline.Worker.Models;

namespace EventPipeline.Worker.Clients;

public interface IElasticsearchClient
{
    Task UpsertAsync(long id, OrderSnapshot document);
    Task DeleteAsync(long id);
}
