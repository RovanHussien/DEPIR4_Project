using DEPI.DAL.Enums;
using DEPI.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Model
{
    public class Employee
    {
        [Key]
        [Display(Name ="EmployeeSsn")]
        public string EmployeeSsn { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Salary { get; set; }
        public string Sex { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }
        public int PhoneNumber { get; set; }
        public int? VacationBalance { get; set; }
        public string? DefaultRole { get; set; }

        // navigation properties for department
        public Department ManagedDepartment { get; set; }

        // self-referencing relationship for manager-subordinate
        public Employee Manager { get; set; }
        public string? ManagerSsn { get; set; }

        public List<Employee> Subordinates { get; set; }

        // navigation properties for shifts
        public Shift Shift { get; set; }
        public int? ShiftId { get; set; }

        // navigation properties for missions
        public List<Mission> AuthorizedMissions { get; set; }
        public List<Mission> GoesOnMissions { get; set; }
        // navigation properties for schedule
        public List<Schedule> Schedules { get; set; }

        // navigation properties for swap requests
        public List<SwapRequest> SentSwapRequests { get; set; }
        public List<SwapRequest> ReceivedSwapRequests { get; set; }

        // navigation properties for vacation requests
        public List<VacationRequest> VacationRequests {get; set;}

        // navigation properties for ProductionLine
        public ProductionLine ProductionLine { get; set;}
        public int? ProductionLineId { get; set; }

        // navigation properties for EmployeeDepartment
        public List<EmployeeDepartment> EmployeeDepartments { get; set; }

        // navigation properties for ApplicationUser
        public ApplicationUser ApplicationUser { get; set; }
        public string? UserId { get; set; }

    }
}
