using DEPI.DAL.Enums;

namespace DEPI.BLL.DTO
{
    public class AdminApprovalDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public decimal BaseSalary { get; set; }
        public string ActualRole { get; set; }
        public int ProductionLineId { get; set; }
        public EmployeeStatus Status { get; set; }
    }
}
