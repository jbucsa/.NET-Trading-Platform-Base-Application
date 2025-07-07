public class WebSocketMarketDataService : IMarketDataService, IDisposable
{
    private readonly ClientWebSocket _webSocket;
    private readonly Uri _webSocketUri;
    private readonly ILogger<WebSocketMarketDataService> _logger;
    private readonly CancellationTokenSource _cts;
    private readonly ConcurrentDictionary<string, MarketData> _marketDataCache;

    public WebSocketMarketDataService(
        IConfiguration configuration,
        ILogger<WebSocketMarketDataService> logger)
    {
        _webSocket = new ClientWebSocket();
        _webSocketUri = new Uri(configuration["MarketData:WebSocketUrl"]);
        _logger = logger;
        _cts = new CancellationTokenSource();
        _marketDataCache = new ConcurrentDictionary<string, MarketData>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _webSocket.ConnectAsync(_webSocketUri, _cts.Token);
            _ = Task.Run(ReceiveMessagesAsync);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to WebSocket");
            throw;
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[4096];
        try
        {
            while (_webSocket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessMarketData(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSocket receive loop");
        }
    }

    private void ProcessMarketData(string message)
    {
        try
        {
            var marketData = JsonSerializer.Deserialize<MarketDataUpdate>(message);
            if (marketData != null)
            {
                _marketDataCache.AddOrUpdate(marketData.Symbol, 
                    new MarketData(marketData),
                    (_, existing) => existing.UpdateWith(marketData));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing market data");
        }
    }

    public async Task<MarketData> GetMarketDataAsync(string symbol)
    {
        if (_marketDataCache.TryGetValue(symbol, out var marketData))
        {
            return marketData;
        }

        // Fallback to REST API if not in cache
        throw new NotImplementedException("REST fallback not implemented");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _webSocket.Dispose();
    }
}