[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TradingController : ControllerBase
{
    private readonly ITradingService _tradingService;
    private readonly UserManager<User> _userManager;

    public TradingController(
        ITradingService tradingService,
        UserManager<User> userManager)
    {
        _tradingService = tradingService;
        _userManager = userManager;
    }

    [HttpPost("orders")]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        try
        {
            var order = await _tradingService.PlaceOrderAsync(request, user.Id);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("marketdata/{symbol}")]
    public async Task<IActionResult> GetMarketData(string symbol)
    {
        try
        {
            var marketData = await _tradingService.GetMarketDataAsync(symbol);
            return Ok(marketData);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}