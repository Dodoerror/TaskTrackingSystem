using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTrackingSystem.Shared.Models.User
{
        public class CreateUserDto
        {
            [Required, MaxLength(50)]
            public string Username { get; set; } = string.Empty; 

            [Required, MaxLength(50)]
            public string FirstName { get; set; } = string.Empty;

            [Required, MaxLength(50)]
            public string LastName { get; set; } = string.Empty;

            [Required, EmailAddress, MaxLength(256)]
            public string Email { get; set; } = string.Empty;

            [Required, MinLength(6)]
            public string Password { get; set; } = string.Empty;

            [MaxLength(20)]
            public string? Phone { get; set; }

            [Required]
            public long RoleId { get; set; }
        }
    }
