using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public class JopDescription
    {
        public int JopDescriptionId { get; set; }
        public string DailyTasks { get; set; }
        public string RequiredCount { get; set; }
        public string RoleName { get; set; }

        // navigation property for Schedule
        public List<Schedule> Schedules { get; set; }

        // navigation property for ProductionLine
        public ProductionLine? ProductionLine { get; set; }
        public int? ProductionId { get; set; }
    }
}
