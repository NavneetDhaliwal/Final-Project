using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Areas.Identity.Data;
using WebApplication1.Data;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebApplication1 {

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("AppDBContextConnection") ?? throw new InvalidOperationException("Connection string 'AppDBContextConnection' not found.");

            builder.Services.AddDbContext<WebAuthAppDBContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<WebAuthAppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                 .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<WebAuthAppDBContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();
            //builder.Services.AddScoped<IAuthorizationMiddlewareService, AuthorizationMiddlewareService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            //using (var scope = app.Services.CreateScope())
            //{
            //    var roleManager =
            //        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //    // Define your roles here
            //    string[] roles = { "Admin", "Manager", "User" };

            //    foreach (var role in roles)
            //    {
            //        if (!await roleManager.RoleExistsAsync(role))
            //        {
            //            await roleManager.CreateAsync(new IdentityRole(role));
            //        }
            //    }
            //}

            // Define your roles here
            string[] roles = { "Admin", "Manager", "User" };

            using (var scope = app.Services.CreateScope())
            {
                // Call the method to create roles
                await EnsureRolesExist(scope.ServiceProvider, roles);
            }

            // Method to ensure roles exist
             async Task EnsureRolesExist(IServiceProvider serviceProvider, string[] roles)
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }


            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<WebAuthAppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Define the roles the users should be assigned to
                var rolesToCreate = new List<string> { "Admin", "Manager", "User" };
                foreach (var role in rolesToCreate)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Define your users and their corresponding roles here
                var usersWithRoles = new Dictionary<string, string>
        {
        { "Admin@example.com", "Admin" },
        { "Manager@example.com", "Manager" },
        { "User@example.com", "User" },
        };

                foreach (var userRole in usersWithRoles)
                {
                    var email = userRole.Key;
                    var role = userRole.Value;

                    var existingUser = await userManager.FindByEmailAsync(email);
                    if (existingUser == null)
                    {
                        var user = new WebAuthAppUser { UserName = email, Email = email, EmailConfirmed = true };
                        var result = await userManager.CreateAsync(user, "AllUsersPassword123!");

                        if (result.Succeeded)
                        {
                            // Assign the role to the user
                            if (await roleManager.RoleExistsAsync(role))
                            {
                                await userManager.AddToRoleAsync(user, role);
                            }
                            else
                            {
                                // Handle the case where the role doesn't exist (optional).
                            }
                        }
                        else
                        {
                            // Handle the case where user creation failed (optional).
                        }
                    }
                }
            }


            app.Run();

        }
    }
}