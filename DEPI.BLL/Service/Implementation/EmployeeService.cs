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
        

        public async Task<bool> CreateSwapRequestAsync(int scheduleId, string requestingEmpSsn, string recipientEmpSsn)
        {
            var swapRequest = new SwapRequest
            {
                ScheduleId = scheduleId,
                RequestingEmployeeId = requestingEmpSsn,
                RecipientEmployeeId = recipientEmpSsn
            };

            return await _swapRepo.AddSwapRequestAsync(swapRequest);
        }

        
        public async Task<bool> ApplyForVacationAsync(VacationRequest vacationRequest)
        {
            if (vacationRequest == null) return false;

           
            if (vacationRequest.StartDate < DateTime.Now.Date)
            {
                throw new Exception("Cannot request a vacation with a past date!");
            }

            
            int requestedDays = (vacationRequest.EndDate - vacationRequest.StartDate).Days + 1;
            if (requestedDays <= 0)
            {
                throw new Exception("End date must be after the start date!");
            }

          
            var employee = await _employeeRepo.GetEmployeeById(vacationRequest.EmployeeSsn);
            if (employee == null)
            {
                throw new Exception("Employee not found!");
            }
            int currentBalance = employee.VacationBalance ?? 0; 
            if (currentBalance < requestedDays)
            {
                throw new Exception($"Sorry, your vacation balance ({currentBalance} days) is insufficient for this request ({requestedDays} days).");
            }


            
            return await _vacationRepo.AddVacationRequestAsync(vacationRequest);
        }
        public async Task<bool> RespondToSwapRequestAsync(int swapId, string status)
        {
            var swapRequest = await _swapRepo.GetSwapRequestByIdAsync(swapId);
            if (swapRequest == null) return false;
            swapRequest.Status = status;
            return await _swapRepo.UpdateSwapRequestAsync(swapRequest);
        }
    }
}



