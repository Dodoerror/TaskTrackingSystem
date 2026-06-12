using Microsoft.EntityFrameworkCore;
using TaskTrackingSystem.Database.AppDbContextModels;

public static class DataSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // ── 1. Seed Roles ────────────────────────────────────────────────
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin" && r.IsDeleted != true);
        if (adminRole == null)
        {
            adminRole = new Role
            {
                Name = "Admin",
                Description = "Full system access",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            db.Roles.Add(adminRole);
        }

        var memberRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Member" && r.IsDeleted != true);
        if (memberRole == null)
        {
            memberRole = new Role
            {
                Name = "Member",
                Description = "Standard user access",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            db.Roles.Add(memberRole);
        }

        await db.SaveChangesAsync();

        // Re-fetch to get IDs after insert
        adminRole  = await db.Roles.FirstAsync(r => r.Name == "Admin"  && r.IsDeleted != true);
        memberRole = await db.Roles.FirstAsync(r => r.Name == "Member" && r.IsDeleted != true);

        // ── 2. Seed Default Users ────────────────────────────────────────
        var usersToSeed = new[]
        {
            new
            {
                Username    = "admin",
                FirstName   = "System",
                LastName    = "Admin",
                Email       = "admin@tts.com",
                Password    = "Admin@123",
                Phone       = (string?)"09000000000",
                RoleId      = adminRole.Id
            },
            new
            {
                Username    = "member",
                FirstName   = "Default",
                LastName    = "Member",
                Email       = "member@tts.com",
                Password    = "Member@123",
                Phone       = (string?)"09000000001",
                RoleId      = memberRole.Id
            }
        };

        foreach (var u in usersToSeed)
        {
            var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Username == u.Username);
            if (existingUser != null)
            {
                if (existingUser.IsDeleted)
                {
                    existingUser.IsDeleted = false;
                    existingUser.IsActive = true;
                    existingUser.Email = u.Email;
                    existingUser.RoleId = u.RoleId;
                }
            }
            else
            {
                db.Users.Add(new User
                {
                    Username     = u.Username,
                    FirstName    = u.FirstName,
                    LastName     = u.LastName,
                    Email        = u.Email,
                    PasswordHash = "HASHED_" + u.Password,   // matches AuthService hashing
                    Phone        = u.Phone,
                    RoleId       = u.RoleId,
                    IsActive     = true,
                    IsDeleted    = false,
                    CreatedAt    = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();

        // ── 3. Seed MenuAdmins ───────────────────────────────────────────
        var menusToSeed = new[]
        {
            new MenuAdmin { AdminMenuId = "M001", MenuCode = "Dashboard", ParentCode = "0", MenuName = "Dashboard", MenuUrl = "dashboard", Visible = true, OrderNo = 1, Icon = "home", DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdmin { AdminMenuId = "M002", MenuCode = "Projects", ParentCode = "0", MenuName = "Projects", MenuUrl = "projects", Visible = true, OrderNo = 2, Icon = "folder-kanban", DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdmin { AdminMenuId = "M003", MenuCode = "Tasks", ParentCode = "0", MenuName = "Tasks", MenuUrl = "tasks", Visible = true, OrderNo = 3, Icon = "list-todo", DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdmin { AdminMenuId = "M004", MenuCode = "Reports", ParentCode = "0", MenuName = "Reports", MenuUrl = "reports", Visible = true, OrderNo = 4, Icon = "bar-chart-2", DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdmin { AdminMenuId = "M005", MenuCode = "Roles", ParentCode = "0", MenuName = "Roles", MenuUrl = "roles", Visible = true, OrderNo = 5, Icon = "shield", DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdmin { AdminMenuId = "M006", MenuCode = "Users", ParentCode = "0", MenuName = "Users", MenuUrl = "users", Visible = true, OrderNo = 6, Icon = "users", DelFlag = 0, CreatedDateTime = DateTime.UtcNow },

            // Reports sub-menus
            new MenuAdmin { AdminMenuId = "M007", MenuCode = "Reports_Employees", ParentCode = "Reports", MenuName = "Employee Report", MenuUrl = "reports/employees", Visible = true, OrderNo = 1, Icon = "users", DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdmin { AdminMenuId = "M008", MenuCode = "Reports_Projects", ParentCode = "Reports", MenuName = "Project Progress", MenuUrl = "reports/projects", Visible = true, OrderNo = 2, Icon = "folder-kanban", DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdmin { AdminMenuId = "M009", MenuCode = "Reports_Tasks", ParentCode = "Reports", MenuName = "Task Report", MenuUrl = "reports/tasks", Visible = true, OrderNo = 3, Icon = "check-square", DelFlag = 0, CreatedDateTime = DateTime.UtcNow }
        };

        foreach (var menu in menusToSeed)
        {
            var exists = await db.MenuAdmins.AnyAsync(m => m.MenuCode == menu.MenuCode);
            if (!exists)
            {
                db.MenuAdmins.Add(menu);
            }
        }
        await db.SaveChangesAsync();

        // ── 3.5 Seed MenuAdminDetails ────────────────────────────────────
        var detailsToSeed = new[]
        {
            // Projects Actions
            new MenuAdminDetail { MenuAdminDetailId = "MAD001", MenuDetailCode = "Projects_List", ParentMenuCode = "Projects", ActionName = "List", ApiName = "api/Project", Visible = true, OrderNo = 1, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD002", MenuDetailCode = "Projects_Create", ParentMenuCode = "Projects", ActionName = "Create", ApiName = "api/Project", Visible = true, OrderNo = 2, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD003", MenuDetailCode = "Projects_Update", ParentMenuCode = "Projects", ActionName = "Update", ApiName = "api/Project", Visible = true, OrderNo = 3, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD004", MenuDetailCode = "Projects_Delete", ParentMenuCode = "Projects", ActionName = "Delete", ApiName = "api/Project", Visible = true, OrderNo = 4, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },

            // Tasks Actions
            new MenuAdminDetail { MenuAdminDetailId = "MAD005", MenuDetailCode = "Tasks_List", ParentMenuCode = "Tasks", ActionName = "List", ApiName = "api/Task", Visible = true, OrderNo = 1, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD006", MenuDetailCode = "Tasks_Create", ParentMenuCode = "Tasks", ActionName = "Create", ApiName = "api/Task", Visible = true, OrderNo = 2, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD007", MenuDetailCode = "Tasks_Update", ParentMenuCode = "Tasks", ActionName = "Update", ApiName = "api/Task", Visible = true, OrderNo = 3, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD008", MenuDetailCode = "Tasks_Delete", ParentMenuCode = "Tasks", ActionName = "Delete", ApiName = "api/Task", Visible = true, OrderNo = 4, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },

            // Roles Actions
            new MenuAdminDetail { MenuAdminDetailId = "MAD009", MenuDetailCode = "Roles_List", ParentMenuCode = "Roles", ActionName = "List", ApiName = "api/Role", Visible = true, OrderNo = 1, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD010", MenuDetailCode = "Roles_Create", ParentMenuCode = "Roles", ActionName = "Create", ApiName = "api/Role", Visible = true, OrderNo = 2, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD011", MenuDetailCode = "Roles_Update", ParentMenuCode = "Roles", ActionName = "Update", ApiName = "api/Role", Visible = true, OrderNo = 3, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD012", MenuDetailCode = "Roles_Delete", ParentMenuCode = "Roles", ActionName = "Delete", ApiName = "api/Role", Visible = true, OrderNo = 4, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },

            // Users Actions
            new MenuAdminDetail { MenuAdminDetailId = "MAD013", MenuDetailCode = "Users_List", ParentMenuCode = "Users", ActionName = "List", ApiName = "api/User", Visible = true, OrderNo = 1, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD014", MenuDetailCode = "Users_Create", ParentMenuCode = "Users", ActionName = "Create", ApiName = "api/User", Visible = true, OrderNo = 2, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD015", MenuDetailCode = "Users_Update", ParentMenuCode = "Users", ActionName = "Update", ApiName = "api/User", Visible = true, OrderNo = 3, DelFlag = 0, CreatedDateTime = DateTime.UtcNow },
            new MenuAdminDetail { MenuAdminDetailId = "MAD016", MenuDetailCode = "Users_Delete", ParentMenuCode = "Users", ActionName = "Delete", ApiName = "api/User", Visible = true, OrderNo = 4, DelFlag = 0, CreatedDateTime = DateTime.UtcNow }
        };

        foreach (var detail in detailsToSeed)
        {
            var exists = await db.MenuAdminDetails.AnyAsync(d => d.MenuDetailCode == detail.MenuDetailCode);
            if (!exists)
            {
                db.MenuAdminDetails.Add(detail);
            }
        }
        await db.SaveChangesAsync();

        // ── 4. Seed RoleMenus ────────────────────────────────────────────
        var roleMenusToSeed = new List<RoleMenu>();
        
        // Admin role menus: All
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM001", RoleCode = "Admin", MenuCode = "Dashboard", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM002", RoleCode = "Admin", MenuCode = "Projects", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM003", RoleCode = "Admin", MenuCode = "Tasks", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM004", RoleCode = "Admin", MenuCode = "Reports", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM005", RoleCode = "Admin", MenuCode = "Roles", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM006", RoleCode = "Admin", MenuCode = "Users", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM010", RoleCode = "Admin", MenuCode = "Reports_Employees", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM011", RoleCode = "Admin", MenuCode = "Reports_Projects", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM012", RoleCode = "Admin", MenuCode = "Reports_Tasks", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });

        // Admin role action permissions:
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA001", RoleCode = "Admin", MenuCode = "Projects_List", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA002", RoleCode = "Admin", MenuCode = "Projects_Create", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA003", RoleCode = "Admin", MenuCode = "Projects_Update", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA004", RoleCode = "Admin", MenuCode = "Projects_Delete", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });

        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA005", RoleCode = "Admin", MenuCode = "Tasks_List", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA006", RoleCode = "Admin", MenuCode = "Tasks_Create", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA007", RoleCode = "Admin", MenuCode = "Tasks_Update", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA008", RoleCode = "Admin", MenuCode = "Tasks_Delete", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });

        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA009", RoleCode = "Admin", MenuCode = "Roles_List", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA010", RoleCode = "Admin", MenuCode = "Roles_Create", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA011", RoleCode = "Admin", MenuCode = "Roles_Update", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA012", RoleCode = "Admin", MenuCode = "Roles_Delete", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });

        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA013", RoleCode = "Admin", MenuCode = "Users_List", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA014", RoleCode = "Admin", MenuCode = "Users_Create", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA015", RoleCode = "Admin", MenuCode = "Users_Update", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMA016", RoleCode = "Admin", MenuCode = "Users_Delete", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });

        // Member role menus: Dashboard, Projects, Tasks
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM007", RoleCode = "Member", MenuCode = "Dashboard", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM008", RoleCode = "Member", MenuCode = "Projects", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RM009", RoleCode = "Member", MenuCode = "Tasks", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });

        // Member role action permissions:
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMM001", RoleCode = "Member", MenuCode = "Projects_List", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMM002", RoleCode = "Member", MenuCode = "Projects_Create", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMM003", RoleCode = "Member", MenuCode = "Projects_Update", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });

        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMM004", RoleCode = "Member", MenuCode = "Tasks_List", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMM005", RoleCode = "Member", MenuCode = "Tasks_Create", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });
        roleMenusToSeed.Add(new RoleMenu { RoleMenuId = "RMM006", RoleCode = "Member", MenuCode = "Tasks_Update", DelFlag = 0, CreatedDateTime = DateTime.UtcNow });

        foreach (var rm in roleMenusToSeed)
        {
            var exists = await db.RoleMenus.AnyAsync(x => x.RoleCode == rm.RoleCode && x.MenuCode == rm.MenuCode);
            if (!exists)
            {
                db.RoleMenus.Add(rm);
            }
        }
        await db.SaveChangesAsync();

        Console.WriteLine("✅ DataSeeder: Roles, default users & dynamic menus seeded successfully.");
    }
}
