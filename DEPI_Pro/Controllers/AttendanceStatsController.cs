using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.BLL.Service.Implementation;
using DEPI.DAL.DbContext;
using DEPI.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DEPI.PLL.Controllers
{
    /// <summary>
    /// API controller providing attendance analytics data for charts and reports.
    /// Uses in-memory caching to reduce database load on frequently accessed stats.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceStatsController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IManagerService _managerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public AttendanceStatsController(
            IAttendanceService attendanceService,
            IManagerService managerService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IMemoryCache cache)
        {
            _attendanceService = attendanceService;
            _managerService = managerService;
            _userManager = userManager;
            _context = context;
            _cache = cache;
        }

        /// <summary>
        /// Returns today's attendance summary (Present, Late, Absent, OnLeave counts).
        /// Cached for 2 minutes to reduce DB load.
        /// </summary>
        [HttpGet("today-summary")]
        public async Task<IActionResult> GetTodaySummary()
        {
            var cacheKey = $"attendance_summary_{DateTime.Today:yyyyMMdd}";

            if (!_cache.TryGetValue(cacheKey, out AttendanceSummaryDto summary))
            {
                summary = await _attendanceService.GetAttendanceSummaryByDateAsync(DateTime.Today);
                _cache.Set(cacheKey, summary, TimeSpan.FromMinutes(2));
            }

            return Ok(new
            {
                summary.TotalEmployees,
                summary.PresentCount,
                summary.LateCount,
                summary.AbsentCount,
                summary.OnLeaveCount,
                Date = summary.Date.ToString("yyyy-MM-dd")
            });
        }

        /// <summary>
        /// Returns attendance trend data for the last N days (default 7), filtered by role or department context.
        /// Cached for 5 minutes.
        /// </summary>
        [HttpGet("weekly-trend")]
        public async Task<IActionResult> GetWeeklyTrend(int days = 7, string? role = null, int? departmentId = null)
        {
            if (days < 1 || days > 90) days = 7;

            // Determine SSNs to filter by
            System.Collections.Generic.List<string>? filterSsns = null;
            string scope = "global";

            if (User.IsInRole("Manager"))
            {
                // Managers can only see their department's trend
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var deptId = _managerService.GetManagerDepartmentId(userId);
                    if (deptId.HasValue)
                    {
                        var employees = _managerService.GetDepartmentEmployees(deptId.Value);
                        filterSsns = employees.Select(e => e.EmployeeSsn).ToList();
                        scope = $"dept_{deptId.Value}";
                    }
                }
            }
            else if (User.IsInRole("Admin"))
            {
                if (!string.IsNullOrEmpty(role))
                {
                    if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                    {
                        var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
                        var managerUserIds = managerUsers.Select(u => u.Id).ToList();
                        filterSsns = await _context.Employees
                            .Where(e => managerUserIds.Contains(e.UserId))
                            .Select(e => e.EmployeeSsn)
                            .ToListAsync();
                        scope = "role_manager";
                    }
                }
                else if (departmentId.HasValue)
                {
                    var employees = _managerService.GetDepartmentEmployees(departmentId.Value);
                    filterSsns = employees.Select(e => e.EmployeeSsn).ToList();
                    scope = $"dept_{departmentId.Value}";
                }
            }

            var cacheKey = $"attendance_trend_{days}_{scope}_{DateTime.Today:yyyyMMdd}";
            if (_cache.TryGetValue(cacheKey, out object cachedResult))
            {
                return Ok(cachedResult);
            }

            var result = new System.Collections.Generic.List<object>();
            for (int i = days - 1; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                
                if (filterSsns != null)
                {
                    var records = await _attendanceService.GetAttendanceByEmployeeSsnsAsync(filterSsns, date);
                    result.Add(new
                    {
                        Date = date.ToString("dd/MM"),
                        DateFull = date.ToString("yyyy-MM-dd"),
                        Present = records.Count(r => r.Status == "Present"),
                        Late = records.Count(r => r.Status == "Late"),
                        Absent = records.Count(r => r.Status == "Absent"),
                        OnLeave = records.Count(r => r.Status == "OnLeave")
                    });
                }
                else
                {
                    var daySummary = await _attendanceService.GetAttendanceSummaryByDateAsync(date);
                    result.Add(new
                    {
                        Date = date.ToString("dd/MM"),
                        DateFull = date.ToString("yyyy-MM-dd"),
                        Present = daySummary.PresentCount,
                        Late = daySummary.LateCount,
                        Absent = daySummary.AbsentCount,
                        OnLeave = daySummary.OnLeaveCount
                    });
                }
            }

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return Ok(result);
        }

        /// <summary>
        /// Returns cached department list for dropdown filters.
        /// Cached for 10 minutes since departments rarely change.
        /// </summary>
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var cacheKey = "departments_list";
            if (!_cache.TryGetValue(cacheKey, out object departments))
            {
                departments = await _context.Departments
                    .Select(d => new { d.DepartmentId, d.Name })
                    .ToListAsync();
                _cache.Set(cacheKey, departments, TimeSpan.FromMinutes(10));
            }
            return Ok(departments);
        }

        /// <summary>
        /// Returns cached shift list for dropdown filters.
        /// Cached for 10 minutes since shifts rarely change.
        /// </summary>
        [HttpGet("shifts")]
        public async Task<IActionResult> GetShifts()
        {
            var cacheKey = "shifts_list";
            if (!_cache.TryGetValue(cacheKey, out object shifts))
            {
                shifts = await _context.Shifts
                    .Select(s => new { s.ShiftId, s.Name })
                    .ToListAsync();
                _cache.Set(cacheKey, shifts, TimeSpan.FromMinutes(10));
            }
            return Ok(shifts);
        }
    }
}
