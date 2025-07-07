# .NET-Trading-Platform-Base-Application


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