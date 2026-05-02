using DEPI.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.DTO
{
    public class EmployeeStatusDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public EmployeeStatus Status { get; set; }
    }
}
