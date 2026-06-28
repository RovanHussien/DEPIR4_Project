using DEPI.DAL.DbContext;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Implementation
{
    public class DepartmentRepo : IDepartmentRepo
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments.Include(d => d.Manager).ToListAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Manager)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (department == null)
                throw new Exception("Department not found");
            return department;
        }

        public async Task<Department> AddDepartmentAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<Department> UpdateDepartmentAsync(int id, Department department)
        {
            var existingDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (existingDepartment == null)
                throw new Exception("Department not found");

            existingDepartment.Name = department.Name;
            existingDepartment.ManagerSsn = department.ManagerSsn;
            existingDepartment.EmployeeCount = department.EmployeeCount;

            _context.Departments.Update(existingDepartment);
            await _context.SaveChangesAsync();
            return existingDepartment;
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (department == null)
                throw new Exception("Department not found");

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
