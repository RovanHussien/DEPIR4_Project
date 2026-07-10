using DEPI.DAL.DbContext;
using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Implementation
{
    public class EmployeeRepo : IEmployeeRepo
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IdentityResult> AddEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            _context.SaveChanges();
            return Task.FromResult(IdentityResult.Success);
        }
        public async Task<IdentityResult> DeleteEmployeeAsync(string id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeSsn == id);
            if (employee == null)
            {
                throw new Exception("Employee not found");
            }
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<List<Employee>> GetAllEmployees()
        {
            var employees = await _context.Employees.ToListAsync();
            return employees;
        }



        public async Task<Employee> GetEmployeeByUserIdAsync(string userId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }
        public async Task<Employee> GetEmployeeById(string id)
        {
            var employee = await _context.Employees
                 .Include(e => e.VacationRequests)
                .Include(e => e.ReceivedSwapRequests)
                 .Include(e => e.SentSwapRequests)
                .Include(e => e.Shift)
                .Include(e => e.ProductionLine)
                    .ThenInclude(p => p.Department)
                        .ThenInclude(d => d.Manager)
                .Include(e => e.Schedules)
                    .ThenInclude(s => s.Shift)
                .Include(e => e.Schedules)
                    .ThenInclude(s => s.ProductionLine)
                        .ThenInclude(p => p.Department)
                            .ThenInclude(d => d.Manager)
                .Include(e => e.Schedules)
                    .ThenInclude(s => s.Mission)
                .FirstOrDefaultAsync(e => e.EmployeeSsn == id);




            if (employee == null)
            {
                throw new Exception("Employee not found");
            }
            return employee;
        }
        public async Task<IdentityResult> UpdateEmployeeAsync(string id, Employee employee)
        {
            var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeSsn == id);
            if (existingEmployee == null)
            {
                throw new Exception("Employee not found");
            }
            existingEmployee.FirstName = employee.FirstName;
            existingEmployee.LastName = employee.LastName;
            existingEmployee.Salary = employee.Salary;
            existingEmployee.Sex = employee.Sex;
            existingEmployee.BirthDate = employee.BirthDate;
            existingEmployee.Address = employee.Address;
            existingEmployee.PhoneNumber = employee.PhoneNumber;
            existingEmployee.VacationBalance = employee.VacationBalance;
            existingEmployee.DefaultRole = employee.DefaultRole;
            existingEmployee.ManagedDepartment = employee.ManagedDepartment;
            existingEmployee.Manager = employee.Manager;
            existingEmployee.ManagerSsn = employee.ManagerSsn;
            existingEmployee.Shift = employee.Shift;
            existingEmployee.ShiftId = employee.ShiftId;
            existingEmployee.ProductionLine = employee.ProductionLine;
            existingEmployee.ProductionLineId = employee.ProductionLineId;

            _context.Employees.Update(existingEmployee);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
        public async Task<bool> UpdateProfilePictureAsync(string ssn, string fileName)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeSsn == ssn);
            if (employee == null) return false;
            employee.ProfilePicture = fileName;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task UpdateVacationRequestsCountAsync(string ssn, int count, int year)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeSsn == ssn);
            if (employee != null)
            {
                employee.VacationRequestsCount = count;
                employee.LastResetYear = year;
                await _context.SaveChangesAsync();
            }
        }
    }
}