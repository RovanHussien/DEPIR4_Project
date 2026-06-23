using DEPI.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IAdminService
    {
        Task<List<EmployeeStatusDto>> GetPendingEmployeesAsync();
        Task<List<EmployeeStatusDto>> ApprovedEmployeeAsync();
        Task<List<EmployeeStatusDto>> RejectedEmployeeAsync();
        Task<EmployeeStatusDto> GetPendingEmployeeDetailsAsync(string userId);
        Task<bool> CompleteEmployeeApprovalAsync(string userId, AdminApprovalDto approvalDto);
        Task<bool> ApproveEmployeeAsync(string Email);
        Task<bool> RejectEmployeeAsync(string Email);
        Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync();
        Task<List<UserManagementDto>> GetAllUsersGroupedByDepartmentAsync();
        Task<UserManagementDto> AddUserAsync(UserManagementDto userDto, string password);
        Task<UserManagementDto> UpdateUserAsync(string userId, UserManagementDto userDto);
        Task<bool> DeactivateUserAsync(string userId);
    }
}

