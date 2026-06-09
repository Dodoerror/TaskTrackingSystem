using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskTrackingSystem.Database.AppDbContextModels;
using TaskTrackingSystem.Shared;
using TaskTrackingSystem.Shared.Models.Role;

namespace TaskTrackingSystem.WebApi.Features.Role
{
    public class RoleService
    {
        private readonly AppDbContext _db;

        public RoleService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            return await _db.Roles
                .Where(r => r.IsDeleted != true)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedAt = (DateTime)r.CreatedAt
                }).ToListAsync();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(long id)
        {
            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);

            if (role == null) return null;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = (DateTime)role.CreatedAt
            };
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto, long? currentUserId = null)
        {
            var nameExists = await _db.Roles.AnyAsync(r => r.Name == dto.Name && r.IsDeleted != true);
            if (nameExists)
            {
                throw new InvalidOperationException("Role name is already taken.");
            }

            var role = new TaskTrackingSystem.Database.AppDbContextModels.Role
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };

            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = (DateTime)role.CreatedAt
            };
        }

        public async Task<bool> UpdateRoleAsync(long id, UpdateRoleDto dto, long? currentUserId = null)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);
            if (role == null) return false;

            var nameExists = await _db.Roles.AnyAsync(r => r.Name == dto.Name && r.Id != id && r.IsDeleted != true);
            if (nameExists)
            {
                throw new InvalidOperationException("Role name is already taken by another role.");
            }

            role.Name = dto.Name;
            role.Description = dto.Description;
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = currentUserId;

            _db.Roles.Update(role);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteRoleAsync(long id)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted != true);
            if (role == null) return false;

            role.IsDeleted = true;
            _db.Roles.Update(role);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Result> AssignPermissionsToRoleAsync(long roleId, AssignPermissionsDto dto)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.IsDeleted != true);
            if (role == null)
            {
                return Result.Failure($"Role with ID {roleId} not found.", 404);
            }

            if (dto.PermissionIds == null)
            {
                return Result.Failure("Permission IDs cannot be null.", 400);
            }

            // Verify that all requested permission IDs are valid (not deleted)
            if (dto.PermissionIds.Any())
            {
                var validPermissionIds = await _db.Permissions
                    .Where(p => dto.PermissionIds.Contains(p.Id) && p.IsDeleted != true)
                    .Select(p => p.Id)
                    .ToListAsync();

                var invalidPermissionIds = dto.PermissionIds.Except(validPermissionIds).ToList();
                if (invalidPermissionIds.Any())
                {
                    return Result.Failure($"The following permission IDs are invalid or deleted: {string.Join(", ", invalidPermissionIds)}", 400);
                }
            }

            // Remove existing role permissions matching roleId
            var existingRolePermissions = await _db.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            if (existingRolePermissions.Any())
            {
                _db.RolePermissions.RemoveRange(existingRolePermissions);
            }

            // Bulk insert new RolePermission links
            foreach (var permissionId in dto.PermissionIds)
            {
                _db.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return Result.Success(200);
        }

        public async Task<Result<List<long>>> GetAssignedPermissionsByRoleIdAsync(long roleId)
        {
            var roleExists = await _db.Roles.AnyAsync(r => r.Id == roleId && r.IsDeleted != true);
            if (!roleExists)
            {
                return Result<List<long>>.Failure($"Role with ID {roleId} not found.", 404);
            }

            var permissionIds = await _db.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            return Result<List<long>>.Success(permissionIds);
        }
    }
}
