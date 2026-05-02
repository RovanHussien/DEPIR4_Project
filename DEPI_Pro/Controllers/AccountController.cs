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
                var result = await _accountService.RegisterEmployeeAsync(employeeDto);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Registration failed. Please try again.");
                    return View(employeeDto);
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
        public async Task<IActionResult> Login(LoginDto login)
        {
            if (ModelState.IsValid) 
            {
                var user = await _accountService.LoginAsync(login);
                if (user != null) 
                {
                    if (await _accountService.CheckUserStatus(login.Email) == "Pending") 
                    {
                        return View("Pending");
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
