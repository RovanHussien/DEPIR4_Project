using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DEPI.BLL.DTO;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IManagerService
    {
        int? GetManagerDepartmentId(string applicationUserId);
        string GetManagerDepartmentName(int departmentId);

        List<ManagerEmployeeDto> GetDepartmentEmployees(int departmentId);

        List<ManagerLeaveRequestDto> GetDepartmentLeaveRequests(int departmentId);
        (bool Success, string ErrorMessage) ApproveLeaveRequest(int vacationRequestId, int departmentId);
        (bool Success, string ErrorMessage) RejectLeaveRequest(int vacationRequestId, int departmentId);

        List<ManagerShiftChangeDto> GetDepartmentShiftChanges(int departmentId);

        List<ManagerMissionDto> GetDepartmentMissions(int departmentId);
        (bool Success, string ErrorMessage) AssignMission(ManagerMissionCreateDto dto, string applicationUserId, int departmentId);

        List<ManagerAttendanceDto> GetDepartmentAttendance(int departmentId, DateTime? date);

        List<ManagerProductionLineDto> GetDepartmentProductionLines(int departmentId);

        ManagerDashboardDto GetDashboardSummary(int departmentId);

        ManagerProfileDto GetManagerProfile(string applicationUserId);
        Task<(bool Success, string ErrorMessage)> UpdateManagerProfileAsync(string applicationUserId, ManagerProfileEditDto dto);
        List<ManagerShiftDto> GetAvailableShifts();
        List<EmployeeScheduleDto> GetDepartmentSchedules(int departmentId, DateTime? date);
        (bool Success, string ErrorMessage) AssignShiftToEmployee(AssignShiftDto dto, int departmentId);
        bool RemoveSchedule(int scheduleId, int departmentId);
        (bool Success, string ErrorMessage) ExecuteSwap(int requestId, int departmentId);
        (bool Success, string ErrorMessage) RejectSwap(int requestId, int departmentId);
        List<EmployeeScheduleRangeDto> GetDepartmentScheduleRanges(int departmentId, DateTime? date);
    }
}