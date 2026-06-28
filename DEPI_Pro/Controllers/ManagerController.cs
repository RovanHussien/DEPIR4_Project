using System;
using System.Security.Claims;
using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DEPI.PLL.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly IManagerService _managerService;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        private int? CurrentDepartmentId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _managerService.GetManagerDepartmentId(userId);
        }

        public IActionResult Index()
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            return View(_managerService.GetDashboardSummary(deptId.Value));
        }

        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = _managerService.GetManagerProfile(userId);
            if (profile == null) return NotFound();

            return View(profile);
        }
        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = _managerService.GetManagerProfile(userId);
            if (profile == null) return NotFound();

            var dto = new ManagerProfileEditDto
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                PhoneNumber = int.TryParse(profile.PhoneNumber, out var phone) ? phone : 0,
                Address = profile.Address,
                BirthDate = profile.BirthDate
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(ManagerProfileEditDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error) = await _managerService.UpdateManagerProfileAsync(userId, dto);

            if (!success)
            {
                ModelState.AddModelError("", error);
                return View(dto);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }
        public IActionResult Employees()
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            return View(_managerService.GetDepartmentEmployees(deptId.Value));
        }

        public IActionResult Leaves()
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            return View(_managerService.GetDepartmentLeaveRequests(deptId.Value));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveLeave(int id)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            var (success, error) = _managerService.ApproveLeaveRequest(id, deptId.Value);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Leave request approved successfully." : error;
            return RedirectToAction(nameof(Leaves));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectLeave(int id)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            var (success, error) = _managerService.RejectLeaveRequest(id, deptId.Value);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Leave request rejected successfully." : error;
            return RedirectToAction(nameof(Leaves));
        }

        public IActionResult ShiftChanges()
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            return View(_managerService.GetDepartmentShiftChanges(deptId.Value));
        }

        public IActionResult Missions()
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            ViewBag.Employees = _managerService.GetDepartmentEmployees(deptId.Value);
            return View(_managerService.GetDepartmentMissions(deptId.Value));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignMission(ManagerMissionCreateDto dto)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Missions));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error) = _managerService.AssignMission(dto, userId, deptId.Value);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "Mission assigned successfully." : error;
            return RedirectToAction(nameof(Missions));
        }

        public IActionResult Attendance(DateTime? date)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            ViewBag.SelectedDate = date;
            return View(_managerService.GetDepartmentAttendance(deptId.Value, date));
        }

        public IActionResult ProductionLines()
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            return View(_managerService.GetDepartmentProductionLines(deptId.Value));
        }
    }
}