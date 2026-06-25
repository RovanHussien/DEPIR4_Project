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
    }
}
