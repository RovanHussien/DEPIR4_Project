using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public int EmployeeCount { get; set; }

        // navigation property for manager
        public Employee Manager { get; set; }
        [ForeignKey("Manager")]
        public string? ManagerSsn { get; set; }

        // navigation property for production lines
        public List<ProductionLine> ProductionLines { get; set; }

        // navigation property for EmployeeDepartment
        public List<EmployeeDepartment> EmployeeDepartments { get; set; }
    }
}
