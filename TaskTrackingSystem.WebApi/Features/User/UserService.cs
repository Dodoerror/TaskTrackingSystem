using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTrackingSystem.Database;
using TaskTrackingSystem.Database.AppDbContextModels;
using TaskTrackingSystem.Shared.Models.User;

namespace TaskTrackingSystem.WebApi.Features.User
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            return await _db.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username, // 🆕 Mapped
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    RoleId = u.RoleId,
                    IsActive = u.IsActive,
                    CreatedAt = (DateTime)u.CreatedAt
                }).ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(long id)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username, 
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                CreatedAt = (DateTime)user.CreatedAt
            };
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto, long? currentUserId = null)
        {
            
            var usernameExists = await _db.Users.AnyAsync(u => u.Username == dto.Username && !u.IsDeleted);
            if (usernameExists)
            {
                throw new InvalidOperationException("Username is already taken.");
            }

            
            var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted);
            if (emailExists)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            string fakePasswordHash = "HASHED_" + dto.Password;

            var user = new TaskTrackingSystem.Database.AppDbContextModels.User
            {
                Username = dto.Username, 
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = fakePasswordHash,
                Phone = dto.Phone,
                RoleId = dto.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username, 
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                CreatedAt = (DateTime)user.CreatedAt
            };
        }

        public async Task<bool> UpdateUserAsync(long id, UpdateUserDto dto, long? currentUserId = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null) return false;

            var usernameExists = await _db.Users.AnyAsync(u => u.Username == dto.Username && u.Id != id && !u.IsDeleted);
            if (usernameExists)
            {
                throw new InvalidOperationException("Username is already taken by another user.");
            }

            user.Username = dto.Username; 
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Phone = dto.Phone;
            user.RoleId = dto.RoleId;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = currentUserId;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(long id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null) return false;

            user.IsDeleted = true;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}