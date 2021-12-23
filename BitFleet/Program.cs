using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BitFleet.Data;
using BitFleet.Domain;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BitFleet
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();

                await dbContext.Database.MigrateAsync();

                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var roles = new[] {"SuperAdmin","Admin", "CarUser"};

                foreach (var role in roles)
                {
                    if (await roleManager.RoleExistsAsync(role)) continue;

                    var roleToCreate = new IdentityRole(role);
                    await roleManager.CreateAsync(roleToCreate);
                }


                var userNames = new[] {"SuperAdmin", "Admin", "CarUser"};

                //move to seeder class
                foreach (var userName in userNames)
                {
                    if (await userManager.FindByNameAsync(userName) == null)
                    {
                        var newUserId = Guid.NewGuid();
                        var newUserRole = userName;
                        var newUser = new IdentityUser
                        {
                            Id = newUserId.ToString(),
                            UserName = userName,
                            Email = userName +"@test.com"
                        };
                        await userManager.CreateAsync(newUser, "Test1234.");
                        await userManager.AddToRoleAsync(newUser, newUserRole);
                        await userManager.AddClaimAsync(newUser, new Claim("FirstName", userName.ToLower()));
                        await userManager.AddClaimAsync(newUser, new Claim("LastName", userName.ToUpper()));
                        await userManager.AddClaimAsync(newUser, new Claim("IsOnRide", "false"));
                    }
                }
                
            }

            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}