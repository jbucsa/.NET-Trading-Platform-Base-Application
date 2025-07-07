[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ITradingService _tradingService;

    public AccountsController(
        UserManager<User> userManager,
        ITradingService tradingService)
    {
        _userManager = userManager;
        _tradingService = tradingService;
    }

    [HttpGet("portfolios")]
    public async Task<IActionResult> GetPortfolios()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var portfolios = await _tradingService.GetPortfoliosAsync(user.Id);
        return Ok(portfolios);
    }
}