using Anekdotify.BL.Interfaces.Repositories;
using Anekdotify.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers;

[ApiController]
[Route("api/source")]
[Authorize]
public class SourceController : ControllerBase
{
    private readonly IClassifficationService _classificationService;

    public SourceController(IClassifficationService classificationService)
    {
        _classificationService = classificationService;   
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSources()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var sources = await _classificationService.GetAllSourcesAsync();
        return Ok(sources.Value);
    }
}