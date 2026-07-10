using DEPI.DAL.Enums;
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
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }

        // navigation property for employee (direct link)
        public Employee Employee { get; set; }
        public string EmployeeSsn { get; set; }

        // navigation property for shift
        public Shift? Shift { get; set; }
        public int? ShiftId { get; set; }
    }
}
