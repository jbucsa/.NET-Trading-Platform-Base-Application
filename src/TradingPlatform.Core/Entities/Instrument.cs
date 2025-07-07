public class Instrument
{
    public string Symbol { get; set; }
    public string Name { get; set; }
    public string Exchange { get; set; }
    public string Currency { get; set; }
    public InstrumentType Type { get; set; }
    public decimal TickSize { get; set; }
    public decimal LotSize { get; set; }
}