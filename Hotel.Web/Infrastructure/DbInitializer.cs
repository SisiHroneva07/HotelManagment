using Hotel.Data;
using Hotel.Data.Models;
using Hotel.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Infrastructure;

/// <summary>
/// Applies migrations and seeds baseline roles plus an administrator account.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Ensures the database exists, runs pending migrations, and seeds roles/admin user.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync().ConfigureAwait(false);

        foreach (var role in new[] { HotelRoles.Admin, HotelRoles.Staff })
        {
            if (!await roleManager.RoleExistsAsync(role).ConfigureAwait(false))
            {
                await roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);
            }
        }

        const string adminEmail = "admin@velvetvine.local";
        var admin = await userManager.FindByEmailAsync(adminEmail).ConfigureAwait(false);
        if (admin != null)
        {
            return;
        }

        admin = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            EGN = "0000000000",
            PhoneNumber = "0888123456",
            HireDate = DateTime.UtcNow.Date,
            IsActive = true,
            DismissalDate = null
        };

        var result = await userManager.CreateAsync(admin, "Admin123!").ConfigureAwait(false);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, HotelRoles.Admin).ConfigureAwait(false);
        }
    }
}
