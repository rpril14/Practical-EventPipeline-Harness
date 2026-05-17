using System.Threading.Tasks;
using EventPipeline.Worker.Models;

namespace EventPipeline.Worker.Handlers;

public interface IOrderEventHandler
{
    Task HandleAsync(CdcEvent<OrderSnapshot> cdcEvent);
}
