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
        private readonly IEmailService _emailService;

        public ManagerService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        private List<string> GetEmployeeSsnsInDepartment(int departmentId)
        {
            var productionLineIds = _context.ProductionLines
                .Where(p => p.DepartmentId == departmentId)
                .Select(p => p.ProductionLineId)
                .ToList();

            return _context.Employees
                .Where(e => e.ProductionLineId != null
                       && productionLineIds.Contains(e.ProductionLineId.Value)
                       && e.DefaultRole != "Manager")
                .Select(e => e.EmployeeSsn)
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

        public async Task<(bool Success, string ErrorMessage)> ApproveLeaveRequestAsync(int vacationRequestId, int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            var request = await _context.VacationRequests
                .Include(v => v.Employee)
                    .ThenInclude(e => e.ApplicationUser)
                .FirstOrDefaultAsync(v => v.VacationRequestId == vacationRequestId && ssns.Contains(v.EmployeeSsn));

            if (request == null)
                return (false, "Leave request not found or does not belong to your department.");

            if (request.Status != VacationRequestStatus.Pending)
                return (false, $"This request has already been {request.Status}.");

            request.Status = VacationRequestStatus.Approved;
            await _context.SaveChangesAsync();

            if (request.Employee?.ApplicationUser?.Email != null)
            {
                string subject = "Leave Request Approved";
                string body = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Leave Request Approved</h2>
                    <p>Dear {request.Employee.FirstName},</p>
                    <p>Your leave request from <strong>{request.StartDate:dd MMM yyyy}</strong> to <strong>{request.EndDate:dd MMM yyyy}</strong> has been <strong>approved</strong>.</p>
                    <p>Thank you.</p>
                </div>";
                await _emailService.SendEmailAsync(request.Employee.ApplicationUser.Email, subject, body);
            }

            return (true, null);
        }

        public async Task<(bool Success, string ErrorMessage)> RejectLeaveRequestAsync(int vacationRequestId, int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            var request = await _context.VacationRequests
                .Include(v => v.Employee)
                    .ThenInclude(e => e.ApplicationUser)
                .FirstOrDefaultAsync(v => v.VacationRequestId == vacationRequestId && ssns.Contains(v.EmployeeSsn));

            if (request == null)
                return (false, "Leave request not found or does not belong to your department.");

            if (request.Status != VacationRequestStatus.Pending)
                return (false, $"This request has already been {request.Status}.");

            request.Status = VacationRequestStatus.Rejected;
            await _context.SaveChangesAsync();

            if (request.Employee?.ApplicationUser?.Email != null)
            {
                string subject = "Leave Request Rejected";
                string body = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Leave Request Rejected</h2>
                    <p>Dear {request.Employee.FirstName},</p>
                    <p>Your leave request from <strong>{request.StartDate:dd MMM yyyy}</strong> to <strong>{request.EndDate:dd MMM yyyy}</strong> has been <strong>rejected</strong> by your manager.</p>
                    <p>Please contact your manager for further details.</p>
                </div>";
                await _emailService.SendEmailAsync(request.Employee.ApplicationUser.Email, subject, body);
            }

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
                .ToList()
                .Select(s => new ManagerShiftChangeDto
                {
                    RequestId = s.RequestId,
                    RequestingEmployeeName = s.RequestEmployee != null
                        ? s.RequestEmployee.FirstName + " " + s.RequestEmployee.LastName : "N/A",
                    RecipientEmployeeName = s.RecipientEmployee != null
                        ? s.RecipientEmployee.FirstName + " " + s.RecipientEmployee.LastName : null,
                    ShiftName = s.Schedule?.Shift?.Name,
                    ScheduleDate = s.Schedule?.ScheduleDate,
                    Status = s.Status.ToString()
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
                    Status =
                    DateTime.Today < m.StartDate.Date ? "Scheduled" :
                    DateTime.Today > m.EndDate.Date ? "Completed" :
                    "Active"
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

            var employee = _context.Employees
                .Include(e => e.Shift)
                .FirstOrDefault(e => e.EmployeeSsn == dto.GoesOnEmployeeSsn);

            if (employee == null)
                return (false, "Employee not found.");

            var mission = new Mission
            {
                GoesOnEmployeeSsn = dto.GoesOnEmployeeSsn,
                AuthorizedEmployeeSsn = manager.EmployeeSsn,
                Purpose = dto.Purpose,
                Destination = dto.Destination,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = MissionStatus.Active
            };

            _context.Missions.Add(mission);
            _context.SaveChanges();

            var currentDate = dto.StartDate.Date;
            var endDate = dto.EndDate.Date;

            var existingSchedules = _context.Schedules
                .Where(s => s.EmployeeSsn == dto.GoesOnEmployeeSsn && s.ScheduleDate >= currentDate && s.ScheduleDate <= endDate)
                .ToList();

            var newSchedules = new List<Schedule>();

            while (currentDate <= endDate)
            {
                var existingSchedule = existingSchedules.FirstOrDefault(s => s.ScheduleDate.Date == currentDate);
                if (existingSchedule != null)
                {
                    existingSchedule.MissionId = mission.MissionId;
                    existingSchedule.ScheduleName = $"Mission - {dto.Purpose} ({currentDate:dd/MM/yyyy})";
                }
                else
                {
                    newSchedules.Add(new Schedule
                    {
                        ScheduleName = $"Mission - {dto.Purpose} ({currentDate:dd/MM/yyyy})",
                        ScheduleDate = currentDate,
                        EmployeeSsn = dto.GoesOnEmployeeSsn,
                        ShiftId = employee.ShiftId,
                        ProductionLineId = employee.ProductionLineId,
                        MissionId = mission.MissionId,
                    });
                }
                currentDate = currentDate.AddDays(1);
            }

            if (newSchedules.Any())
                _context.Schedules.AddRange(newSchedules);

            _context.SaveChanges();
            return (true, null);
        }

        public List<ManagerAttendanceDto> GetDepartmentAttendance(int departmentId, DateTime? date)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);
            var today = DateTime.Today;

            var todaySchedules = _context.Schedules
                .Include(s => s.Shift)
                .Where(s => ssns.Contains(s.EmployeeSsn)
                && s.ScheduleDate.Date == today
                && s.MissionId != null)
            .ToList();

            foreach (var schedule in todaySchedules)
            {
                bool exists = _context.Attendances
                    .Any(a => a.ScheduleId == schedule.ScheduleId);

                if (!exists)
                {
                    DateTime timeIn;
                    DateTime timeOut;

                    if (schedule.Shift != null)
                    {
                        timeIn = schedule.ScheduleDate.Date + schedule.Shift.StartTime.TimeOfDay;
                        timeOut = schedule.ScheduleDate.Date + schedule.Shift.EndTime.TimeOfDay;

                        if (timeOut <= timeIn)
                            timeOut = timeOut.AddDays(1);
                    }
                    else
                    {
                        timeIn = schedule.ScheduleDate.Date.AddHours(8);
                        timeOut = schedule.ScheduleDate.Date.AddHours(16);
                    }

                    _context.Attendances.Add(new Attendance
                    {
                        ScheduleId = schedule.ScheduleId,
                        CheckInTime = timeIn,
                        CheckOutTime = timeOut
                    });
                }
            }

            _context.SaveChanges();
            var query = _context.Attendances
                .Include(a => a.Employee)
                .Include(a => a.Schedule).ThenInclude(s => s.Shift)
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
                    ShiftName = (a.Schedule != null && a.Schedule.Shift != null) ? a.Schedule.Shift.Name : "N/A",
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
    ssns.Contains(s.RequestingEmployeeId) &&
    s.Status == SwapRequestStatus.RecipientApproved),
                ActiveMissionsCount = _context.Missions.Count(m =>
                    ssns.Contains(m.GoesOnEmployeeSsn) &&
                    m.Status != MissionStatus.Completed && m.Status != MissionStatus.Cancelled),
                ProductionLinesCount = _context.ProductionLines.Count(p => p.DepartmentId == departmentId),
                TodaySchedulesCount = _context.Schedules.Count(s =>
                    ssns.Contains(s.EmployeeSsn) &&
                    s.ScheduleDate.Date == DateTime.Today.Date &&
                    s.ShiftId != null)
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
        public List<ManagerShiftDto> GetAvailableShifts()
        {
            return _context.Shifts
                .Select(s => new ManagerShiftDto
                {
                    ShiftId = s.ShiftId,
                    Name = s.Name,
                    StartTime = s.StartTime.TimeOfDay,
                    EndTime = s.EndTime.TimeOfDay
                })
                .ToList();
        }

        public List<EmployeeScheduleDto> GetDepartmentSchedules(int departmentId, DateTime? date)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);

            var query = _context.Schedules
                .Include(s => s.Employee)
                .Include(s => s.Shift)
                .Where(s => ssns.Contains(s.EmployeeSsn) && s.ShiftId != null);

            if (date.HasValue)
                query = query.Where(s => s.ScheduleDate.Date == date.Value.Date);

            return query
                .OrderBy(s => s.ScheduleDate)
                .ThenBy(s => s.Shift.StartTime)
                .Select(s => new EmployeeScheduleDto
                {
                    ScheduleId = s.ScheduleId,
                    EmployeeName = s.Employee.FirstName + " " + s.Employee.LastName,
                    EmployeeSsn = s.EmployeeSsn,
                    ScheduleDate = s.ScheduleDate,
                    ShiftName = s.Shift.Name,
                    ShiftStart = s.Shift.StartTime.TimeOfDay,
                    ShiftEnd = s.Shift.EndTime.TimeOfDay
                })
                .ToList();
        }

        public (bool Success, string ErrorMessage) AssignShiftToEmployee(AssignShiftDto dto, int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);
            if (!ssns.Contains(dto.EmployeeSsn))
                return (false, "Employee does not belong to your department.");

            var newShift = _context.Shifts.Find(dto.ShiftId);
            if (newShift == null)
                return (false, "Shift not found.");

            if (dto.EndDate.Date < dto.StartDate.Date)
                return (false, "End date cannot be before start date.");

            var newStart = newShift.StartTime.TimeOfDay;
            var newEnd = newShift.EndTime.TimeOfDay;

            var currentDate = dto.StartDate.Date;
            while (currentDate <= dto.EndDate.Date)
            {
                var existingSchedules = _context.Schedules
                    .Include(s => s.Shift)
                    .Where(s => s.EmployeeSsn == dto.EmployeeSsn
                             && s.ScheduleDate.Date == currentDate
                             && s.ShiftId != null)
                    .ToList();

                foreach (var existing in existingSchedules)
                {
                    var existStart = existing.Shift.StartTime.TimeOfDay;
                    var existEnd = existing.Shift.EndTime.TimeOfDay;

                    if (newStart < existEnd && newEnd > existStart)
                        return (false, $"Conflict on {currentDate:dd/MM/yyyy}: Employee already has " +
                                      $"'{existing.Shift.Name}' ({existStart:hh\\:mm} - {existEnd:hh\\:mm}).");
                }
                currentDate = currentDate.AddDays(1);
            }

            currentDate = dto.StartDate.Date;
            int count = 0;
            while (currentDate <= dto.EndDate.Date)
            {
                _context.Schedules.Add(new Schedule
                {
                    ScheduleName = $"{currentDate:dd/MM/yyyy} - {newShift.Name}",
                    ScheduleDate = currentDate,
                    EmployeeSsn = dto.EmployeeSsn,
                    ShiftId = dto.ShiftId
                });
                currentDate = currentDate.AddDays(1);
                count++;
            }

            _context.SaveChanges();
            return (true, $"Shift assigned for {count} day(s) successfully.");
        }
        public bool RemoveSchedule(int scheduleId, int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);
            var schedule = _context.Schedules.Find(scheduleId);

            if (schedule == null || !ssns.Contains(schedule.EmployeeSsn))
                return false;

            _context.Schedules.Remove(schedule);
            _context.SaveChanges();
            return true;
        }

        public async Task<(bool Success, string ErrorMessage)> ExecuteSwapAsync(int requestId, int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);
            var request = await _context.SwapRequests
                .Include(r => r.Schedule)
                .Include(r => r.RequestEmployee)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.RecipientEmployee)
                    .ThenInclude(e => e.ApplicationUser)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null || !ssns.Contains(request.RequestingEmployeeId))
                return (false, "Swap request not found.");

            if (request.Status == SwapRequestStatus.PendingRecipient)
                return (false, "Waiting for recipient response.");
            if (request.Status == SwapRequestStatus.RecipientRejected)
                return (false, "The recipient has already rejected this request.");
            if (request.Status == SwapRequestStatus.FinalApproved)
                return (false, "This swap has already been executed.");
            if (request.Status == SwapRequestStatus.FinalRejected)
                return (false, "This request has already been rejected.");

            var requestingSchedule = request.Schedule;
            if (requestingSchedule == null)
                return (false, "No schedule found for this request.");
            if (!ssns.Contains(request.RecipientEmployeeId))
                return (false, "Recipient is not in your department.");

            var recipientSchedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.EmployeeSsn == request.RecipientEmployeeId
                                  && s.ScheduleDate.Date == requestingSchedule.ScheduleDate.Date
                                  && s.ShiftId != null);

            if (recipientSchedule == null)
                return (false, "Recipient has no shift scheduled on that date to swap with.");

            var tempShiftId = requestingSchedule.ShiftId;
            requestingSchedule.ShiftId = recipientSchedule.ShiftId;
            recipientSchedule.ShiftId = tempShiftId;

            request.Status = SwapRequestStatus.FinalApproved;
            await _context.SaveChangesAsync();

            // Send Email to Requesting Employee
            if (request.RequestEmployee?.ApplicationUser?.Email != null)
            {
                string subject = "Shift Swap Approved";
                string body = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Shift Swap Approved</h2>
                    <p>Dear {request.RequestEmployee.FirstName},</p>
                    <p>Your shift swap request on <strong>{requestingSchedule.ScheduleDate:dd MMM yyyy}</strong> with {request.RecipientEmployee?.FirstName} has been <strong>approved</strong> by your manager.</p>
                </div>";
                await _emailService.SendEmailAsync(request.RequestEmployee.ApplicationUser.Email, subject, body);
            }

            // Send Email to Recipient Employee
            if (request.RecipientEmployee?.ApplicationUser?.Email != null)
            {
                string subject = "Shift Swap Approved";
                string body = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Shift Swap Approved</h2>
                    <p>Dear {request.RecipientEmployee.FirstName},</p>
                    <p>The shift swap request on <strong>{requestingSchedule.ScheduleDate:dd MMM yyyy}</strong> with {request.RequestEmployee?.FirstName} has been <strong>approved</strong> by your manager.</p>
                </div>";
                await _emailService.SendEmailAsync(request.RecipientEmployee.ApplicationUser.Email, subject, body);
            }

            return (true, "Shift swap executed successfully.");
        }

        public async Task<(bool Success, string ErrorMessage)> RejectSwapAsync(int requestId, int departmentId)
        {
            var ssns = GetEmployeeSsnsInDepartment(departmentId);
            var request = await _context.SwapRequests
                .Include(r => r.Schedule)
                .Include(r => r.RequestEmployee)
                    .ThenInclude(e => e.ApplicationUser)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null || !ssns.Contains(request.RequestingEmployeeId))
                return (false, "Swap request not found.");

            if (request.Status == SwapRequestStatus.PendingRecipient)
                return (false, "Waiting for recipient response.");
            if (request.Status == SwapRequestStatus.RecipientRejected)
                return (false, "The recipient has already rejected this request.");
            if (request.Status == SwapRequestStatus.FinalApproved)
                return (false, "This swap has already been executed.");
            if (request.Status == SwapRequestStatus.FinalRejected)
                return (false, "This request has already been rejected.");

            request.Status = SwapRequestStatus.FinalRejected;
            await _context.SaveChangesAsync();

            if (request.RequestEmployee?.ApplicationUser?.Email != null)
            {
                string subject = "Shift Swap Rejected";
                string body = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>Shift Swap Rejected</h2>
                    <p>Dear {request.RequestEmployee.FirstName},</p>
                    <p>Your shift swap request on <strong>{request.Schedule?.ScheduleDate:dd MMM yyyy}</strong> has been <strong>rejected</strong> by your manager.</p>
                </div>";
                await _emailService.SendEmailAsync(request.RequestEmployee.ApplicationUser.Email, subject, body);
            }

            return (true, null);
        }
        public List<EmployeeScheduleRangeDto> GetDepartmentScheduleRanges(int departmentId, DateTime? date)
        {
            var flatSchedules = GetDepartmentSchedules(departmentId, date);

            var result = new List<EmployeeScheduleRangeDto>();

            var groups = flatSchedules
                .GroupBy(s => new { s.EmployeeSsn, s.ShiftName, s.ShiftStart, s.ShiftEnd })
                .OrderBy(g => g.Key.EmployeeSsn);

            foreach (var group in groups)
            {
                var ordered = group.OrderBy(s => s.ScheduleDate).ToList();

                List<EmployeeScheduleDto> currentRange = new List<EmployeeScheduleDto>();

                foreach (var item in ordered)
                {
                    if (currentRange.Any() &&
                        item.ScheduleDate.Date != currentRange.Last().ScheduleDate.Date.AddDays(1))
                    {
                        result.Add(BuildRangeDto(currentRange));
                        currentRange = new List<EmployeeScheduleDto>();
                    }
                    currentRange.Add(item);
                }

                if (currentRange.Any())
                    result.Add(BuildRangeDto(currentRange));
            }

            return result.OrderBy(r => r.StartDate).ToList();
        }

        private EmployeeScheduleRangeDto BuildRangeDto(List<EmployeeScheduleDto> range)
        {
            var first = range.First();
            return new EmployeeScheduleRangeDto
            {
                EmployeeName = first.EmployeeName,
                EmployeeSsn = first.EmployeeSsn,
                ShiftName = first.ShiftName,
                ShiftStart = first.ShiftStart,
                ShiftEnd = first.ShiftEnd,
                StartDate = range.First().ScheduleDate,
                EndDate = range.Last().ScheduleDate,
                ScheduleIds = range.Select(r => r.ScheduleId).ToList()
            };
        }
    }
}