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

        // navigation property for employees
        public List<Employee> Employees { get; set; }

        // navigation property for schedule
        public List<Schedule> Schedules { get; set; }
    }
}
