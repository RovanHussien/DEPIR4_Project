using DEPI.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IAdminService
    {
        public Task<List<EmployeeStatusDto>> GetPendingEmployeesAsync();
        public Task<List<EmployeeStatusDto>> ApprovedEmployeeAsync();
        public Task<List<EmployeeStatusDto>> RejectedEmployeeAsync();
        public Task ApproveEmployeeAsync(string Email);
        public Task RejectEmployeeAsync(string Email);
    }
}
