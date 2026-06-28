using System;

namespace DEPI.BLL.DTO
{
    public class ShiftDto
    {
        public int ShiftId { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
