using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Implementation
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepo _departmentRepo;

        public DepartmentService(IDepartmentRepo departmentRepo)
        {
            _departmentRepo = departmentRepo;
        }

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _departmentRepo.GetAllDepartmentsAsync();
            return departments.Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                Name = d.Name,
                EmployeeCount = d.EmployeeCount,
                ManagerSsn = d.ManagerSsn,
                ManagerName = d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}" : "Not Assigned"
            }).ToList();
        }

        public async Task<DepartmentDto> GetDepartmentByIdAsync(int id)
        {
            var department = await _departmentRepo.GetDepartmentByIdAsync(id);
            return new DepartmentDto
            {
                DepartmentId = department.DepartmentId,
                Name = department.Name,
                EmployeeCount = department.EmployeeCount,
                ManagerSsn = department.ManagerSsn,
                ManagerName = department.Manager != null ? $"{department.Manager.FirstName} {department.Manager.LastName}" : "Not Assigned"
            };
        }

        public async Task<DepartmentDto> AddDepartmentAsync(DepartmentDto departmentDto)
        {
            var department = new Department
            {
                Name = departmentDto.Name,
                ManagerSsn = departmentDto.ManagerSsn,
                EmployeeCount = 0
            };
            var newDept = await _departmentRepo.AddDepartmentAsync(department);
            return new DepartmentDto
            {
                DepartmentId = newDept.DepartmentId,
                Name = newDept.Name,
                EmployeeCount = newDept.EmployeeCount,
                ManagerSsn = newDept.ManagerSsn
            };
        }

        public async Task<DepartmentDto> UpdateDepartmentAsync(int id, DepartmentDto departmentDto)
        {
            var department = new Department
            {
                DepartmentId = id,
                Name = departmentDto.Name,
                ManagerSsn = departmentDto.ManagerSsn,
                EmployeeCount = departmentDto.EmployeeCount
            };
            var updatedDept = await _departmentRepo.UpdateDepartmentAsync(id, department);
            return new DepartmentDto
            {
                DepartmentId = updatedDept.DepartmentId,
                Name = updatedDept.Name,
                EmployeeCount = updatedDept.EmployeeCount,
                ManagerSsn = updatedDept.ManagerSsn
            };
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            return await _departmentRepo.DeleteDepartmentAsync(id);
        }
    }
}
