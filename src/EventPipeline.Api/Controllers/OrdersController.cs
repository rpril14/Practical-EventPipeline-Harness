using System.Threading.Tasks;
using EventPipeline.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace EventPipeline.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var result = await orderService.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await orderService.UpdateStatusAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(long id)
    {
        var result = await orderService.GetAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
