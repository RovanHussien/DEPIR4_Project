using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public class Shift
    {
        public int ShiftId { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<Employee> Employees { get; set; }

        public List<Schedule> Schedules { get; set; }
    }
}
