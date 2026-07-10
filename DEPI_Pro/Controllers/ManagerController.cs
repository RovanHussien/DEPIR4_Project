using System;
using System.Linq;
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
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

        public ManagerController(IManagerService managerService, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            _managerService = managerService;
            _env = env;
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

            var uploadsFolder = System.IO.Path.Combine(_env.WebRootPath, "uploads", "profiles");
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            foreach (var ext in extensions)
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(uploadsFolder, profile.EmployeeSsn + ext)))
                {
                    ViewBag.ProfileImageUrl = $"/uploads/profiles/{profile.EmployeeSsn}{ext}";
                    break;
                }
            }

            return View(profile);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = _managerService.GetManagerProfile(userId);
            if (profile == null) return NotFound();

            var uploadsFolder = System.IO.Path.Combine(_env.WebRootPath, "uploads", "profiles");
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            foreach (var ext in extensions)
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(uploadsFolder, profile.EmployeeSsn + ext)))
                {
                    ViewBag.ProfileImageUrl = $"/uploads/profiles/{profile.EmployeeSsn}{ext}";
                    break;
                }
            }

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

            if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
            {
                var profile = _managerService.GetManagerProfile(userId);
                if (profile != null)
                {
                    var uploadsFolder = System.IO.Path.Combine(_env.WebRootPath, "uploads", "profiles");
                    System.IO.Directory.CreateDirectory(uploadsFolder);

                    var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    foreach (var oldExt in extensions)
                    {
                        var oldPath = System.IO.Path.Combine(uploadsFolder, profile.EmployeeSsn + oldExt);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    var ext = System.IO.Path.GetExtension(dto.ProfileImage.FileName).ToLower();
                    var filePath = System.IO.Path.Combine(uploadsFolder, profile.EmployeeSsn + ext);

                    using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        await dto.ProfileImage.CopyToAsync(stream);
                    }
                }
            }

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
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Leave request approved successfully." : error;
            return RedirectToAction(nameof(Leaves));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectLeave(int id)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            var (success, error) = _managerService.RejectLeaveRequest(id, deptId.Value);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Leave request rejected successfully." : error;
            return RedirectToAction(nameof(Leaves));
        }

        public IActionResult ShiftChanges()
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();
            return View(_managerService.GetDepartmentShiftChanges(deptId.Value));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveSwap(int id)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            var (success, error) = _managerService.ExecuteSwap(id, deptId.Value);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Swap executed successfully." : error;
            return RedirectToAction(nameof(ShiftChanges));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectSwap(int id)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            var (success, error) = _managerService.RejectSwap(id, deptId.Value);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Swap request rejected." : error;
            return RedirectToAction(nameof(ShiftChanges));
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
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Mission assigned successfully." : error;
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

        public IActionResult Schedules(DateTime? date)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            ViewBag.Employees = _managerService.GetDepartmentEmployees(deptId.Value);
            ViewBag.Shifts = _managerService.GetAvailableShifts();
            ViewBag.SelectedDate = date;

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            if (TempData["ErrorMessage"] != null)
                ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(_managerService.GetDepartmentScheduleRanges(deptId.Value, date));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignShift(AssignShiftDto dto)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            var (success, error) = _managerService.AssignShiftToEmployee(dto, deptId.Value);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? error : error;
            return RedirectToAction(nameof(Schedules), new { date = dto.StartDate.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveSchedule(string ids, string returnDate)
        {
            var deptId = CurrentDepartmentId();
            if (deptId == null) return Forbid();

            var idList = ids.Split(',').Select(int.Parse).ToList();
            bool anyFailed = false;
            foreach (var id in idList)
            {
                if (!_managerService.RemoveSchedule(id, deptId.Value))
                    anyFailed = true;
            }

            TempData[!anyFailed ? "SuccessMessage" : "ErrorMessage"] = !anyFailed
                ? "Schedule removed successfully."
                : "Some schedules could not be removed.";

            return RedirectToAction(nameof(Schedules), new { date = returnDate });
        }
    }
}