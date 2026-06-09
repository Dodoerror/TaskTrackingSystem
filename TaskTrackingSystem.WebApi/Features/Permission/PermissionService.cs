using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTrackingSystem.Database.AppDbContextModels;
using TaskTrackingSystem.Shared.Models.Permission;

namespace TaskTrackingSystem.WebApi.Features.Permission
{
    public class PermissionService
    {
        private readonly AppDbContext _db;

        public PermissionService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _db.Permissions
                .Where(p => p.IsDeleted != true)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Module = p.Module,
                    CreatedAt = (DateTime)p.CreatedAt
                }).ToListAsync();
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(long id)
        {
            var permission = await _db.Permissions
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted != true);

            if (permission == null) return null;

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Module = permission.Module,
                CreatedAt = (DateTime)permission.CreatedAt
            };
        }

        public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionDto dto, long? currentUserId = null)
        {
            var nameExists = await _db.Permissions.AnyAsync(p => p.Name == dto.Name && p.IsDeleted != true);
            if (nameExists)
            {
                throw new InvalidOperationException("Permission name is already taken.");
            }

            var permission = new TaskTrackingSystem.Database.AppDbContextModels.Permission
            {
                Name = dto.Name,
                Description = dto.Description,
                Module = dto.Module,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            _db.Permissions.Add(permission);
            await _db.SaveChangesAsync();

            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Module = permission.Module,
                CreatedAt = (DateTime)permission.CreatedAt
            };
        }

        public async Task<bool> UpdatePermissionAsync(long id, UpdatePermissionDto dto, long? currentUserId = null)
        {
            var permission = await _db.Permissions.FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted != true);
            if (permission == null) return false;

            var nameExists = await _db.Permissions.AnyAsync(p => p.Name == dto.Name && p.Id != id && p.IsDeleted != true);
            if (nameExists)
            {
                throw new InvalidOperationException("Permission name is already taken by another permission.");
            }

            permission.Name = dto.Name;
            permission.Description = dto.Description;
            permission.Module = dto.Module;
            permission.UpdatedAt = DateTime.UtcNow;
            permission.UpdatedBy = currentUserId;

            _db.Permissions.Update(permission);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeletePermissionAsync(long id)
        {
            var permission = await _db.Permissions.FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted != true);
            if (permission == null) return false;

            permission.IsDeleted = true;
            _db.Permissions.Update(permission);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
