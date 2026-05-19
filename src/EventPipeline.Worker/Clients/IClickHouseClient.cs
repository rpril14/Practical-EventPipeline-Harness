using System.Threading.Tasks;
using EventPipeline.Worker.Models;

namespace EventPipeline.Worker.Clients;

public interface IClickHouseClient
{
    Task InsertAsync(OrderSnapshot snapshot, string op, long tsMs, int kafkaPartition, long kafkaOffset);
}
