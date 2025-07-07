using Anekdotify.BL.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/classification")]
    [Authorize]
    public class ClassificationController : ControllerBase
    {
        private readonly IClassifficationService _classificationService;
        private readonly ILogger<ClassificationController> _logger;

        public ClassificationController(IClassifficationService classificationService, ILogger<ClassificationController> logger)
        {
            _classificationService = classificationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClassificationAsync([FromBody] string classificationName)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in CreateClassificationAsync.");
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(classificationName))
            {
                _logger.LogWarning("Attempted to create classification with empty name.");
                return BadRequest("Classification name cannot be empty");
            }
            var classExisting = await _classificationService.IsExistingAsync(classificationName);
            if (!classExisting)
            {
                var creatingResult = await _classificationService.CreateClassificationAsync(classificationName);
                _logger.LogInformation("Created classification: {Name}", classificationName);
                return Ok(creatingResult.Value);
            }
            else
            {
                _logger.LogWarning("Attempted to create duplicate classification: {Name}", classificationName);
                return BadRequest("Classification already exists");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClassificationsAsync()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetAllClassificationsAsync.");
                return BadRequest(ModelState);
            }
            var classifications = await _classificationService.GetAllClassificationsAsync();
            _logger.LogInformation("Fetched all classifications (Count: {Count})", classifications.Value?.Count ?? 0);
            return Ok(classifications.Value);
        }

        [HttpGet]
        [Route("{classificationId:int}")]
        public async Task<IActionResult> GetClassificationByIdAsync([FromRoute] int classificationId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in GetClassificationByIdAsync for id {Id}", classificationId);
                return BadRequest(ModelState);
            }
            var getResult = await _classificationService.GetClassificationByIdAsync(classificationId);

            if (getResult.IsNotFound)
            {
                _logger.LogWarning("Classification not found for id {Id}", classificationId);
                return NotFound(getResult.ErrorMessage);
            }
            _logger.LogInformation("Fetched classification by id {Id}", classificationId);
            return Ok(getResult.Value);
        }

        [HttpPut]
        [Route("{classificationId:int}")]
        public async Task<IActionResult> UpdateClassificationAsync([FromRoute] int classifclassificationId, [FromBody] string classificationName)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in UpdateClassificationAsync for id {Id}", classifclassificationId);
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(classificationName))
            {
                _logger.LogWarning("Attempted to update classification {Id} with empty name.", classifclassificationId);
                return BadRequest("Classification name cannot be empty");
            }
            var updateResult = await _classificationService.UpdateClassificationAsync(classifclassificationId, classificationName);
            if (updateResult.IsNotFound)
            {
                _logger.LogWarning("Attempted to update non-existent classification {Id}", classifclassificationId);
                return NotFound(updateResult.ErrorMessage);
            }
            _logger.LogInformation("Updated classification {Id} to {Name}", classifclassificationId, classificationName);
            return Ok(updateResult.Value);
        }

        [HttpDelete]
        [Route("{classificationId:int}")]
        public async Task<IActionResult> DeleteClassificationAsync([FromRoute] int classificationId)
        {
            var deleteResult = await _classificationService.DeleteClassificationAsync(classificationId);
            if (deleteResult.IsNotFound)
            {
                _logger.LogWarning("Attempted to delete non-existent classification {Id}", classificationId);
                return NotFound(deleteResult.ErrorMessage);
            }
            _logger.LogInformation("Deleted classification {Id}", classificationId);
            return Ok(new { ClassificationId = classificationId, Deleted = true });
        }
    }
}