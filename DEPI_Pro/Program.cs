using Microsoft.EntityFrameworkCore;
using DEPI.DAL.DbContext;
using DEPI.DAL.Models;
using Microsoft.AspNetCore.Identity;
using DEPI.DAL.Repo.Interfaces;
using DEPI.DAL.Repo.Implementation;
using DEPI.BLL.Service.Implementation;
using DEPI.BLL.Service.Interfaces;

namespace DEPI_Pro
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddScoped<IEmployeeRepo, EmployeeRepo>();
            builder.Services.AddScoped<IUserRepo, UserRepo>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // Seed Admin and Roles
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    string[] roleNames = { "Admin", "Manager", "Employee" };
                    foreach (var roleName in roleNames)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            await roleManager.CreateAsync(new IdentityRole(roleName));
                        }
                    }

                    var adminEmail = "admin@pharmapro.com";
                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        var admin = new ApplicationUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true,
                            Status = DEPI.DAL.Enums.EmployeeStatus.Approved
                        };

                        var createAdminResult = await userManager.CreateAsync(admin, "Admin@1234");
                        if (createAdminResult.Succeeded)
                        {
                            await userManager.AddToRoleAsync(admin, "Admin");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Quietly handle or log seeding exceptions
                }
            }

            await app.RunAsync();
        }
    }
}
