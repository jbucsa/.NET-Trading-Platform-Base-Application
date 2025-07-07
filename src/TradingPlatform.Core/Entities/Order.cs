public class Order
{
    public int Id { get; set; }
    public string OrderId { get; set; } = Guid.NewGuid().ToString();
    public string InstrumentSymbol { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public OrderSide Side { get; set; }
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; }
}