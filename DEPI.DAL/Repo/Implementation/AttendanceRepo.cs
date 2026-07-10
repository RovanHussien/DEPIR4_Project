using DEPI.DAL.DbContext;
using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Implementation
{
    public class AttendanceRepo : IAttendanceRepo
    {
        private readonly ApplicationDbContext _context;

        public AttendanceRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Attendance?> GetTodayAttendanceAsync(string employeeSsn)
        {
            var today = DateTime.Today;
            return await _context.Attendances
                .Include(a => a.Employee)
                .Include(a => a.Schedule).ThenInclude(s => s.Shift)
                .FirstOrDefaultAsync(a => a.EmployeeSsn == employeeSsn && a.Date == today);
        }

        public async Task<List<Attendance>> GetAttendanceByEmployeeAsync(string employeeSsn, DateTime? from, DateTime? to)
        {
            var query = _context.Attendances
                .Include(a => a.Employee)
                .Include(a => a.Schedule).ThenInclude(s => s.Shift)
                .Where(a => a.EmployeeSsn == employeeSsn);

            if (from.HasValue)
                query = query.Where(a => a.Date >= from.Value.Date);
            if (to.HasValue)
                query = query.Where(a => a.Date <= to.Value.Date);

            return await query.OrderByDescending(a => a.Date).ToListAsync();
        }

        public async Task<List<Attendance>> GetAllAttendanceByDateAsync(DateTime date)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Include(a => a.Schedule).ThenInclude(s => s.Shift)
                .Where(a => a.Date == date.Date)
                .OrderBy(a => a.Employee.FirstName)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetAttendanceByEmployeeSsnsAsync(List<string> employeeSsns, DateTime? date)
        {
            var query = _context.Attendances
                .Include(a => a.Employee)
                .Include(a => a.Schedule).ThenInclude(s => s.Shift)
                .Where(a => employeeSsns.Contains(a.EmployeeSsn));

            if (date.HasValue)
                query = query.Where(a => a.Date == date.Value.Date);

            return await query.OrderByDescending(a => a.Date)
                .ThenBy(a => a.Employee.FirstName)
                .ToListAsync();
        }

        public async Task AddAttendanceAsync(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAttendanceAsync(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task<Employee?> GetEmployeeByFingerprintIdAsync(string fingerprintId)
        {
            return await _context.Employees
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.FingerprintId == fingerprintId);
        }

        public async Task<List<Employee>> GetEmployeesWithNoCheckInTodayAsync()
        {
            var today = DateTime.Today;

            var employeesWithShift = await _context.Employees
                .Include(e => e.Shift)
                .Where(e => e.ShiftId != null)
                .ToListAsync();

            var checkedInSsns = await _context.Attendances
                .Where(a => a.Date == today)
                .Select(a => a.EmployeeSsn)
                .ToListAsync();

            return employeesWithShift
                .Where(e => !checkedInSsns.Contains(e.EmployeeSsn))
                .ToList();
        }
    }
}
