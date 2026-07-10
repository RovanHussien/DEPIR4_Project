using DEPI.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Interfaces
{
    public interface IScheduleRepo
    {

        Task<IEnumerable<Schedule>> GetScheduleByEmployeeSsnAsync(string empSsn);

        // Gets the employee's schedule/attendance row for today (null if not created yet)
        Task<Schedule?> GetTodayScheduleByEmployeeSsnAsync(string empSsn);

        Task AddScheduleAsync(Schedule schedule);
    }
}
