using FinanceNotifier.Core;

using Microsoft.AspNetCore.Mvc;

namespace FinanceNotifier.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IDataScraperFormatter _scraperFormatter;
    private readonly IUrlValidator _urlValidator;
    private readonly ISummarizer _summarizer;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(
        IDataScraperFormatter scraperFormatter,
        IUrlValidator urlValidator,
        ISummarizer summarizer,
        ILogger<ArticlesController> logger)
    {
        _scraperFormatter = scraperFormatter;
        _urlValidator = urlValidator;
        _summarizer = summarizer;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            _logger.LogInformation("Testing API endpoint...");
            return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test endpoint");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}