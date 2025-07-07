public class MarketDataBackgroundService : BackgroundService
{
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<MarketDataBackgroundService> _logger;

    public MarketDataBackgroundService(
        IMarketDataService marketDataService,
        ILogger<MarketDataBackgroundService> logger)
    {
        _marketDataService = marketDataService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (_marketDataService is WebSocketMarketDataService wsService)
            {
                await wsService.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing market data service");
        }
    }
}