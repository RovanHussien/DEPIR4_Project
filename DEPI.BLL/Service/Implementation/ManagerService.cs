using System;
using System.Collections.Generic;
using System.Linq;
using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.DbContext;
using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DEPI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DEPI.BLL.Service.Implementation
{
    public class ManagerService : IManagerService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagerService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<string> GetEmployeeSsnsInDepartment(int departmentId)
        {
            return _context.EmployeeDepartments
                .Where(ed => ed.DepartmentID == departmentId)
                .Select(ed => ed.EmployeeSsn)
                .ToList();
        }

        public int? GetManagerDepartmentId(string applicationUserId)
        {
            var employee = _context.Employees
                .FirstOrDefault(e => e.UserId == applicationUserId);

            if (employee == null) return null;

            return _context.Departments
                .Where(d => d.ManagerSsn == employee.EmployeeSsn)
                .Select(d => (int?)d.DepartmentId)
                .FirstOrDefault();
        }

        public string GetManagerDepartmentName(int departmentId)
        {
            return _context.Departments
                .Where(d => d.DepartmentId == departmentId)
                .Select(d => d.Name)
                .FirstOrDefault();
        }

        public List<ManagerEmployeeDto> GetDepartmentEmployees(int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            return _context.Employees
                .Include(e => e.ApplicationUser)
                .Where(e => ssns.Contains(e.EmployeeSsn))
                .Select(e => new ManagerEmployeeDto
                {
                    EmployeeSsn = e.EmployeeSsn,
                    FullName = e.FirstName + " " + e.LastName,
                    Email = e.ApplicationUser != null ? e.ApplicationUser.Email : null,
                    JobTitle = e.DefaultRole
                })
                .ToList();
        }

        public List<ManagerLeaveRequestDto> GetDepartmentLeaveRequests(int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            return _context.VacationRequests
                .Include(v => v.Employee)
                .Where(v => ssns.Contains(v.EmployeeSsn))
                .OrderByDescending(v => v.StartDate)
                .Select(v => new ManagerLeaveRequestDto
                {
                    VacationRequestId = v.VacationRequestId,
                    EmployeeName = v.Employee.FirstName + " " + v.Employee.LastName,
                    StartDate = v.StartDate,
                    EndDate = v.EndDate,
                    Reason = v.Reason,
                    Status = v.Status.ToString()
                })
                .ToList();
        }

        public (bool Success, string ErrorMessage) ApproveLeaveRequest(int vacationRequestId, int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            var request = _context.VacationRequests
                .FirstOrDefault(v => v.VacationRequestId == vacationRequestId && ssns.Contains(v.EmployeeSsn));

            if (request == null)
                return (false, "Leave request not found or does not belong to your department.");

            if (request.Status != VacationRequestStatus.Pending)
                return (false, $"This request has already been {request.Status}.");

            request.Status = VacationRequestStatus.Approved;
            _context.SaveChanges();
            return (true, null);
        }

        public (bool Success, string ErrorMessage) RejectLeaveRequest(int vacationRequestId, int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            var request = _context.VacationRequests
                .FirstOrDefault(v => v.VacationRequestId == vacationRequestId && ssns.Contains(v.EmployeeSsn));

            if (request == null)
                return (false, "Leave request not found or does not belong to your department.");

            if (request.Status != VacationRequestStatus.Pending)
                return (false, $"This request has already been {request.Status}.");

            request.Status = VacationRequestStatus.Rejected;
            _context.SaveChanges();
            return (true, null);
        }

        public List<ManagerShiftChangeDto> GetDepartmentShiftChanges(int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            return _context.SwapRequests
                .Include(s => s.RequestEmployee)
                .Include(s => s.RecipientEmployee)
                .Include(s => s.Schedule).ThenInclude(sc => sc.Shift)
                .Where(s => ssns.Contains(s.RequestingEmployeeId))
                .Select(s => new ManagerShiftChangeDto
                {
                    RequestId = s.RequestId,
                    RequestingEmployeeName = s.RequestEmployee.FirstName + " " + s.RequestEmployee.LastName,
                    RecipientEmployeeName = s.RecipientEmployee != null
                        ? s.RecipientEmployee.FirstName + " " + s.RecipientEmployee.LastName
                        : null,
                    ShiftName = s.Schedule != null && s.Schedule.Shift != null ? s.Schedule.Shift.Name : null,
                    ScheduleDate = s.Schedule != null ? s.Schedule.ScheduleDate : (DateTime?)null
                })
                .ToList();
        }

        public List<ManagerMissionDto> GetDepartmentMissions(int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            return _context.Missions
                .Include(m => m.GoesOnEmployee)
                .Where(m => ssns.Contains(m.GoesOnEmployeeSsn))
                .OrderByDescending(m => m.StartDate)
                .Select(m => new ManagerMissionDto
                {
                    MissionId = m.MissionId,
                    EmployeeName = m.GoesOnEmployee.FirstName + " " + m.GoesOnEmployee.LastName,
                    Purpose = m.Purpose,
                    Destination = m.Destination,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Status = m.Status.ToString()
                })
                .ToList();
        }

        public (bool Success, string ErrorMessage) AssignMission(ManagerMissionCreateDto dto, string applicationUserId, int departmentId)
        {
            var manager = _context.Employees.FirstOrDefault(e => e.UserId == applicationUserId);
            if (manager == null)
                return (false, "Manager record not found.");

            var ssns = GetEmployeeSsnsInDepartment(departmentId);
            if (!ssns.Contains(dto.GoesOnEmployeeSsn))
                return (false, "Selected employee does not belong to your department.");

            if (dto.EndDate < dto.StartDate)
                return (false, "End date cannot be before the start date.");

            var mission = new Mission
            {
                GoesOnEmployeeSsn = dto.GoesOnEmployeeSsn,
                AuthorizedEmployeeSsn = manager.EmployeeSsn,
                Purpose = dto.Purpose,
                Destination = dto.Destination,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = MissionStatus.Pending
            };

            _context.Missions.Add(mission);
            _context.SaveChanges();
            return (true, null);
        }

        public List<ManagerAttendanceDto> GetDepartmentAttendance(int departmentId, DateTime? date)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            var query = _context.Attendances
                .Include(a => a.Employee)
                .Include(a => a.Shift)
                .Where(a => ssns.Contains(a.EmployeeSsn));

            if (date.HasValue)
                query = query.Where(a => a.Date == date.Value.Date);

            return query
                .OrderByDescending(a => a.Date)
                .Select(a => new ManagerAttendanceDto
                {
                    AttendanceId = a.AttendanceId,
                    EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                    Date = a.Date,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    Status = a.Status.ToString(),
                    StatusBadgeClass = a.Status == DAL.Enums.AttendanceStatus.Present ? "bg-success" :
                                       a.Status == DAL.Enums.AttendanceStatus.Late ? "bg-warning text-dark" :
                                       a.Status == DAL.Enums.AttendanceStatus.Absent ? "bg-danger" : "bg-info",
                    ShiftName = a.Shift != null ? a.Shift.Name : "N/A",
                    Notes = a.Notes
                })
                .ToList();
        }

        public List<ManagerProductionLineDto> GetDepartmentProductionLines(int departmentId)
        {
            return _context.ProductionLines
                .Where(p => p.DepartmentId == departmentId)
                .Select(p => new ManagerProductionLineDto
                {
                    ProductionLineId = p.ProductionLineId,
                    Name = p.Name
                })
                .ToList();
        }

        public ManagerDashboardDto GetDashboardSummary(int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            return new ManagerDashboardDto
            {
                DepartmentName = GetManagerDepartmentName(departmentId),
                EmployeesCount = ssns.Count,
                PendingLeavesCount = _context.VacationRequests.Count(v =>
                    ssns.Contains(v.EmployeeSsn) && v.Status == VacationRequestStatus.Pending),
                ShiftChangeRequestsCount = _context.SwapRequests.Count(s =>
                    ssns.Contains(s.RequestingEmployeeId)),
                ActiveMissionsCount = _context.Missions.Count(m =>
                    ssns.Contains(m.GoesOnEmployeeSsn) &&
                    m.Status != MissionStatus.Completed && m.Status != MissionStatus.Cancelled),
                ProductionLinesCount = _context.ProductionLines.Count(p => p.DepartmentId == departmentId)
            };
        }
        public ManagerProfileDto GetManagerProfile(string applicationUserId)
        {
            var employee = _context.Employees
                .Include(e => e.ApplicationUser)
                .FirstOrDefault(e => e.UserId == applicationUserId);

            if (employee == null) return null;

            var departmentName = _context.Departments
                .Where(d => d.ManagerSsn == employee.EmployeeSsn)
                .Select(d => d.Name)
                .FirstOrDefault();

            return new ManagerProfileDto
            {
                EmployeeSsn = employee.EmployeeSsn,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                FullName = employee.FirstName + " " + employee.LastName,
                Email = employee.ApplicationUser != null ? employee.ApplicationUser.Email : null,
                PhoneNumber = employee.PhoneNumber.ToString(),
                Address = employee.Address,
                BirthDate = employee.BirthDate,
                DepartmentName = departmentName
            };
        }
        public async Task<(bool Success, string ErrorMessage)> UpdateManagerProfileAsync(string applicationUserId, ManagerProfileEditDto dto)
        {
            var user = await _userManager.FindByIdAsync(applicationUserId);
            if (user == null) return (false, "User not found.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordValid) return (false, "The password you entered is incorrect.");

            var employee = _context.Employees.FirstOrDefault(e => e.UserId == applicationUserId);
            if (employee == null) return (false, "Employee record not found.");

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.Address = dto.Address;
            employee.BirthDate = dto.BirthDate;

            _context.SaveChanges();
            return (true, null);
        }
    }
}