using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.DbContext;
using DEPI.DAL.Model;
using DEPI.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using DEPI.DAL.Enums;

namespace DEPI.PLL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IAdminService _adminService;
        private readonly IDepartmentService _departmentService;
        private readonly IProductionLineService _productionLineService;
        private readonly IShiftService _shiftService;
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IAccountService accountService, IAdminService adminService,
            IDepartmentService departmentService, IProductionLineService productionLineService,
            IShiftService shiftService, IAttendanceService attendanceService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _accountService = accountService;
            _adminService = adminService;
            _departmentService = departmentService;
            _productionLineService = productionLineService;
            _shiftService = shiftService;
            _attendanceService = attendanceService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> DisplayAllEmployee()
        {
            var users = await _adminService.GetPendingEmployeesAsync();
            return View(users);
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _adminService.GetAdminDashboardStatsAsync();
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var productionLines = await _productionLineService.GetAllProductionLinesAsync();

            stats.ActiveDepartments = departments.Count;
            stats.TotalProductionLines = productionLines.Count;

            return View(stats);
        }

        public async Task<IActionResult> PendingRegistrations()
        {
            var pendingUsers = await _adminService.GetPendingEmployeesAsync();
            return View(pendingUsers);
        }

        [HttpGet]
        public async Task<IActionResult> ApproveEmployee(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Status.ToString() != "Pending")
                return NotFound();

            var lines = await _productionLineService.GetAllProductionLinesAsync();
            ViewBag.ProductionLines = new SelectList(lines, "ProductionLineId", "Name");

            var approvalDto = new AdminApprovalDto
            {
                UserId = user.Id,
                Email = user.Email
            };

            return View(approvalDto);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveEmployee(AdminApprovalDto approvalDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _adminService.CompleteEmployeeApprovalAsync(approvalDto.UserId, approvalDto);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Employee approved successfully.";
                        return RedirectToAction(nameof(PendingRegistrations));
                    }
                    ModelState.AddModelError("", "Failed to approve employee.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
            var lines = await _productionLineService.GetAllProductionLinesAsync();
            ViewBag.ProductionLines = new SelectList(lines, "ProductionLineId", "Name");
            return View(approvalDto);
        }

        [HttpPost]
        public async Task<IActionResult> RejectEmployee(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                try
                {
                    await _adminService.RejectEmployeeAsync(user.Email);
                    TempData["SuccessMessage"] = "Employee rejected successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(PendingRegistrations));
        }

        public async Task<IActionResult> Users()
        {
            var users = await _adminService.GetAllUsersGroupedByDepartmentAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var lines = await _productionLineService.GetAllProductionLinesAsync();
            ViewBag.ProductionLines = new SelectList(lines, "ProductionLineId", "Name");
            return View(new UserManagementDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserManagementDto userDto, string password)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(password))
            {
                try {
                    await _adminService.AddUserAsync(userDto, password);
                    TempData["SuccessMessage"] = "User created successfully.";
                    return RedirectToAction(nameof(Users));
                } catch (Exception ex) {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
            if (string.IsNullOrEmpty(password))
                ModelState.AddModelError("Password", "Password is required");

            var lines = await _productionLineService.GetAllProductionLinesAsync();
            ViewBag.ProductionLines = new SelectList(lines, "ProductionLineId", "Name");
            return View(userDto);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var users = await _adminService.GetAllUsersGroupedByDepartmentAsync();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            var lines = await _productionLineService.GetAllProductionLinesAsync();
            ViewBag.ProductionLines = new SelectList(lines, "ProductionLineId", "Name");
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(string id, UserManagementDto userDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _adminService.UpdateUserAsync(id, userDto);
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction(nameof(Users));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            var lines = await _productionLineService.GetAllProductionLinesAsync();
            ViewBag.ProductionLines = new SelectList(lines, "ProductionLineId", "Name");
            return View(userDto);
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            try
            {
                await _adminService.DeactivateUserAsync(id);
                TempData["SuccessMessage"] = "User deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Departments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return View(departments);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDepartment()
        {
            await PopulateManagersDropdown();
            return View(new DepartmentDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateDepartment(DepartmentDto departmentDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _departmentService.AddDepartmentAsync(departmentDto);
                    TempData["SuccessMessage"] = "Department created successfully.";
                    return RedirectToAction(nameof(Departments));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            await PopulateManagersDropdown();
            return View(departmentDto);
        }

        [HttpGet]
        public async Task<IActionResult> EditDepartment(int id)
        {
            var dept = await _departmentService.GetDepartmentByIdAsync(id);
            if (dept == null) return NotFound();
            await PopulateManagersDropdown();
            return View(dept);
        }

        [HttpPost]
        public async Task<IActionResult> EditDepartment(int id, DepartmentDto departmentDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _departmentService.UpdateDepartmentAsync(id, departmentDto);
                    TempData["SuccessMessage"] = "Department updated successfully.";
                    return RedirectToAction(nameof(Departments));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            await PopulateManagersDropdown();
            return View(departmentDto);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                await _departmentService.DeleteDepartmentAsync(id);
                TempData["SuccessMessage"] = "Department deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Departments));
        }

        public async Task<IActionResult> ProductionLines()
        {
            var lines = await _productionLineService.GetAllProductionLinesAsync();
            return View(lines);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProductionLine()
        {
            await PopulateDepartmentsDropdown();
            return View(new ProductionLineDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductionLine(ProductionLineDto productionLineDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productionLineService.AddProductionLineAsync(productionLineDto);
                    TempData["SuccessMessage"] = "Production Line created successfully.";
                    return RedirectToAction(nameof(ProductionLines));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            await PopulateDepartmentsDropdown();
            return View(productionLineDto);
        }

        [HttpGet]
        public async Task<IActionResult> EditProductionLine(int id)
        {
            var line = await _productionLineService.GetProductionLineByIdAsync(id);
            if (line == null) return NotFound();
            await PopulateDepartmentsDropdown();
            return View(line);
        }

        [HttpPost]
        public async Task<IActionResult> EditProductionLine(int id, ProductionLineDto productionLineDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productionLineService.UpdateProductionLineAsync(id, productionLineDto);
                    TempData["SuccessMessage"] = "Production Line updated successfully.";
                    return RedirectToAction(nameof(ProductionLines));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            await PopulateDepartmentsDropdown();
            return View(productionLineDto);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductionLine(int id)
        {
            try
            {
                await _productionLineService.DeleteProductionLineAsync(id);
                TempData["SuccessMessage"] = "Production Line deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(ProductionLines));
        }

        public async Task<IActionResult> Shifts()
        {
            var shifts = await _shiftService.GetAllShiftsAsync();
            return View(shifts);
        }

        [HttpGet]
        public IActionResult CreateShift()
        {
            return View(new ShiftDto { StartTime = DateTime.Today.AddHours(8), EndTime = DateTime.Today.AddHours(16) });
        }

        [HttpPost]
        public async Task<IActionResult> CreateShift(ShiftDto shiftDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _shiftService.AddShiftAsync(shiftDto);
                    TempData["SuccessMessage"] = "Shift created successfully.";
                    return RedirectToAction(nameof(Shifts));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(shiftDto);
        }

        [HttpGet]
        public async Task<IActionResult> EditShift(int id)
        {
            var shift = await _shiftService.GetShiftByIdAsync(id);
            if (shift == null) return NotFound();
            return View(shift);
        }

        [HttpPost]
        public async Task<IActionResult> EditShift(int id, ShiftDto shiftDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _shiftService.UpdateShiftAsync(id, shiftDto);
                    TempData["SuccessMessage"] = "Shift updated successfully.";
                    return RedirectToAction(nameof(Shifts));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(shiftDto);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteShift(int id)
        {
            try
            {
                await _shiftService.DeleteShiftAsync(id);
                TempData["SuccessMessage"] = "Shift deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Shifts));
        }

        public async Task<IActionResult> ManagerAttendance(DateTime? date)
        {
            ViewBag.SelectedDate = date;
            var records = await _attendanceService.GetManagerAttendanceAsync(date);
            return View(records);
        }

        private async Task PopulateManagersDropdown()
        {
            var users = await _userManager.GetUsersInRoleAsync("Manager");
            var managerUsers = _context.Users.Include(u => u.Employee)
                .Where(u => users.Select(x => x.Id).Contains(u.Id) && u.Status == EmployeeStatus.Approved)
                .ToList();

            var managers = managerUsers
                .Select(u => new { Id = u.Employee?.EmployeeSsn, Name = $"{u.Employee?.FirstName} {u.Employee?.LastName}" })
                .ToList();
            ViewBag.Managers = new SelectList(managers, "Id", "Name");
        }

        private async Task PopulateDepartmentsDropdown()
        {
            var depts = await _departmentService.GetAllDepartmentsAsync();
            ViewBag.Departments = new SelectList(depts, "DepartmentId", "Name");
        }
    }
}
