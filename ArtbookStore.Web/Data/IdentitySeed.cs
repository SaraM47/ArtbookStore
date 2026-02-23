using ArtbookStore.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArtbookStore.Web.Data;

/*
* This static class is responsible for seeding initial Identity data. It creates default roles and an admin user based on configuration settings, but only in the Development environment to avoid affecting production data.
*/
public static class IdentitySeed
{
    // This method is called at application startup and it receives IServiceProvider so we can manually resolve required services.
    public static async Task SeedAsync(IServiceProvider services)
    {
        // Get current hosting environment
        var env = services.GetRequiredService<IHostEnvironment>();

        // Seed only in Development
        if (!env.IsDevelopment())
            return;

        // Get configuration (used to read admin credentials from appsettings)
        var config = services.GetRequiredService<IConfiguration>();
        // RoleManager handles creation and management of roles.
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        // UserManager handles creation and management of users.
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Define the roles we want to ensure exist in the system. In this case, "Admin" and "Customer".
        string[] roles = { "Admin", "Customer" };

        // Ensure each role exists.
        foreach (var role in roles)
        {
            // If role does not exist, create it.
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Read admin user credentials from configuration. These should be set in appsettings.Development.json or user secrets, and not in production configuration.
        var adminEmail = config["SeedAdmin:Email"];
        var adminPassword = config["SeedAdmin:Password"];

        // If credentials are missing, stop seeding. This is a safety check to prevent creating an admin user with empty credentials.
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        // Check if admin user already exists.
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Create new admin user
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
            };

            // Create user with password
            var result = await userManager.CreateAsync(adminUser, adminPassword);

            // If creation succeeded, assign Admin role to the user.
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // If user exists but is not in Admin role, add role.
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
