using DEPI.DAL.DbContext;
using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.BackgroundServices
{
    /// <summary>
    /// Runs once immediately on app startup, then once every day, and creates
    /// a Schedule row (Status = Absent) for every employee who doesn't already
    /// have one for today's date. This is what powers the "Today" card on the
    /// Employee Profile page.
    /// </summary>
    public class DailyScheduleGeneratorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyScheduleGeneratorService> _logger;

        public DailyScheduleGeneratorService(
            IServiceScopeFactory scopeFactory,
            ILogger<DailyScheduleGeneratorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await GenerateTodaySchedulesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Never let a failed run crash the whole app - just log and retry next cycle.
                    _logger.LogError(ex, "DailyScheduleGeneratorService: failed to generate today's schedules.");
                }

                var delay = GetDelayUntilNextRun();
                await Task.Delay(delay, stoppingToken);
            }
        }

        // Runs at 00:05 every night. If the app was down at midnight, the very
        // next startup (see ExecuteAsync above) will immediately catch up.
        private static TimeSpan GetDelayUntilNextRun()
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1).AddMinutes(5);
            var delay = nextRun - now;
            return delay <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : delay;
        }

        private async Task GenerateTodaySchedulesAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var today = DateTime.Today;

            var employees = await context.Employees
                .Select(e => new { e.EmployeeSsn, e.ShiftId, e.ProductionLineId })
                .ToListAsync(ct);

            var alreadyScheduledSsns = (await context.Schedules
                .Where(s => s.ScheduleDate.Date == today && s.EmployeeSsn != null)
                .Select(s => s.EmployeeSsn)
                .ToListAsync(ct))
                .ToHashSet();

            var newSchedules = employees
                .Where(e => !alreadyScheduledSsns.Contains(e.EmployeeSsn))
                .Select(e => new Schedule
                {
                    ScheduleName = $"Daily Attendance - {today:dd MMM yyyy}",
                    ScheduleDate = today,
                    EmployeeSsn = e.EmployeeSsn,
                    ShiftId = e.ShiftId,
                    ProductionLineId = e.ProductionLineId,
                    Status = AttendanceStatus.Absent
                })
                .ToList();

            if (newSchedules.Count == 0)
            {
                _logger.LogInformation("DailyScheduleGeneratorService: all employees already have a schedule for {Date}.", today.ToShortDateString());
                return;
            }

            context.Schedules.AddRange(newSchedules);
            await context.SaveChangesAsync(ct);

            _logger.LogInformation("DailyScheduleGeneratorService: created {Count} schedule rows for {Date}.", newSchedules.Count, today.ToShortDateString());
        }
    }
}