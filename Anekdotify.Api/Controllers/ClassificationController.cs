using Anekdotify.BL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Anekdotify.Api.Controllers
{
    [ApiController]
    [Route("api/classification")]
    [Authorize]
    public class ClassificationController : ControllerBase
    {
        private readonly IClassificationRepository _classificationRepo;
        public ClassificationController(IClassificationRepository classificationRepo)
        {
            _classificationRepo = classificationRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClassificationAsync([FromBody] string classificationName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(classificationName))
            {
                return BadRequest("Classification name cannot be empty");
            }
            var classExisting = await _classificationRepo.IsExistingAsync(classificationName);
            if (!classExisting)
            {
                var creatingResult = await _classificationRepo.CreateClassificationAsync(classificationName);
                return Ok(creatingResult.Value);
            }
            else
            {
                return BadRequest("Classification already exists");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClassificationsAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var classifications = await _classificationRepo.GetAllClassificationsAsync();
            return Ok(classifications.Value);
        }

        [HttpGet]
        [Route("{classificationId:int}")]
        public async Task<IActionResult> GetClassificationByIdAsync([FromRoute] int classificationId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var getResult = await _classificationRepo.GetClassificationByIdAsync(classificationId);

            if (getResult.IsNotFound)
            {
                return NotFound(getResult.ErrorMessage);
            }
            return Ok(getResult.Value);
        }

        [HttpPut]
        [Route("{classificationId:int}")]
        public async Task<IActionResult> UpdateClassificationAsync([FromRoute] int classifclassificationId, [FromBody] string classificationName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(classificationName))
            {
                return BadRequest("Classification name cannot be empty");
            }
            var updateResult = await _classificationRepo.UpdateClassificationAsync(classifclassificationId, classificationName);
            if (updateResult.IsNotFound)
            {
                return NotFound(updateResult.ErrorMessage);
            }
            return Ok(updateResult.Value);
        }
        [HttpDelete]
        [Route("{classificationId:int}")]
        public async Task<IActionResult> DeleteClassificationAsync([FromRoute] int classificationId)
        {
            var deleteResult = await _classificationRepo.DeleteClassificationAsync(classificationId);
            if (deleteResult.IsNotFound)
            {
                return NotFound(deleteResult.ErrorMessage);
            }
            return Ok(new {ClassificationId = classificationId, Deleted = true});
        }
    }
}