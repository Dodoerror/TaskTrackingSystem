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
            var exists = await db.Users.AnyAsync(x => x.Username == u.Username && !x.IsDeleted);
            if (!exists)
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

        Console.WriteLine("✅ DataSeeder: Roles & default users seeded successfully.");
    }
}
