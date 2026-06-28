using DEPI.DAL.Model;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Interfaces
{
    public interface IDepartmentRepo
    {
        Task<List<Department>> GetAllDepartmentsAsync();
        Task<Department> GetDepartmentByIdAsync(int id);
        Task<Department> AddDepartmentAsync(Department department);
        Task<Department> UpdateDepartmentAsync(int id, Department department);
        Task<bool> DeleteDepartmentAsync(int id);
    }
}
