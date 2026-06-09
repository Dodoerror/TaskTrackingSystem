using System;

namespace TaskTrackingSystem.Shared.Models.Permission
{
    public class PermissionDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
