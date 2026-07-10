using DEPI.DAL.Enums;
using System;

namespace DEPI.BLL.DTO
{
    public class ManagerEmployeeDto
    {
        public string EmployeeSsn { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string JobTitle { get; set; }
    }

    public class ManagerLeaveRequestDto
    {
        public int VacationRequestId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
    }

    public class ManagerShiftChangeDto
    {
        public int RequestId { get; set; }
        public string RequestingEmployeeName { get; set; }
        public string RecipientEmployeeName { get; set; }
        public string ShiftName { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string Status { get; set; }
    }

    public class ManagerMissionDto
    {
        public int MissionId { get; set; }
        public string EmployeeName { get; set; }
        public string Purpose { get; set; }
        public string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
    }

    public class ManagerMissionCreateDto
    {
        public string GoesOnEmployeeSsn { get; set; }
        public string Purpose { get; set; }
        public string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ManagerAttendanceDto
    {
        public int AttendanceId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; }
        public string StatusBadgeClass { get; set; }
        public string ShiftName { get; set; }
        public string Notes { get; set; }
    }

    public class ManagerProductionLineDto
    {
        public int ProductionLineId { get; set; }
        public string Name { get; set; }
    }

    public class ManagerDashboardDto
    {
        public string DepartmentName { get; set; }
        public int EmployeesCount { get; set; }
        public int PendingLeavesCount { get; set; }
        public int ShiftChangeRequestsCount { get; set; }
        public int ActiveMissionsCount { get; set; }
        public int ProductionLinesCount { get; set; }
        public int TodaySchedulesCount { get; set; }  
    }
    public class ManagerProfileDto
    {
        public string EmployeeSsn { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public string DepartmentName { get; set; }
    }
    public class ManagerProfileEditDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public string CurrentPassword { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile ProfileImage { get; set; } // ? ÌÏíÏ
    }
    public class AssignShiftDto
    {
        public string EmployeeSsn { get; set; }
        public int ShiftId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class EmployeeScheduleDto
    {
        public int ScheduleId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeSsn { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan ShiftStart { get; set; }
        public TimeSpan ShiftEnd { get; set; }
    }

    public class ManagerShiftDto
    {
        public int ShiftId { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
    public class EmployeeScheduleRangeDto
    {
        public string EmployeeName { get; set; }
        public string EmployeeSsn { get; set; }
        public string ShiftName { get; set; }
        public TimeSpan ShiftStart { get; set; }
        public TimeSpan ShiftEnd { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> ScheduleIds { get; set; } = new List<int>();
        public Microsoft.AspNetCore.Http.IFormFile ProfileImage { get; set; } // ? ÌÏíÏ
    }
}