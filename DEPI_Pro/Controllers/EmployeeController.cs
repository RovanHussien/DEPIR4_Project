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

       
        public async Task<IActionResult> Profile(string ssn)
        {
            
            if (string.IsNullOrEmpty(ssn))
            {
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst(c => c.Type == "sub")?.Value; 

                if (!string.IsNullOrEmpty(userId))
                {
                    
                    var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);

                    if (employee != null)
                    {
                        ssn = employee.EmployeeSsn;
                    }
                }
            }

           
            if (string.IsNullOrEmpty(ssn))
            {
                
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return BadRequest($"Error: Could not link current user (ID: {currentUserId}) to any Employee record.");
            }

            
            var employeeProfile = await _employeeService.GetEmployeeProfileAsync(ssn);
            if (employeeProfile == null)
            {
                return NotFound("Sorry, employee profile not found.");
            }

            ViewData["Schedules"] = await _employeeService.GetMyScheduleAsync(ssn);

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
                    return RedirectToAction(nameof(Profile), new { ssn = request.EmployeeSsn });
                }

                ModelState.AddModelError("", "An error occurred while saving the vacation request, please try again.");
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError("", ex.Message);
            }

           
            ViewData["Schedules"] = await _employeeService.GetMyScheduleAsync(request.EmployeeSsn);
            var employee = await _employeeService.GetEmployeeProfileAsync(request.EmployeeSsn);
            return View(nameof(Profile), employee);
        }

       
        
        [HttpPost]
        public async Task<IActionResult> RequestSwap(int scheduleId, string requestingSsn, string recipientSsn)
        {
            if (scheduleId < 0 || string.IsNullOrEmpty(requestingSsn) || string.IsNullOrEmpty(recipientSsn))
            {
                TempData["ErrorMessage"] = "Missing or invalid parameters for the swap request.";
                return RedirectToAction(nameof(Profile), new { ssn = requestingSsn });
            }

            var result = await _employeeService.CreateSwapRequestAsync(scheduleId, requestingSsn, recipientSsn);
            if (result)
            {
                TempData["SuccessMessage"] = "Swap request has been sent to your colleague successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to send swap request at this time.";
            }

            
            return RedirectToAction(nameof(Profile), new { ssn = requestingSsn });
        }
        [HttpPost]
        public async Task<IActionResult> RespondToSwap(int swapId, string status)
        {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? User.FindFirst(c => c.Type == "sub")?.Value;

    var employee = await _employeeService.GetEmployeeByUserIdAsync(userId);

    var result = await _employeeService.RespondToSwapRequestAsync(swapId, status);

    if (result)
        TempData["SuccessMessage"] = $"Swap request {status} successfully.";
    else
        TempData["ErrorMessage"] = "Failed to process swap response.";

    return RedirectToAction(nameof(Profile), new { ssn = employee.EmployeeSsn });
}
    }
}
