using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTrackingSystem.Database.AppDbContextModels;
using TaskTrackingSystem.Shared;
using TaskTrackingSystem.Shared.Models.Report;

namespace TaskTrackingSystem.WebApi.Features.Report
{
    public class ReportService
    {
        private readonly AppDbContext _db;

        public ReportService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<IEnumerable<TaskReportDto>>> GetTasksReportAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? status,
            int? projectId)
        {
            var query = _db.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.AssignedByNavigation)
                .Where(t => t.IsDeleted != true);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= endDate.Value);
            }

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            // We need to resolve the matching status from DB or map
            // Assuming status query param is status name e.g., "To Do", "In Progress", "Done"
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusLower = status.Trim().ToLower();
                long? targetStatusId = statusLower switch
                {
                    "to do" => 1,
                    "in progress" => 2,
                    "done" => 3,
                    _ => null
                };

                if (targetStatusId.HasValue)
                {
                    query = query.Where(t => t.StatusId == targetStatusId.Value);
                }
                else if (long.TryParse(status, out var parsedStatusId))
                {
                    query = query.Where(t => t.StatusId == parsedStatusId);
                }
            }

            var tasks = await query.ToListAsync();

            var statusMap = new Dictionary<long, string> { { 1, "To Do" }, { 2, "In Progress" }, { 3, "Done" } };
            var priorityMap = new Dictionary<long, string> { { 1, "Low" }, { 2, "Medium" }, { 3, "High" } };

            var reportList = tasks.Select(t => new TaskReportDto
            {
                TaskId = t.Id,
                Title = t.Title,
                Description = t.Description,
                ProjectId = t.ProjectId,
                ProjectName = t.Project.Name,
                StatusId = t.StatusId,
                StatusName = statusMap.TryGetValue(t.StatusId, out var statusName) ? statusName : $"Status {t.StatusId}",
                PriorityId = t.PriorityId,
                PriorityName = priorityMap.TryGetValue(t.PriorityId, out var priorityName) ? priorityName : $"Priority {t.PriorityId}",
                AssignedToUserId = t.AssignedTo,
                AssignedToUser = t.AssignedToNavigation != null ? $"{t.AssignedToNavigation.FirstName} {t.AssignedToNavigation.LastName}" : null,
                AssignedByUser = t.AssignedByNavigation != null ? $"{t.AssignedByNavigation.FirstName} {t.AssignedByNavigation.LastName}" : null,
                EstimatedHours = t.EstimatedHours,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt ?? DateTime.UtcNow
            }).ToList();

            return Result<IEnumerable<TaskReportDto>>.Success(reportList);
        }

        public async Task<Result<IEnumerable<UserProductivityDto>>> GetUserProductivityReportAsync()
        {
            var users = await _db.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            var productivityList = new List<UserProductivityDto>();

            foreach (var user in users)
            {
                var tasks = await _db.Tasks
                    .Where(t => t.AssignedTo == user.Id && t.IsDeleted != true)
                    .ToListAsync();

                int totalAssigned = tasks.Count;
                int completed = tasks.Count(t => t.StatusId == 3); // StatusId 3 = Done

                double efficiency = totalAssigned > 0 ? Math.Round(((double)completed / totalAssigned) * 100, 2) : 0;

                productivityList.Add(new UserProductivityDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    TotalAssignedTasks = totalAssigned,
                    CompletedTasksCount = completed,
                    EfficiencyRatio = efficiency
                });
            }

            return Result<IEnumerable<UserProductivityDto>>.Success(productivityList);
        }
    }
}
