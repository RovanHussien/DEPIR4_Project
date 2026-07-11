using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DEPI.PLL.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;


        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }


        public async Task<IActionResult> Profile(string ssn, string tab = "schedule")
        {
            ModelState.Clear();
            if (string.IsNullOrEmpty(ssn))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst(c => c.Type == "sub")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);
                    if (employee != null)
                        ssn = employee.EmployeeSsn;
                }
            }

            if (string.IsNullOrEmpty(ssn))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return BadRequest($"Error: Could not link current user (ID: {currentUserId}) to any Employee record.");
            }

            var employeeProfile = await _employeeService.GetEmployeeProfileAsync(ssn);
            if (employeeProfile == null)
                return NotFound("Sorry, employee profile not found.");

            ViewData["Schedules"] = await _employeeService.GetMyScheduleAsync(ssn);
            ViewData["TodaySchedule"] = await _employeeService.GetTodayScheduleAsync(ssn);
            ViewData["ActiveTab"] = tab; // ✅
            return View(employeeProfile);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyVacation(VacationRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Schedules"] = await _employeeService.GetMyScheduleAsync(request.EmployeeSsn);
                var emp = await _employeeService.GetEmployeeProfileAsync(request.EmployeeSsn);
                return View(nameof(Profile), emp);
            }
            try
            {
                var result = await _employeeService.ApplyForVacationAsync(request);
                if (result)
                {
                    TempData["SuccessMessage"] = "Vacation request submitted successfully and is under review.";
                    return RedirectToAction(nameof(Profile), new { ssn = request.EmployeeSsn, tab = "vacation" });
                }
                ModelState.AddModelError("", "An error occurred while saving the vacation request, please try again.");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Profile), new { ssn = request.EmployeeSsn, tab = "vacation" });
            }
            ViewData["Schedules"] = await _employeeService.GetMyScheduleAsync(request.EmployeeSsn);
            var employee = await _employeeService.GetEmployeeProfileAsync(request.EmployeeSsn);
            return View(nameof(Profile), employee);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture, string ssn)
        {
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(profilePicture.FileName);
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                await _employeeService.UpdateProfilePictureAsync(ssn, fileName);
            }
            return RedirectToAction(nameof(Profile), new { ssn });
        }

        [HttpPost]
        public async Task<IActionResult> RequestSwap(int scheduleId, string requestingSsn, string recipientSsn, string reason)
        {
            if (scheduleId < 0 || string.IsNullOrEmpty(requestingSsn) || string.IsNullOrEmpty(recipientSsn) || string.IsNullOrEmpty(reason))
            {
                TempData["ErrorMessage"] = "Missing or invalid parameters for the swap request.";
                return RedirectToAction(nameof(Profile), new { ssn = requestingSsn, tab = "swap" });
            }

            try
            {
                var result = await _employeeService.CreateSwapRequestAsync(scheduleId, requestingSsn, recipientSsn, reason);
                if (result)
                    TempData["SuccessMessage"] = "Swap request sent successfully.";
                else
                    TempData["ErrorMessage"] = "Failed to send swap request.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Profile), new { ssn = requestingSsn, tab = "swap" });
        }

        [HttpPost]
        public async Task<IActionResult> RespondToSwap(int swapId, string status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirst(c => c.Type == "sub")?.Value;

            var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);

            string newStatus = status == "Approved" ? "RecipientApproved" : "RecipientRejected";

            var result = await _employeeService.RespondToSwapRequestAsync(swapId, newStatus);

            if (result)
                TempData["SuccessMessage"] = status == "Approved"
                    ? "Swap request sent to manager for approval."
                    : "Swap request rejected.";
            else
                TempData["ErrorMessage"] = "Failed to process swap response.";

            return RedirectToAction(nameof(Profile), new { ssn = employee.EmployeeSsn, tab = "requests" });
        }

        [HttpGet]
        public async Task<IActionResult> GetColleagueShift(int scheduleId, string colleagueSsn)
        {
            if (scheduleId <= 0 || string.IsNullOrEmpty(colleagueSsn))
                return Json(new { success = false, message = "Invalid input." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirst(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not found." });

            var me = await _employeeService.GetEmployeeByUserIdAsync(userId);
            if (me == null)
                return Json(new { success = false, message = "Employee not found." });

            var mySchedules = await _employeeService.GetMyScheduleAsync(me.EmployeeSsn);
            var mySchedule = mySchedules.FirstOrDefault(s => s.ScheduleId == scheduleId);

            if (mySchedule == null)
                return Json(new { success = false, message = "Your schedule was not found." });

            var collSchedules = await _employeeService.GetMyScheduleAsync(colleagueSsn);
            var collSchedule = collSchedules.FirstOrDefault(s => s.ScheduleDate.Date == mySchedule.ScheduleDate.Date && s.ShiftId != null);

            if (collSchedule == null || collSchedule.Shift == null)
            {
                return Json(new { success = false, message = "Colleague has no shift scheduled on this date." });
            }

            if (collSchedule.ShiftId == mySchedule.ShiftId)
            {
                return Json(new { success = false, message = "Colleague is on the SAME shift as you on this date!" });
            }

            return Json(new
            {
                success = true,
                shiftName = collSchedule.Shift.Name,
                startTime = collSchedule.Shift.StartTime.ToString(@"hh\:mm"),
                endTime = collSchedule.Shift.EndTime.ToString(@"hh\:mm")
            });
        }
    }
}