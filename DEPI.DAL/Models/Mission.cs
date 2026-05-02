using DEPI.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public  class Mission
    {
        public int MissionId { get; set; }
        public MissionStatus Status { get; set; }
        public string Purpose { get; set; }
        public string Destination { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // navigation property for employee
        public Employee? AuthorizedEmployee { get; set; }
        public string? AuthorizedEmployeeSsn { get; set; }
        public Employee? GoesOnEmployee { get; set; }
        public string? GoesOnEmployeeSsn { get; set; }

        // navigation property for schedule
        public List<Schedule> Schedules { get; set; }

    }
}
