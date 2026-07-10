using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DEPI.PLL.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _context;

        public AttendanceController(IAttendanceService attendanceService, ApplicationDbContext context)
        {
            _attendanceService = attendanceService;
            _context = context;
        }

        public IActionResult Scanner()
        {
            return View();
        }

        /// <summary>
        /// Returns only the currently logged-in user's fingerprint info.
        /// Simulates the biometric device automatically recognizing the user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyFingerprint()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Json(new { found = false });

            var employee = await _context.Employees
                .Include(e => e.ApplicationUser)
                .FirstOrDefaultAsync(e => e.ApplicationUser != null && e.ApplicationUser.UserName == userName);

            if (employee == null || string.IsNullOrEmpty(employee.FingerprintId))
                return Json(new { found = false });

            // Also get today's attendance status for this employee
            var todayAttendance = await _attendanceService.GetTodayAttendanceForEmployeeAsync(employee.EmployeeSsn);

            return Json(new
            {
                found = true,
                fingerprintId = employee.FingerprintId,
                name = $"{employee.FirstName} {employee.LastName}",
                role = employee.ApplicationUser?.ActualRole ?? "Employee",
                alreadyCheckedIn = todayAttendance?.CheckInTime != null,
                alreadyCheckedOut = todayAttendance?.CheckOutTime != null,
                checkInTime = todayAttendance?.CheckInTime,
                checkOutTime = todayAttendance?.CheckOutTime,
                todayStatus = todayAttendance?.Status
            });
        }

        [HttpGet]
        public async Task<IActionResult> TodayLog()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Manager"))
            {
                return Json(new System.Collections.Generic.List<AttendanceRecordDto>());
            }

            var userEmail = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userEmail);
            if (user == null)
            {
                return Json(new System.Collections.Generic.List<AttendanceRecordDto>());
            }

            var managerEmployee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
            if (managerEmployee == null)
            {
                return Json(new System.Collections.Generic.List<AttendanceRecordDto>());
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.ManagerSsn == managerEmployee.EmployeeSsn);
            if (department == null)
            {
                return Json(new System.Collections.Generic.List<AttendanceRecordDto>());
            }

            var employeeSsnsFromDept = await _context.EmployeeDepartments
                .Where(ed => ed.DepartmentID == department.DepartmentId)
                .Select(ed => ed.EmployeeSsn)
                .ToListAsync();

            var employeeSsnsFromProdLines = await _context.Employees
                .Include(e => e.ProductionLine)
                .Where(e => e.ProductionLine != null && e.ProductionLine.DepartmentId == department.DepartmentId)
                .Select(e => e.EmployeeSsn)
                .ToListAsync();

            var employeeSsnsFromManager = await _context.Employees
                .Where(e => e.ManagerSsn == managerEmployee.EmployeeSsn)
                .Select(e => e.EmployeeSsn)
                .ToListAsync();

            var allEmployeeSsns = employeeSsnsFromDept
                .Union(employeeSsnsFromProdLines)
                .Union(employeeSsnsFromManager)
                .Where(ssn => !string.IsNullOrEmpty(ssn))
                .Distinct()
                .ToList();

            var summary = await _attendanceService.GetAttendanceSummaryByDateAsync(DateTime.Today);
            var filteredRecords = summary.Records
                .Where(r => allEmployeeSsns.Contains(r.EmployeeSsn))
                .ToList();

            return Json(filteredRecords);
        }
    }
}

