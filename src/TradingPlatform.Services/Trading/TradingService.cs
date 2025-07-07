public class TradingService : ITradingService
{
    private readonly ILogger<TradingService> _logger;
    private readonly IMarketDataService _marketDataService;
    private readonly IOrderExecutionService _orderExecutionService;
    private readonly ApplicationDbContext _context;

    public TradingService(
        ILogger<TradingService> logger,
        IMarketDataService marketDataService,
        IOrderExecutionService orderExecutionService,
        ApplicationDbContext context)
    {
        _logger = logger;
        _marketDataService = marketDataService;
        _orderExecutionService = orderExecutionService;
        _context = context;
    }

    public async Task<Order> PlaceOrderAsync(OrderRequest request, string userId)
    {
        var portfolio = await _context.Portfolios
            .FirstOrDefaultAsync(p => p.Id == request.PortfolioId && p.UserId == userId);

        if (portfolio == null)
            throw new ArgumentException("Invalid portfolio");

        var order = new Order
        {
            InstrumentSymbol = request.Symbol,
            Price = request.Price,
            Quantity = request.Quantity,
            Side = request.Side,
            Type = request.Type,
            Status = OrderStatus.New,
            PortfolioId = portfolio.Id
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        try
        {
            var executionResult = await _orderExecutionService.ExecuteOrderAsync(order);
            order.Status = executionResult.Status;
            order.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing order");
            order.Status = OrderStatus.Rejected;
            await _context.SaveChangesAsync();
            throw;
        }
    }

    public async Task<MarketData> GetMarketDataAsync(string symbol)
    {
        return await _marketDataService.GetMarketDataAsync(symbol);
    }
}