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
        public DateTime ScheduleDate { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime TimeOut { get; set; }
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
    }
}