using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.DbContext;
using DEPI.DAL.Model;
using DEPI.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DEPI.PLL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IAdminService _adminService;
        public AdminController(IAccountService accountService, IAdminService adminService)
        {
            _accountService = accountService;
            _adminService = adminService;
        }
        public async Task<IActionResult> DisplayAllEmployee()
        {
            var users = await _adminService.GetPendingEmployeesAsync();
            return View(users);  
        }
    }
}
