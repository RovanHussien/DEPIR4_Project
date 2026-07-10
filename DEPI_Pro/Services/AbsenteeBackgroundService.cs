using DEPI.BLL.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DEPI_Pro.Services
{
 
    public class AbsenteeBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AbsenteeBackgroundService> _logger;

        public AbsenteeBackgroundService(IServiceProvider serviceProvider, ILogger<AbsenteeBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Absentee Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceService>();

                    await attendanceService.MarkAbsenteesAsync();
                    _logger.LogInformation("Absentee check completed at {Time}", DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while marking absentees.");
                }

                // Wait 5 minutes before next check
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("Absentee Background Service stopped.");
        }
    }
}
