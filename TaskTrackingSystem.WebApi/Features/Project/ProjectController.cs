using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskTrackingSystem.Shared;
using TaskTrackingSystem.Shared.Models.User;
using TaskTrackingSystem.Shared.Models.Task;
using TaskTrackingSystem.Shared.Models.Project;

namespace TaskTrackingSystem.WebApi.Features.Project
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;

        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(long id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
            {
                return NotFound(new { message = $"Project with ID {id} not found." });
            }
            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            try
            {
                long? currentUserId = null;
                var createdProject = await _projectService.CreateProjectAsync(createProjectDto, currentUserId);
                return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, createdProject);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(long id, [FromBody] UpdateProjectDto updateProjectDto)
        {
            try
            {
                long? currentUserId = null;
                var success = await _projectService.UpdateProjectAsync(id, updateProjectDto, currentUserId);
                if (!success)
                {
                    return NotFound(new { message = $"Project with ID {id} not found." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(long id)
        {
            var success = await _projectService.SoftDeleteProjectAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Project with ID {id} not found or already deleted." });
            }

            return NoContent();
        }

        [HttpGet("{id}/members")]
        public async Task<ActionResult<Result<IEnumerable<UserDto>>>> GetProjectMembers(long id)
        {
            var result = await _projectService.GetProjectMembersAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{id}/members")]
        public async Task<IActionResult> AssignProjectMembers(long id, [FromBody] AssignMembersDto dto)
        {
            var result = await _projectService.AssignMembersToProjectAsync(id, dto);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return Ok();
        }

        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveProjectMember(long id, long userId)
        {
            var result = await _projectService.RemoveMemberFromProjectAsync(id, userId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return NoContent();
        }

        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<Result<IEnumerable<TaskDto>>>> GetProjectTasks(long id)
        {
            var result = await _projectService.GetProjectTasksAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
