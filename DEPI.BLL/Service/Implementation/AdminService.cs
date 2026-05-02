using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.Repo.Implementation;
using DEPI.DAL.Repo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IUserRepo _userRepo;

        public AdminService(IEmployeeRepo employeeRepo, IUserRepo userRepo)
        {
            _employeeRepo = employeeRepo;
            _userRepo = userRepo;
        }
        public async Task<List<EmployeeStatusDto>> ApprovedEmployeeAsync()
        {
            var employees = await _userRepo.GetAllUsersAsync();
            if (employees == null)
            {
                throw new InvalidOperationException("No employees found.");
            }
            var approvedEmployees = employees.Where(p => p.Status.ToString() == "Approved").ToList();
            if (approvedEmployees == null)
            {
                throw new InvalidOperationException("No approved employees found.");
            }
            var emps = approvedEmployees.Select(e => new EmployeeStatusDto
            {
                Id = e.Employee.EmployeeSsn,
                Email = e.Email ?? string.Empty,
                Name = e.Employee.FirstName + " " + e.Employee.LastName,
                Status = e.Status
            }).ToList();
            return emps;
        }


        public async Task<List<EmployeeStatusDto>> GetPendingEmployeesAsync()
        {
            var employees = await _userRepo.GetAllUsersAsync();
            if (employees == null)
            {
                throw new InvalidOperationException("No employees found.");
            }
            var approvedEmployees = employees.Where(p => p.Status.ToString() == "Pending").ToList();
            if (approvedEmployees == null)
            {
                throw new InvalidOperationException("No pending employees found.");
            }
            var emps = approvedEmployees.Select(e => new EmployeeStatusDto
            {
                Id = e.Employee.EmployeeSsn,
                Email = e.Email ?? string.Empty,
                Name = e.Employee.FirstName + " " + e.Employee.LastName,
                Status = e.Status
            }).ToList();
            return emps;
        }

        public async Task<List<EmployeeStatusDto>> RejectedEmployeeAsync()
        {
            var employees = await _userRepo.GetAllUsersAsync();
            if (employees == null)
            {
                throw new InvalidOperationException("No employees found.");
            }
            var rejectedEmployees = employees.Where(p => p.Status.ToString() == "Rejected").ToList();
            if (rejectedEmployees == null)
            {
                throw new InvalidOperationException("No rejected employees found.");
            }
            var emps = rejectedEmployees.Select(e => new EmployeeStatusDto
            {
                Id = e.Employee.EmployeeSsn,
                Email = e.Email ?? string.Empty,
                Name = e.Employee.FirstName + " " + e.Employee.LastName,
                Status = e.Status
            }).ToList();
            return emps;
        }

        public async Task ApproveEmployeeAsync(string Email)
        {
            var user = await _userRepo.GetByEmailAsync(Email);
            if (user == null) {
                throw new InvalidOperationException("Employee not found.");
            }
            user.Status = DAL.Enums.EmployeeStatus.Approved;
        }
        public async Task RejectEmployeeAsync(string Email)
        {
            var user = await _userRepo.GetByEmailAsync(Email);
            if (user == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }
            user.Status = DAL.Enums.EmployeeStatus.Rejected;
        }
    }
}
