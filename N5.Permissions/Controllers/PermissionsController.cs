using MediatR;
using Microsoft.AspNetCore.Mvc;
using N5.Permissions.Application.Commands.ModifyPermission;
using N5.Permissions.Application.Commands.RequestPermission;
using N5.Permissions.Application.DTOs;
using N5.Permissions.Application.Queries.GetPermissions;
using N5.Permissions.Domain.Exceptions;

namespace N5.Permissions.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Request a new permission
        /// </summary>
        /// <param name="dto">Permission request data</param>
        /// <returns>Created permission</returns>
        [HttpPost("request")]
        [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PermissionDto>> RequestPermission([FromBody] RequestPermissionDto dto)
        {
            _logger.LogInformation("Request permission endpoint called for employee: {EmployeeForename} {EmployeeSurname}",
                dto.EmployeeForename, dto.EmployeeSurname);

            try
            {
                var command = new RequestPermissionCommand
                {
                    EmployeeForename = dto.EmployeeForename,
                    EmployeeSurname = dto.EmployeeSurname,
                    PermissionTypeId = dto.PermissionTypeId,
                    PermissionDate = dto.PermissionDate
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Permission requested successfully with ID: {PermissionId}", result.Id);

                return CreatedAtAction(nameof(GetPermissionById), new { id = result.Id }, result);
            }
            catch (PermissionTypeNotFoundException ex)
            {
                _logger.LogWarning(ex, "Permission type not found: {PermissionTypeId}", dto.PermissionTypeId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting permission");
                return StatusCode(500, new { message = "An error occurred while processing the request" });
            }
        }

        /// <summary>
        /// Modify an existing permission
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <param name="dto">Updated permission data</param>
        /// <returns>Updated permission</returns>
        [HttpPut("modify/{id}")]
        [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PermissionDto>> ModifyPermission(int id, [FromBody] ModifyPermissionDto dto)
        {
            _logger.LogInformation("Modify permission endpoint called for ID: {PermissionId}", id);

            try
            {
                var command = new ModifyPermissionCommand
                {
                    Id = id,
                    EmployeeForename = dto.EmployeeForename,
                    EmployeeSurname = dto.EmployeeSurname,
                    PermissionTypeId = dto.PermissionTypeId,
                    PermissionDate = dto.PermissionDate
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Permission modified successfully with ID: {PermissionId}", result.Id);

                return Ok(result);
            }
            catch (PermissionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Permission not found: {PermissionId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (PermissionTypeNotFoundException ex)
            {
                _logger.LogWarning(ex, "Permission type not found: {PermissionTypeId}", dto.PermissionTypeId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error modifying permission with ID: {PermissionId}", id);
                return StatusCode(500, new { message = "An error occurred while processing the request" });
            }
        }

        /// <summary>
        /// Get all permissions with optional filters
        /// </summary>
        /// <param name="employeeName">Filter by employee name</param>
        /// <param name="permissionTypeId">Filter by permission type</param>
        /// <param name="fromDate">Filter from date</param>
        /// <param name="toDate">Filter to date</param>
        /// <returns>List of permissions</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions(
            [FromQuery] string? employeeName = null,
            [FromQuery] int? permissionTypeId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            _logger.LogInformation("Get permissions endpoint called with filters - Employee: {EmployeeName}, Type: {PermissionTypeId}, From: {FromDate}, To: {ToDate}",
                employeeName, permissionTypeId, fromDate, toDate);

            try
            {
                var query = new GetPermissionsQuery
                {
                    EmployeeName = employeeName,
                    PermissionTypeId = permissionTypeId,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                var result = await _mediator.Send(query);

                _logger.LogInformation("Retrieved {Count} permissions", result.Count());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                return StatusCode(500, new { message = "An error occurred while processing the request" });
            }
        }

        /// <summary>
        /// Get a specific permission by ID
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <returns>Permission details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PermissionDto>> GetPermissionById(int id)
        {
            _logger.LogInformation("Get permission by ID endpoint called for ID: {PermissionId}", id);

            try
            {
                var query = new GetPermissionByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    _logger.LogWarning("Permission not found: {PermissionId}", id);
                    return NotFound(new { message = $"Permission with ID {id} was not found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permission with ID: {PermissionId}", id);
                return StatusCode(500, new { message = "An error occurred while processing the request" });
            }
        }
    }
}
