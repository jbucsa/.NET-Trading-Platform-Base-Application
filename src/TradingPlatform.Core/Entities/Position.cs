public class Position
{
    public int Id { get; set; }
    public string InstrumentSymbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal AveragePrice { get; set; }
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; }
}