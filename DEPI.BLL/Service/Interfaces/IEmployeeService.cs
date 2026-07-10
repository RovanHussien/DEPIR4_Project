using DEPI.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IEmployeeService
    {



        Task<Employee> GetEmployeeProfileAsync(string empSsn);

        Task<IEnumerable<Schedule>> GetMyScheduleAsync(string empSsn);
        Task<Schedule?> GetTodayScheduleAsync(string empSsn);
        Task<bool> CreateSwapRequestAsync(int scheduleId, string requestingEmpSsn, string recipientEmpSsn, string reason);
        Task<bool> ApplyForVacationAsync(VacationRequest vacationRequest);
        Task<Employee> GetEmployeeByUserIdAsync(string userId);
        Task<bool> RespondToSwapRequestAsync(int swapId, string Status);
        Task<bool> UpdateProfilePictureAsync(string ssn, string fileName);


    }
}
