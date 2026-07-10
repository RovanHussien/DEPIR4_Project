using DEPI.DAL.DbContext;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Implementation
{
    public class ScheduleRepo : IScheduleRepo
    {
        private readonly ApplicationDbContext _context;

        public ScheduleRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Schedule>> GetScheduleByEmployeeSsnAsync(string empSsn)
        {
            return await _context.Schedules
                .Include(s => s.Shift)
                .Include(s => s.ProductionLine)
                .Include(s => s.Mission)
                .Where(s => s.EmployeeSsn == empSsn)
                .OrderByDescending(s => s.ScheduleDate)
                .ToListAsync();
        }

        public async Task<Schedule?> GetTodayScheduleByEmployeeSsnAsync(string empSsn)
        {
            var today = DateTime.Today;
            return await _context.Schedules
                .Include(s => s.Shift)
                .Include(s => s.ProductionLine)
                    .ThenInclude(p => p.Department)
                        .ThenInclude(d => d.Manager)
                .Include(s => s.Mission)
                .FirstOrDefaultAsync(s => s.EmployeeSsn == empSsn && s.ScheduleDate.Date == today);
        }

        public async Task AddScheduleAsync(Schedule schedule)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
        }
    }
}