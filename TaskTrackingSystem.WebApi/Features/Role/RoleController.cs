using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskTrackingSystem.Shared;
using TaskTrackingSystem.Shared.Models.Role;

namespace TaskTrackingSystem.WebApi.Features.Role
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(long id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = $"Role with ID {id} not found." });
            }
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            try
            {
                long? currentUserId = null;
                var createdRole = await _roleService.CreateRoleAsync(createRoleDto, currentUserId);
                return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, createdRole);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            try
            {
                long? currentUserId = null;
                var success = await _roleService.UpdateRoleAsync(id, updateRoleDto, currentUserId);
                if (!success)
                {
                    return NotFound(new { message = $"Role with ID {id} not found." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(long id)
        {
            var success = await _roleService.SoftDeleteRoleAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Role with ID {id} not found or already deleted." });
            }

            return NoContent();
        }

        [HttpPost("{id}/permissions")]
        public async Task<IActionResult> AssignPermissions(long id, [FromBody] AssignPermissionsDto dto)
        {
            var result = await _roleService.AssignPermissionsToRoleAsync(id, dto);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return Ok();
        }

        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<Result<List<long>>>> GetAssignedPermissions(long id)
        {
            var result = await _roleService.GetAssignedPermissionsByRoleIdAsync(id);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return Ok(result);
        }

        [HttpGet("{id}/menus")]
        public async Task<ActionResult<Result<List<string>>>> GetAssignedMenus(long id)
        {
            var result = await _roleService.GetAssignedMenusByRoleIdAsync(id);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return Ok(result);
        }

        [HttpPost("{id}/menus")]
        public async Task<IActionResult> AssignMenus(long id, [FromBody] AssignMenusDto dto)
        {
            var result = await _roleService.AssignMenusToRoleAsync(id, dto);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return Ok();
        }
    }
}
