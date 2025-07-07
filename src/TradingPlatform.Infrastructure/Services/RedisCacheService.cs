public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value) : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from Redis");
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            await _database.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis");
        }
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}