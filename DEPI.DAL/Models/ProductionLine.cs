using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public class ProductionLine
    {
        public int ProductionLineId { get; set; }
        public string Name { get; set; }

        // navigation property for Employee
        public List<Employee> Employees { get; set; }

        // navigation property for Department
        public Department? Department { get; set; }
        public int? DepartmentId { get; set; }

        // navigation property for JopDescription
        public List<JopDescription> JopDescriptions { get; set; }

        // navigation property for Schedule
        public List<Schedule> Schedules { get; set; }
    }
}
