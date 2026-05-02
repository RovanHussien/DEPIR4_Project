using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
       public DateTime TimeIn { get; set; }
        public DateTime TimeOut { get; set; }

        // navigation property for schedule
        public Schedule? Schedule { get; set; }
        public int? ScheduleId { get; set; }
    }
}
