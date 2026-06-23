namespace DEPI.BLL.DTO
{
    public class ProductionLineDto
    {
        public int ProductionLineId { get; set; }
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}
