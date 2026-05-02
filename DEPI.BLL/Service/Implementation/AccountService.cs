using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.Model;
using DEPI.DAL.Models;
using DEPI.DAL.Repo.Implementation;
using DEPI.DAL.Repo.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IUserRepo _userRepo;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService( IEmployeeRepo employeeRepo, IUserRepo userRepo, SignInManager<ApplicationUser> signInManager)
        {
            _employeeRepo = employeeRepo;
            _userRepo = userRepo;
            _signInManager = signInManager;
        }
        public async Task<IdentityResult> RegisterEmployeeAsync(EmployeeRegisterationDto employeeDto)
        {
            ApplicationUser user = new ApplicationUser
            {
                Email = employeeDto.Email,
                UserName = employeeDto.FirstName + employeeDto.LastName,
                PasswordHash = employeeDto.Password,
                PhoneNumber = employeeDto.PhoneNumber.ToString()
            };
            await _userRepo.CreateUserAsync(user, user.PasswordHash);
            var employee = new Employee
            {
                EmployeeSsn = employeeDto.EmployeeId,
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                PhoneNumber = employeeDto.PhoneNumber,
                Address = employeeDto.Address,
                BirthDate = employeeDto.BirthDate,
                Sex = employeeDto.Sex,
                UserId = user.Id
            };
            await _employeeRepo.AddEmployee(employee);
            return IdentityResult.Success;
        }

        public async Task<string> CheckUserStatus(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null)
                return "Not Found";
            return user.Status.ToString();
        }
        public Task<IdentityResult> UpdateUserAsync(string Email, EmployeeRegisterationDto employeeDto)
        {
            throw new NotImplementedException();
        }

        public async Task<SignInResult> LogOutAsync()
        {
            await _signInManager.SignOutAsync();
            return SignInResult.Success;
        }

        public async Task<SignInResult> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepo.GetByEmailAsync(loginDto.Email);
            var result = await _userRepo.CheckPasswordAsync(user, loginDto.Password);
            if (result)
            {
                await _signInManager.SignInAsync(user,loginDto.RememberMe);
                return SignInResult.Success;
            }
            return SignInResult.Failed;
        }
    }
}
