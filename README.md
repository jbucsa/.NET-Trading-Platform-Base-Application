# .NET-Trading-Platform-Base-Application


## Overview
The Trading Platform is a comprehensive solution for electronic trading, providing users with:

- Real-time market data via WebSocket connections
- Order execution through FIX protocol
- Portfolio management
- User authentication and authorization
- High-performance architecture with Redis caching
  
Built on .NET Core with a modular architecture, this platform is designed for scalability and low-latency performance.

## Technology Stack
- Backend: .NET Core 6.0
- Frontend: Blazor WebAssembly (for Web UI)
- Database: SQL Server
- Caching: Redis
- Messaging: WebSocket for market data, FIX protocol for orders
- Authentication: ASP.NET Core Identity

## Getting Started
Prerequisites: 
1. .NET 6.0 SDK
2. Docker Desktop (for Redis and SQL Server)
3. Node.js (for frontend dependencies)
4. QuickFIX/n (for FIX protocol support)


## Application Organization
```bash
TradingPlatform/
├── TradingPlatform.sln
├── src/
│   ├── TradingPlatform.API/          (Web API)    
│   ├── TradingPlatform.Core/         (Domain models)    
│   ├── TradingPlatform.Infrastructure/ (Persistence)   
│   ├── TradingPlatform.Services/     (Business logic)   
│   └── TradingPlatform.Web/          (Web UI)
├── tests/
│   ├── TradingPlatform.UnitTests/
│   └── TradingPlatform.IntegrationTests/
└── docker-compose.yml                (Redis + other services)
```

## Installation
1. Clone the repository:
```bash
git clone https://github.com/your-repo/trading-platform.git
cd trading-platform
```

2. Set up infrastructure with Docker:

```bash
docker-compose up -d
```

3. Apply database migrations:
```bash
cd src/TradingPlatform.API
dotnet ef database update
```

4. Install frontend dependencies:
```bash
cd src/TradingPlatform.Web
npm install
```

## Configuration
Create ```appsettings.Development.json``` in ```src/TradingPlatform.API```:
```bash
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TradingPlatform;User=sa;Password=YourStrong@Passw0rd;"
  },
  "Redis": {
    "ConnectionString": "localhost"
  },
  "MarketData": {
    "WebSocketUrl": "wss://marketdata.example.com/ws",
    "ApiKey": "your-api-key"
  },
  "FIX": {
    "ConfigFile": "Config/FixConfig.cfg",
    "SenderCompId": "YOUR_COMP_ID",
    "TargetCompId": "BROKER_COMP_ID"
  },
  "Jwt": {
    "Key": "your-secure-key-here",
    "Issuer": "TradingPlatform",
    "Audience": "TradingPlatformUsers"
  }
}
```

## Running the Application
1. Start the backend API:
```bash
cd src/TradingPlatform.API
dotnet run
```

2. Start the frontend (in another terminal):
```bash
cd src/TradingPlatform.Web
dotnet run
```

3. Access the application:
- API: ```http://localhost:5000```
- Web UI: ```http://localhost:5001```


## Production Deployment
### Additional Requirements for Going Live
1. FIX Protocol Configuration:
- Add broker-specific FIX configuration in ```Config/FixConfig.cfg```
- Implement certificate-based authentication in ```FixOrderExecutionService```

2. Market Data Providers:
- Implement additional market data adapters in ```TradingPlatform.Services/MarketData```
- Add rate limiting for market data API calls

3. Security Enhancements:
- Configure HTTPS in production
- Set up proper CORS policies
- Implement IP whitelisting for FIX connections

4. Monitoring:
- Add Application Insights or Prometheus integration
- Set up health checks for all services
    
Example production-ready FIX configuration additions:
```bash
// In FixOrderExecutionService
private Message ConvertToFixMessage(Order order)
{
    var message = new QuickFix.FIX44.NewOrderSingle(
        new ClOrdID(order.OrderId),
        new Symbol(order.InstrumentSymbol),
        new Side(order.Side == OrderSide.Buy ? Side.BUY : Side.SELL),
        new TransactTime(DateTime.UtcNow),
        new OrdType(order.Type switch
        {
            OrderType.Market => OrdType.MARKET,
            OrderType.Limit => OrdType.LIMIT,
            _ => OrdType.LIMIT
        }));

    message.Set(new OrderQty(order.Quantity));
    if (order.Type != OrderType.Market)
    {
        message.Set(new Price(order.Price));
    }

    // Add broker-specific fields
    message.SetField(new StringField(12345, "CustomBrokerField"));
    
    return message;
}
```


## Testing
### Unit Testing
The solution includes xUnit test projects. To run tests:
```bash
cd tests/TradingPlatform.UnitTests
dotnet test
```

### Integration Testing
1. Test Configuration:
- Add ```appsettings.Testing.json``` to the integration test project:
```bash
json
{
  "UseInMemoryDatabase": true,
  "FIX": {
    "MockMode": true
  },
  "MarketData": {
    "MockMode": true
  }
}
```

2. Example Integration Test:

```bash
// TradingPlatform.IntegrationTests/TradingTests.cs
public class TradingTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;

    public TradingTests(WebApplicationFactory<Startup> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IOrderExecutionService, MockOrderExecutionService>();
                services.AddScoped<IMarketDataService, MockMarketDataService>();
            });
        });
    }

    [Fact]
    public async Task PlaceOrder_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var orderRequest = new OrderRequest
        {
            Symbol = "AAPL",
            Quantity = 100,
            Price = 150.25m,
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            PortfolioId = 1
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/trading/orders", orderRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<Order>();
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Filled, order.Status);
    }
}
```

### Testing Market Data WebSocket
Add a mock WebSocket service for testing:

```bash 
// TradingPlatform.IntegrationTests/MockWebSocketMarketDataService.cs
public class MockWebSocketMarketDataService : IMarketDataService
{
    private readonly ConcurrentDictionary<string, MarketData> _data = new();

    public MockWebSocketMarketDataService()
    {
        // Initialize with test data
        _data.TryAdd("AAPL", new MarketData { Symbol = "AAPL", Bid = 150.10m, Ask = 150.20m });
        _data.TryAdd("MSFT", new MarketData { Symbol = "MSFT", Bid = 250.50m, Ask = 250.60m });
    }

    public Task<MarketData> GetMarketDataAsync(string symbol)
    {
        if (_data.TryGetValue(symbol, out var data))
        {
            return Task.FromResult(data);
        }
        throw new KeyNotFoundException($"Symbol {symbol} not found");
    }

    public void SimulateMarketUpdate(string symbol, decimal newBid, decimal newAsk)
    {
        if (_data.TryGetValue(symbol, out var existing))
        {
            _data[symbol] = new MarketData
            {
                Symbol = symbol,
                Bid = newBid,
                Ask = newAsk,
                LastPrice = existing.LastPrice,
                Volume = existing.Volume
            };
        }
    }
}
```

## CI/CD Pipeline
Sample GitHub Actions workflow (```.github/workflows/build.yml```):

```bash
# yaml
name: Build and Test

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest

    services:
      redis:
        image: redis
        ports:
          - 6379:6379
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2019-latest
        env:
          SA_PASSWORD: "YourStrong@Passw0rd"
          ACCEPT_EULA: "Y"
        ports:
          - 1433:1433

    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '6.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --configuration Release --no-restore
      
    - name: Test
      run: dotnet test --no-restore --verbosity normal
```


## Documentation
### API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/accounts/portfolios` | GET | Get user portfolios |
| `/api/trading/orders` | POST | Place a new order |
| `/api/trading/marketdata/{symbol}` | GET | Get market data for symbol |
