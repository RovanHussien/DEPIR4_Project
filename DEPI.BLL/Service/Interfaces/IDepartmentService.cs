using DEPI.BLL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> GetDepartmentByIdAsync(int id);
        Task<DepartmentDto> AddDepartmentAsync(DepartmentDto departmentDto);
        Task<DepartmentDto> UpdateDepartmentAsync(int id, DepartmentDto departmentDto);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}
