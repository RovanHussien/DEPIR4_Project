using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public class Schedule
    {
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        public DateTime ScheduleDate { get; set; }

        // navigation property for employee
        public Employee Employee { get; set; }
        public string? EmployeeSsn { get; set; }

        // navigation property for mission
        public Mission Mission { get; set; }
        public int? MissionId { get; set; }

        // navigation property for shift
        public Shift Shift { get; set; }
        public int? ShiftId { get; set; }

        // navigation property for attendance
        public Attendance Attendance { get; set; }

        // navigation property for jop description
        public JopDescription JopDescription { get; set; }
        public int? JopDescriptionId { get; set; }

        // navigation property for production line
        public ProductionLine ProductionLine { get; set; }
        public int? ProductionLineId { get; set; }
        // navigation property for vacation request
        public VacationRequest VacationRequest { get; set; }
         public int? VacationRequestId { get; set; }

        // navigation property for swap request
        public SwapRequest SwapRequest { get; set; }
    }
}
