using DEPI.DAL.DbContext;
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
                .ToListAsync();
        }
    }
}
