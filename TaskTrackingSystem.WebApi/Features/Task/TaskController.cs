using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TaskTrackingSystem.Shared;
using TaskTrackingSystem.Shared.Models.Task;
using TaskTrackingSystem.WebApi.Features.Task;

namespace TaskTrackingSystem.WebApi.Features.Task
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(long id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found." });
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            try
            {
                long? currentUserId = null;
                var createdTask = await _taskService.CreateTaskAsync(createTaskDto, currentUserId);
                return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(long id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            try
            {
                long? currentUserId = null;
                var success = await _taskService.UpdateTaskAsync(id, updateTaskDto, currentUserId);
                if (!success)
                {
                    return NotFound(new { message = $"Task with ID {id} not found." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(long id)
        {
            var success = await _taskService.SoftDeleteTaskAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Task with ID {id} not found or already deleted." });
            }

            return NoContent();
        }

        [HttpGet("/api/User/{userId}/tasks")]
        public async Task<ActionResult<Result<IEnumerable<TaskDto>>>> GetUserTasks(long userId)
        {
            var result = await _taskService.GetTasksByUserIdAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(long id, [FromQuery] long statusId)
        {
            var result = await _taskService.UpdateTaskStatusAsync(id, statusId);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });
            }
            return Ok(result);
        }
    }
}
