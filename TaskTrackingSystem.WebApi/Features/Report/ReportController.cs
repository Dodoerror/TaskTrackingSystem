using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrackingSystem.Shared;
using TaskTrackingSystem.Shared.Models.Report;

namespace TaskTrackingSystem.WebApi.Features.Report
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("tasks")]
        public async Task<ActionResult<Result<IEnumerable<TaskReportDto>>>> GetTasksReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status,
            [FromQuery] int? projectId)
        {
            var result = await _reportService.GetTasksReportAsync(startDate, endDate, status, projectId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("user-productivity")]
        public async Task<ActionResult<Result<IEnumerable<UserProductivityDto>>>> GetUserProductivityReport()
        {
            var result = await _reportService.GetUserProductivityReportAsync();
            return StatusCode(result.StatusCode, result);
        }
    }
}
