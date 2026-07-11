using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Implementation
{

    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IVacationRequestRepo _vacationRepo;
        private readonly ISwapRequestRepo _swapRepo;
        private readonly IScheduleRepo _scheduleRepo;

        public EmployeeService(
            IEmployeeRepo employeeRepo,
            IVacationRequestRepo vacationRepo,
            ISwapRequestRepo swapRepo,
            IScheduleRepo scheduleRepo)
        {
            _employeeRepo = employeeRepo;
            _vacationRepo = vacationRepo;
            _swapRepo = swapRepo;
            _scheduleRepo = scheduleRepo;
        }

        public async Task<Employee> GetEmployeeProfileAsync(string empSsn)
        {
            return await _employeeRepo.GetEmployeeById(empSsn);
        }
        public async Task<Employee> GetEmployeeByUserIdAsync(string userId)
        {
            return await _employeeRepo.GetEmployeeByUserIdAsync(userId);
        }
        public async Task<IEnumerable<Schedule>> GetMyScheduleAsync(string empSsn)
        {
            return await _scheduleRepo.GetScheduleByEmployeeSsnAsync(empSsn);
        }

        public async Task<Schedule?> GetTodayScheduleAsync(string empSsn)
        {
            return await _scheduleRepo.GetTodayScheduleByEmployeeSsnAsync(empSsn);
        }


        public async Task<bool> CreateSwapRequestAsync(int scheduleId, string requestingEmpSsn, string recipientEmpSsn, string reason)
        {
            if (scheduleId <= 0)
                throw new Exception("You must select a specific shift to swap.");

            var requestingEmployee = await _employeeRepo.GetEmployeeById(requestingEmpSsn);
            var recipientEmployee = await _employeeRepo.GetEmployeeById(recipientEmpSsn);

            if (requestingEmployee == null || recipientEmployee == null)
                throw new Exception("Employee not found!");

            var requestingSchedules = await _scheduleRepo.GetScheduleByEmployeeSsnAsync(requestingEmpSsn);
            var specificSchedule = requestingSchedules.FirstOrDefault(s => s.ScheduleId == scheduleId);

            if (specificSchedule == null)
                throw new Exception("The selected schedule was not found.");

            var recipientSchedules = await _scheduleRepo.GetScheduleByEmployeeSsnAsync(recipientEmpSsn);
            var recipientScheduleOnDate = recipientSchedules.FirstOrDefault(s => s.ScheduleDate.Date == specificSchedule.ScheduleDate.Date && s.ShiftId != null);

            if (recipientScheduleOnDate != null && specificSchedule.ShiftId == recipientScheduleOnDate.ShiftId)
                throw new Exception("You cannot request a swap with an employee on the same shift!");

            var swapRequest = new SwapRequest
            {
                ScheduleId = scheduleId,
                RequestingEmployeeId = requestingEmpSsn,
                RecipientEmployeeId = recipientEmpSsn,
                Reason = reason
            };
            return await _swapRepo.AddSwapRequestAsync(swapRequest);
        }


        public async Task<bool> ApplyForVacationAsync(VacationRequest vacationRequest)
        {
            if (vacationRequest == null) return false;
            if (vacationRequest.StartDate < DateTime.Now.Date)
                throw new Exception("Cannot request a vacation with a past date!");
            int requestedDays = (vacationRequest.EndDate - vacationRequest.StartDate).Days + 1;
            if (requestedDays <= 0)
                throw new Exception("End date must be after the start date!");
            var employee = await _employeeRepo.GetEmployeeById(vacationRequest.EmployeeSsn);
            if (employee == null)
                throw new Exception("Employee not found!");


            var hasPendingRequest = employee.VacationRequests
                .Any(v => v.Status == VacationRequestStatus.Pending);
            if (hasPendingRequest)
                throw new Exception("You already have a pending vacation request. Please wait for it to be reviewed.");

            int currentYear = DateTime.Now.Year;
            if (employee.LastResetYear == null || employee.LastResetYear < currentYear)
            {
                employee.VacationRequestsCount = 5;
                employee.LastResetYear = currentYear;
            }
            if (employee.VacationRequestsCount <= 0)
                throw new Exception("You have reached the maximum number of vacation requests (5) for this year.");
            int currentBalance = employee.VacationBalance ?? 0;
            if (currentBalance < requestedDays)
                throw new Exception($"Sorry, your vacation balance ({currentBalance} days) is insufficient for this request ({requestedDays} days).");

            employee.VacationRequestsCount--;
            employee.VacationBalance = currentBalance - requestedDays;
            await _employeeRepo.UpdateVacationRequestsCountAsync(employee.EmployeeSsn, employee.VacationRequestsCount, employee.LastResetYear.Value);
            return await _vacationRepo.AddVacationRequestAsync(vacationRequest);
        }
        public async Task<bool> RespondToSwapRequestAsync(int swapId, string status)
        {
            var swapRequest = await _swapRepo.GetSwapRequestByIdAsync(swapId);
            if (swapRequest == null) return false;

            if (!Enum.TryParse<SwapRequestStatus>(status, out var parsedStatus))
                throw new Exception($"Invalid status value: {status}");

            swapRequest.Status = parsedStatus;
            return await _swapRepo.UpdateSwapRequestAsync(swapRequest);
        }
        public async Task<bool> UpdateProfilePictureAsync(string ssn, string fileName)
        {
            return await _employeeRepo.UpdateProfilePictureAsync(ssn, fileName);
        }
    }
}