using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public class EmployeeDepartment
    {
        public Employee Employees { get; set; }
        public Department Departments { get; set; }
        public string? EmployeeSsn { get; set; }
        public int? DepartmentID { get; set; }
        public int Hours { get; set; }

    }
}
