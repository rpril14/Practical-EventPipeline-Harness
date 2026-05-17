using System.Threading.Tasks;
using EventPipeline.Worker.Clients;
using EventPipeline.Worker.Models;

namespace EventPipeline.Worker.Handlers;

public class OrderSearchHandler(IElasticsearchClient es) : IOrderEventHandler
{
    public async Task HandleAsync(CdcEvent<OrderSnapshot> cdcEvent)
    {
        if (cdcEvent.Op == "d")
        {
            await es.DeleteAsync(cdcEvent.Before!.Id);
            return;
        }
        await es.UpsertAsync(cdcEvent.After!.Id, cdcEvent.After);
    }
}
