using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.DbContext;
using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using DEPI.DAL.Models;
using DEPI.DAL.Repo.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Implementation
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepo _attendanceRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceService(IAttendanceRepo attendanceRepo, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _attendanceRepo = attendanceRepo;
            _context = context;
            _userManager = userManager;
        }

        public async Task<AttendanceResultDto> RecordCheckInAsync(string fingerprintId)
        {
            var employee = await _attendanceRepo.GetEmployeeByFingerprintIdAsync(fingerprintId);
            if (employee == null)
            {
                return new AttendanceResultDto
                {
                    Success = false,
                    Message = "Fingerprint not recognized. Please contact your administrator."
                };
            }

            var existing = await _attendanceRepo.GetTodayAttendanceAsync(employee.EmployeeSsn);
            if (existing != null && existing.CheckInTime != null)
            {
                return new AttendanceResultDto
                {
                    Success = false,
                    Message = $"Already checked in today at {existing.CheckInTime:hh:mm tt}.",
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    Status = existing.Status.ToString(),
                    Time = existing.CheckInTime
                };
            }

            var now = DateTime.Now;
            var status = AttendanceStatus.Present;
            string notes = null;

            if (employee.Shift != null)
            {
                var shiftStart = DateTime.Today.Add(employee.Shift.StartTime.TimeOfDay);
                if (now > shiftStart)
                {
                    status = AttendanceStatus.Late;
                    var lateMinutes = (int)(now - shiftStart).TotalMinutes;
                    notes = $"Late by {lateMinutes} minutes";
                }
            }

            if (existing != null)
            {
                existing.CheckInTime = now;
                existing.Status = status;
                existing.Notes = notes;
                await _attendanceRepo.UpdateAttendanceAsync(existing);
            }
            else
            {
                var attendance = new Attendance
                {
                    Date = DateTime.Today,
                    CheckInTime = now,
                    Status = status,
                    EmployeeSsn = employee.EmployeeSsn,
                    Notes = notes
                };
                await _attendanceRepo.AddAttendanceAsync(attendance);
            }

            return new AttendanceResultDto
            {
                Success = true,
                Message = status == AttendanceStatus.Present
                    ? "Check-in recorded successfully. Welcome!"
                    : $"Check-in recorded. You are late ({notes}).",
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Status = status.ToString(),
                Time = now
            };
        }

        public async Task<AttendanceResultDto> RecordCheckOutAsync(string fingerprintId)
        {
            var employee = await _attendanceRepo.GetEmployeeByFingerprintIdAsync(fingerprintId);
            if (employee == null)
            {
                return new AttendanceResultDto
                {
                    Success = false,
                    Message = "Fingerprint not recognized. Please contact your administrator."
                };
            }

            var existing = await _attendanceRepo.GetTodayAttendanceAsync(employee.EmployeeSsn);
            if (existing == null || existing.CheckInTime == null)
            {
                return new AttendanceResultDto
                {
                    Success = false,
                    Message = "No check-in record found for today. Cannot check out.",
                    EmployeeName = $"{employee.FirstName} {employee.LastName}"
                };
            }

            if (existing.CheckOutTime != null)
            {
                return new AttendanceResultDto
                {
                    Success = false,
                    Message = $"Already checked out today at {existing.CheckOutTime:hh:mm tt}.",
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    Time = existing.CheckOutTime
                };
            }

            existing.CheckOutTime = DateTime.Now;
            await _attendanceRepo.UpdateAttendanceAsync(existing);

            return new AttendanceResultDto
            {
                Success = true,
                Message = "Check-out recorded successfully. Goodbye!",
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Status = existing.Status.ToString(),
                Time = existing.CheckOutTime
            };
        }

        public async Task MarkAbsenteesAsync()
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            var absentEmployees = await _attendanceRepo.GetEmployeesWithNoCheckInTodayAsync();

            foreach (var employee in absentEmployees)
            {
                if (employee.Shift == null) continue;

                var shiftStart = today.Add(employee.Shift.StartTime.TimeOfDay);
                if (now >= shiftStart.AddMinutes(30))
                {
                    var attendance = new Attendance
                    {
                        Date = today,
                        CheckInTime = null,
                        CheckOutTime = null,
                        Status = AttendanceStatus.Absent,
                        EmployeeSsn = employee.EmployeeSsn,
                        Notes = "Auto-marked as absent (no fingerprint scan within 30 minutes of shift start)"
                    };
                    await _attendanceRepo.AddAttendanceAsync(attendance);
                }
            }
        }

        public async Task<AttendanceSummaryDto> GetAttendanceSummaryByDateAsync(DateTime date)
        {
            var records = await _attendanceRepo.GetAllAttendanceByDateAsync(date);
            var allEmployees = await _context.Employees.Where(e => e.ShiftId != null).CountAsync();

            return new AttendanceSummaryDto
            {
                Date = date,
                TotalEmployees = allEmployees,
                PresentCount = records.Count(r => r.Status == AttendanceStatus.Present),
                LateCount = records.Count(r => r.Status == AttendanceStatus.Late),
                AbsentCount = records.Count(r => r.Status == AttendanceStatus.Absent),
                OnLeaveCount = 0,
                Records = records.Select(MapToDto).ToList()
            };
        }

        public async Task<List<AttendanceRecordDto>> GetAttendanceByEmployeeSsnsAsync(List<string> employeeSsns, DateTime? date)
        {
            var records = await _attendanceRepo.GetAttendanceByEmployeeSsnsAsync(employeeSsns, date);
            return records.Select(MapToDto).ToList();
        }

        public async Task<List<AttendanceRecordDto>> GetManagerAttendanceAsync(DateTime? date)
        {
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var managerUserIds = managerUsers.Select(u => u.Id).ToList();

            var managerSsns = await _context.Employees
                .Where(e => managerUserIds.Contains(e.UserId))
                .Select(e => e.EmployeeSsn)
                .ToListAsync();

            var records = await _attendanceRepo.GetAttendanceByEmployeeSsnsAsync(managerSsns, date);
            return records.Select(MapToDto).ToList();
        }

        public async Task<AttendanceRecordDto?> GetTodayAttendanceForEmployeeAsync(string employeeSsn)
        {
            var record = await _attendanceRepo.GetTodayAttendanceAsync(employeeSsn);
            return record != null ? MapToDto(record) : null;
        }

        private AttendanceRecordDto MapToDto(Attendance a)
        {
            return new AttendanceRecordDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeSsn = a.EmployeeSsn,
                EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : "Unknown",
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Status = a.Status.ToString(),
                StatusBadgeClass = GetBadgeClass(a.Status),
                ShiftName = a.Schedule?.Shift?.Name ?? "N/A",
                Notes = a.Notes
            };
        }

        private string GetBadgeClass(AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Present => "bg-success",
                AttendanceStatus.Late => "bg-warning text-dark",
                AttendanceStatus.Absent => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }
}
