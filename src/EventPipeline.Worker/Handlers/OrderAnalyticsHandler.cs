using System.Threading.Tasks;
using EventPipeline.Worker.Clients;
using EventPipeline.Worker.Models;

namespace EventPipeline.Worker.Handlers;

public class OrderAnalyticsHandler(IClickHouseClient clickHouse) : IOrderEventHandler
{
    public async Task HandleAsync(CdcEvent<OrderSnapshot> cdcEvent)
    {
        var snapshot = cdcEvent.After ?? cdcEvent.Before!;
        await clickHouse.InsertAsync(snapshot, cdcEvent.Op, cdcEvent.TsMs);
    }
}
