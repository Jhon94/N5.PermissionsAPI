using Microsoft.AspNetCore.Mvc;
using N5.Permissions.Domain.Interfaces;

namespace N5.Permissions.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionTypesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PermissionTypesController> _logger;

        public PermissionTypesController(IUnitOfWork unitOfWork, ILogger<PermissionTypesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all permission types
        /// </summary>
        /// <returns>List of permission types</returns>
        [HttpGet]
        public async Task<IActionResult> GetPermissionTypes()
        {
            _logger.LogInformation("Get permission types endpoint called");

            try
            {
                var permissionTypes = await _unitOfWork.PermissionTypes.GetAllAsync();
                return Ok(permissionTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission types");
                return StatusCode(500, new { message = "An error occurred while processing the request" });
            }
        }
    }
}
