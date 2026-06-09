using System.ComponentModel.DataAnnotations;

namespace TaskTrackingSystem.Shared.Models.Permission
{
    public class UpdatePermissionDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required, MaxLength(50)]
        public string Module { get; set; } = string.Empty;
    }
}
