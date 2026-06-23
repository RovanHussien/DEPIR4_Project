using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using DEPI.DAL.Models;
using DEPI.DAL.Repo.Interfaces;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public AdminService(IEmployeeRepo employeeRepo, IUserRepo userRepo, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _employeeRepo = employeeRepo;
            _userRepo = userRepo;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<List<EmployeeStatusDto>> ApprovedEmployeeAsync()
        {
            var employees = await _userRepo.GetAllUsersAsync();
            if (employees == null)
                return new List<EmployeeStatusDto>();

            var approvedEmployees = employees.Where(p => p.Status.ToString() == "Approved").ToList();
            if (!approvedEmployees.Any())
                return new List<EmployeeStatusDto>();

            var emps = approvedEmployees.Select(e => new EmployeeStatusDto
            {
                Id = e.Id,
                Email = e.Email ?? string.Empty,
                Name = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : "N/A",
                Status = e.Status
            }).ToList();
            return emps;
        }

        public async Task<List<EmployeeStatusDto>> GetPendingEmployeesAsync()
        {
            var employees = await _userRepo.GetAllUsersAsync();
            if (employees == null)
                return new List<EmployeeStatusDto>();

            var pendingEmployees = employees.Where(p => p.Status == EmployeeStatus.Pending).ToList();
            if (!pendingEmployees.Any())
                return new List<EmployeeStatusDto>();

            var emps = pendingEmployees.Select(e => new EmployeeStatusDto
            {
                Id = e.Id,
                Email = e.Email ?? string.Empty,
                Name = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : "N/A",
                Status = e.Status
            }).ToList();
            return emps;
        }

        public async Task<List<EmployeeStatusDto>> RejectedEmployeeAsync()
        {
            var employees = await _userRepo.GetAllUsersAsync();
            if (employees == null)
                return new List<EmployeeStatusDto>();

            var rejectedEmployees = employees.Where(p => p.Status == EmployeeStatus.Rejected).ToList();
            if (!rejectedEmployees.Any())
                return new List<EmployeeStatusDto>();

            var emps = rejectedEmployees.Select(e => new EmployeeStatusDto
            {
                Id = e.Id,
                Email = e.Email ?? string.Empty,
                Name = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : "N/A",
                Status = e.Status
            }).ToList();
            return emps;
        }

        public async Task<EmployeeStatusDto> GetPendingEmployeeDetailsAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null || user.Status != EmployeeStatus.Pending)
                throw new InvalidOperationException("User not found or not in pending status");

            return new EmployeeStatusDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Employee != null ? $"{user.Employee.FirstName} {user.Employee.LastName}" : "N/A",
                Status = user.Status
            };
        }

        public async Task<bool> CompleteEmployeeApprovalAsync(string userId, AdminApprovalDto approvalDto)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.BaseSalary = approvalDto.BaseSalary;
            user.ActualRole = approvalDto.ActualRole;
            user.Status = EmployeeStatus.Approved;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to update user");

            if (!string.IsNullOrEmpty(approvalDto.ActualRole))
            {
                if (!await _userManager.IsInRoleAsync(user, approvalDto.ActualRole))
                {
                    await _userManager.AddToRoleAsync(user, approvalDto.ActualRole);
                }
            }

            if (user.Employee != null)
            {
                user.Employee.Salary = approvalDto.BaseSalary;
                user.Employee.DefaultRole = approvalDto.ActualRole;
                if (approvalDto.ProductionLineId > 0)
                    user.Employee.ProductionLineId = approvalDto.ProductionLineId;

                var updateResult = await _employeeRepo.UpdateEmployeeAsync(user.Employee.EmployeeSsn, user.Employee);
                if (updateResult != IdentityResult.Success)
                    throw new InvalidOperationException("Failed to update employee data");
            }

            // Send confirmation email
            await _emailService.SendEmailAsync(
                user.Email ?? "",
                "PharmaWorks - Registration Approved",
                $"<h3>Dear {user.Employee?.FirstName ?? "Employee"},</h3>" +
                $"<p>Your registration request has been <b>approved</b> by the Administrator.</p>" +
                $"<p>You can now log in to the portal using your email address: <b>{user.Email}</b></p>" +
                $"<br/><p>Best regards,<br/>PharmaWorks Team</p>"
            );

            return true;
        }

        public async Task<bool> ApproveEmployeeAsync(string Email)
        {
            var user = await _userRepo.GetByEmailAsync(Email);
            if (user == null)
                throw new InvalidOperationException("Employee not found.");

            user.Status = EmployeeStatus.Approved;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _emailService.SendEmailAsync(
                    user.Email ?? "",
                    "PharmaWorks - Registration Approved",
                    $"<h3>Dear {user.Employee?.FirstName ?? "Employee"},</h3>" +
                    $"<p>Your registration request has been <b>approved</b> by the Administrator.</p>" +
                    $"<p>You can now log in to the portal using your email address: <b>{user.Email}</b></p>" +
                    $"<br/><p>Best regards,<br/>PharmaWorks Team</p>"
                );
            }
            return result.Succeeded;
        }

        public async Task<bool> RejectEmployeeAsync(string Email)
        {
            var user = await _userRepo.GetByEmailAsync(Email);
            if (user == null)
                throw new InvalidOperationException("Employee not found.");

            user.Status = EmployeeStatus.Rejected;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync()
        {
            var allUsers = await _userRepo.GetAllUsersAsync();
            var allEmployees = await _employeeRepo.GetAllEmployees();

            var pendingCount = allUsers.Count(u => u.Status == EmployeeStatus.Pending);
            var activeManagers = allUsers.Count(u => u.ActualRole == "Manager" && u.Status == EmployeeStatus.Approved);

            return new AdminDashboardStatsDto
            {
                TotalEmployees = allEmployees.Count,
                ActiveDepartments = 0, 
                PendingApprovals = pendingCount,
                TotalProductionLines = 0, 
                ActiveManagers = activeManagers
            };
        }

        public async Task<List<UserManagementDto>> GetAllUsersGroupedByDepartmentAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();
            var userDtos = users
                .Where(u => u.Status != EmployeeStatus.Rejected)
                .Select(u => new UserManagementDto
                {
                    UserId = u.Id,
                    Email = u.Email,
                    FirstName = u.Employee?.FirstName ?? "N/A",
                    LastName = u.Employee?.LastName ?? "N/A",
                    Sex = u.Employee?.Sex ?? "N/A",
                    BirthDate = u.Employee?.BirthDate ?? DateTime.MinValue,
                    Address = u.Employee?.Address ?? "N/A",
                    PhoneNumber = u.Employee?.PhoneNumber ?? 0,
                    BaseSalary = u.BaseSalary,
                    ActualRole = u.ActualRole ?? "Employee",
                    DepartmentId = 0,
                    ProductionLineId = u.Employee?.ProductionLineId ?? 0,
                    Status = u.Status
                }).ToList();

            return userDtos;
        }

        public async Task<UserManagementDto> AddUserAsync(UserManagementDto userDto, string password)
        {
            var applicationUser = new ApplicationUser
            {
                UserName = userDto.Email,
                Email = userDto.Email,
                BaseSalary = userDto.BaseSalary,
                ActualRole = userDto.ActualRole ?? "Employee",
                Status = EmployeeStatus.Approved,
                PhoneNumber = userDto.PhoneNumber.ToString()
            };

            var result = await _userManager.CreateAsync(applicationUser, password);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!string.IsNullOrEmpty(applicationUser.ActualRole))
            {
                await _userManager.AddToRoleAsync(applicationUser, applicationUser.ActualRole);
            }

            var employee = new Employee
            {
                EmployeeSsn = Guid.NewGuid().ToString().Substring(0, 10),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Sex = userDto.Sex ?? "Male",
                BirthDate = userDto.BirthDate ?? DateTime.Today.AddYears(-25),
                Address = userDto.Address ?? "N/A",
                PhoneNumber = userDto.PhoneNumber,
                Salary = userDto.BaseSalary,
                DefaultRole = userDto.ActualRole,
                ProductionLineId = userDto.ProductionLineId > 0 ? userDto.ProductionLineId : null,
                UserId = applicationUser.Id
            };

            var empResult = await _employeeRepo.AddEmployee(employee);
            if (empResult != IdentityResult.Success)
                throw new InvalidOperationException("Failed to create employee record");

            return new UserManagementDto
            {
                UserId = applicationUser.Id,
                Email = applicationUser.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Sex = employee.Sex,
                BirthDate = employee.BirthDate,
                Address = employee.Address,
                PhoneNumber = employee.PhoneNumber,
                BaseSalary = applicationUser.BaseSalary,
                ActualRole = applicationUser.ActualRole,
                Status = applicationUser.Status
            };
        }

        public async Task<UserManagementDto> UpdateUserAsync(string userId, UserManagementDto userDto)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.Email = userDto.Email;
            user.UserName = userDto.Email;
            user.BaseSalary = userDto.BaseSalary;
            var oldRole = user.ActualRole;
            user.ActualRole = userDto.ActualRole ?? "Employee";
            user.PhoneNumber = userDto.PhoneNumber.ToString();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to update user");

            if (oldRole != user.ActualRole)
            {
                if (!string.IsNullOrEmpty(oldRole) && await _userManager.IsInRoleAsync(user, oldRole))
                {
                    await _userManager.RemoveFromRoleAsync(user, oldRole);
                }
                if (!string.IsNullOrEmpty(user.ActualRole))
                {
                    await _userManager.AddToRoleAsync(user, user.ActualRole);
                }
            }

            if (user.Employee != null)
            {
                user.Employee.FirstName = userDto.FirstName;
                user.Employee.LastName = userDto.LastName;
                user.Employee.Sex = userDto.Sex ?? user.Employee.Sex ?? "Male";
                user.Employee.BirthDate = userDto.BirthDate ?? user.Employee.BirthDate;
                user.Employee.Address = userDto.Address ?? user.Employee.Address ?? "N/A";
                user.Employee.PhoneNumber = userDto.PhoneNumber;
                user.Employee.Salary = userDto.BaseSalary;
                user.Employee.DefaultRole = userDto.ActualRole;
                if (userDto.ProductionLineId > 0)
                    user.Employee.ProductionLineId = userDto.ProductionLineId;

                var empResult = await _employeeRepo.UpdateEmployeeAsync(user.Employee.EmployeeSsn, user.Employee);
                if (empResult != IdentityResult.Success)
                    throw new InvalidOperationException("Failed to update employee data");
            }

            return new UserManagementDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.Employee?.FirstName ?? userDto.FirstName,
                LastName = user.Employee?.LastName ?? userDto.LastName,
                Sex = user.Employee?.Sex ?? userDto.Sex,
                BirthDate = user.Employee?.BirthDate ?? userDto.BirthDate,
                Address = user.Employee?.Address ?? userDto.Address,
                PhoneNumber = user.Employee?.PhoneNumber ?? userDto.PhoneNumber,
                BaseSalary = user.BaseSalary,
                ActualRole = user.ActualRole,
                Status = user.Status
            };
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            user.Status = EmployeeStatus.Rejected;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
