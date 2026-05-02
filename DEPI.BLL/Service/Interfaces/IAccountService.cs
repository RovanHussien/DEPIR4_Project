using DEPI.BLL.DTO;
using DEPI.DAL.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IAccountService
    {
        public Task<IdentityResult> RegisterEmployeeAsync(EmployeeRegisterationDto employeeDto);
        public Task<string> CheckUserStatus(string email);
        public Task<IdentityResult> UpdateUserAsync(string email, EmployeeRegisterationDto employeeDto);
        public Task<SignInResult> LogOutAsync();
        public Task<SignInResult> LoginAsync(LoginDto loginDto);
    }
}
