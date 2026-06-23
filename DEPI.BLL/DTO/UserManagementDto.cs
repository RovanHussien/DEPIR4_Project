using DEPI.DAL.Enums;
using System;

namespace DEPI.BLL.DTO
{
    public class UserManagementDto
    {
        public string? UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Sex { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public int PhoneNumber { get; set; }
        public decimal BaseSalary { get; set; }
        public string ActualRole { get; set; }
        public int DepartmentId { get; set; }
        public int ProductionLineId { get; set; }
        public EmployeeStatus Status { get; set; }
    }
}
