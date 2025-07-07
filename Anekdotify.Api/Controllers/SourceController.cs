using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anekdotify.Api.Controllers;

[ApiController]
[Route("api/source")]
[Authorize]
public class SourceController : ControllerBase
{
    private readonly IClassifficationService _classificationService;
    private readonly ILogger<SourceController> _logger;

    public SourceController(IClassifficationService classificationService, ILogger<SourceController> logger)
    {
        _classificationService = classificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSources()
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state in GetAllSources.");
            return BadRequest(ModelState);
        }

        var sources = await _classificationService.GetAllSourcesAsync();
        _logger.LogInformation("Fetched all sources (Count: {Count})", sources.Value?.Count ?? 0);
        return Ok(sources.Value);
    }
}