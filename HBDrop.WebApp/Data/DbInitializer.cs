using Microsoft.AspNetCore.Identity;
using HBDrop.WebApp.Models;

namespace HBDrop.WebApp.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create User role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Check if we have any users
        var usersCount = userManager.Users.Count();
        if (usersCount == 0)
        {
            // No users exist yet, we'll make the first registered user an admin
            Console.WriteLine("No users found. First registered user will be made an admin.");
        }
        else
        {
            // Check if we have any admin users
            var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
            if (!adminUsers.Any())
            {
                // Make the first user an admin
                var firstUser = userManager.Users.OrderBy(u => u.Id).FirstOrDefault();
                if (firstUser != null)
                {
                    await userManager.AddToRoleAsync(firstUser, "Admin");
                    Console.WriteLine($"Made {firstUser.Email} an administrator.");
                }
            }
        }
    }
}
