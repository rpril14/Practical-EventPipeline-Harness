using System.Threading.Tasks;

namespace EventPipeline.Services.Orders;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request);
    Task<OrderResponse?> UpdateStatusAsync(long id, UpdateOrderStatusRequest request);
    Task<OrderResponse?> GetAsync(long id);
}
