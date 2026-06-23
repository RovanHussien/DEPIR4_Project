using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Models
{
    public class ApplicationUser : IdentityUser
    {
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Pending;
        public decimal BaseSalary { get; set; }
        public string ActualRole { get; set; } = "Employee"; 
        public Employee Employee { get; set; }
    }
}
