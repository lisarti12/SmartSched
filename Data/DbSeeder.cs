using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartSched.Api.Models;

namespace SmartSched.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            string[] roles = { "Admin", "Professor", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "lisart.mella@gmail.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Lisart",
                    LastName = "Mella",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "admin123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            if (!await context.SystemSettings.AnyAsync())
            {
                context.SystemSettings.Add(new SystemSetting
                {
                    DefaultMaxStudyHoursPerDay = 4,
                    DefaultStartHour = 16,
                    DefaultEndHour = 22,
                    DefaultBreakMinutes = 15
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
