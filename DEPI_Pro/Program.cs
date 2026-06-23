using Microsoft.EntityFrameworkCore;
using DEPI.DAL.DbContext;
using DEPI.DAL.Models;
using DEPI.DAL.Model;
using DEPI.DAL.Enums;
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
            builder.Services.AddScoped<IDepartmentRepo, DepartmentRepo>();
            builder.Services.AddScoped<IProductionLineRepo, ProductionLineRepo>();
            builder.Services.AddScoped<IShiftRepo, ShiftRepo>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IProductionLineService, ProductionLineService>();
            builder.Services.AddScoped<IShiftService, ShiftService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            var app = builder.Build();

            // Seed Roles, Admin User, and Manager User
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await SeedRoles(roleManager);
                await SeedAdminUser(userManager);
                await SeedManagerUsers(userManager, context);
            }

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

            await app.RunAsync();
        }


        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Manager", "Employee" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✓ Role '{role}' created successfully");
                }
            }
        }


        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@pharmaworks.com";
            const string adminPassword = "Admin@123456";
            const string adminName = "System Administrator";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin != null)
            {
                Console.WriteLine($"✓ Admin user already exists ({adminEmail})");
                return;
            }

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Status = EmployeeStatus.Approved,
                BaseSalary = 50000,
                ActualRole = "Admin"
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Assign Admin role
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"✓ Admin user created successfully");
                Console.WriteLine($"  Email: {adminEmail}");
                Console.WriteLine($"  Password: {adminPassword}");
                Console.WriteLine($"  Role: Admin");
            }
            else
            {
                Console.WriteLine($"✗ Failed to create admin user:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  - {error.Code}: {error.Description}");
                }
            }
        }

        private static async Task SeedManagerUsers(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            await SeedSingleManager(userManager, context, "manager@pharmaworks.com", "Manager@123456", "MGR001", "John", "Doe");
            await SeedSingleManager(userManager, context, "manager2@pharmaworks.com", "Manager@123456", "MGR002", "Jane", "Smith");
        }

        private static async Task SeedSingleManager(
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context, 
            string email, 
            string password, 
            string ssn, 
            string firstName, 
            string lastName)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                Console.WriteLine($"✓ Manager user already exists ({email})");
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Status = EmployeeStatus.Approved,
                BaseSalary = 40000,
                ActualRole = "Manager"
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Manager");
                Console.WriteLine($"✓ Manager user created successfully ({email})");
                
                var existingEmployee = await context.Employees.FindAsync(ssn);
                if (existingEmployee == null)
                {
                    var employee = new Employee
                    {
                        EmployeeSsn = ssn,
                        FirstName = firstName,
                        LastName = lastName,
                        Salary = 40000,
                        Sex = "Male",
                        BirthDate = new DateTime(1985, 5, 15),
                        Address = "123 Manager Way",
                        PhoneNumber = 123456789,
                        VacationBalance = 30,
                        DefaultRole = "Manager",
                        UserId = user.Id
                    };
                    context.Employees.Add(employee);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✓ Employee record created for Manager (SSN: {ssn})");
                }
            }
            else
            {
                Console.WriteLine($"✗ Failed to create manager user ({email}):");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  - {error.Code}: {error.Description}");
                }
            }
        }
    }
}
