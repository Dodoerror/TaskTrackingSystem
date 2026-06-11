using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskTrackingSystem.Shared.Models.Permission;

namespace TaskTrackingSystem.WebApi.Features.Permission
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PermissionController : ControllerBase
    {
        private readonly PermissionService _permissionService;

        public PermissionController(PermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDto>> GetPermission(long id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound(new { message = $"Permission with ID {id} not found." });
            }
            return Ok(permission);
        }

        [HttpPost]
        public async Task<ActionResult<PermissionDto>> CreatePermission([FromBody] CreatePermissionDto createPermissionDto)
        {
            try
            {
                long? currentUserId = null;
                var createdPermission = await _permissionService.CreatePermissionAsync(createPermissionDto, currentUserId);
                return CreatedAtAction(nameof(GetPermission), new { id = createdPermission.Id }, createdPermission);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(long id, [FromBody] UpdatePermissionDto updatePermissionDto)
        {
            try
            {
                long? currentUserId = null;
                var success = await _permissionService.UpdatePermissionAsync(id, updatePermissionDto, currentUserId);
                if (!success)
                {
                    return NotFound(new { message = $"Permission with ID {id} not found." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(long id)
        {
            var success = await _permissionService.SoftDeletePermissionAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Permission with ID {id} not found or already deleted." });
            }

            return NoContent();
        }
    }
}
