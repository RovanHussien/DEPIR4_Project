using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DEPI.PLL.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IAdminService _adminService;
        public AccountController(IAccountService accountService, IAdminService adminService)
        {
            _accountService = accountService;
            _adminService = adminService;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(EmployeeRegisterationDto employeeDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _accountService.RegisterEmployeeAsync(employeeDto);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    if (errorMsg.Contains("PRIMARY KEY") || errorMsg.Contains("duplicate key"))
                    {
                        ModelState.AddModelError("EmployeeId", "This Employee ID / SSN is already registered.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Database Error: " + errorMsg);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
                }
            }
            return View(employeeDto);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto login, [FromServices] Microsoft.AspNetCore.Identity.UserManager<DEPI.DAL.Models.ApplicationUser> userManager)
        {
            if (ModelState.IsValid) 
            {
                var result = await _accountService.LoginAsync(login);
                if (result.Succeeded) 
                {
                    var status = await _accountService.CheckUserStatus(login.Email);
                    if (status == "Pending" || status == "Rejected") 
                    {
                        return View("Pending");
                    }
                    
                    var appUser = await userManager.FindByEmailAsync(login.Email);
                    if (appUser != null && await userManager.IsInRoleAsync(appUser, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(login);
            }
            return View(login);
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
