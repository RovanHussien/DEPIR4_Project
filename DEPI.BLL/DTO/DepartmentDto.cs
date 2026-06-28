namespace DEPI.BLL.DTO
{
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public int EmployeeCount { get; set; }
        public string? ManagerSsn { get; set; }
        public string? ManagerName { get; set; }
    }
}
