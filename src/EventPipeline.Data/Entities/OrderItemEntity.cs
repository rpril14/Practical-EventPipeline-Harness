namespace EventPipeline.Data.Entities;

public class OrderItemEntity
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
