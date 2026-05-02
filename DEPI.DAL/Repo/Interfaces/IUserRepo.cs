using DEPI.DAL.Enums;
using DEPI.DAL.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Interfaces
{
    public interface IUserRepo
    {
        public Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        public Task<IdentityResult> UpdateUserAsync(string Email, ApplicationUser user);
        public Task<ApplicationUser> GetByEmailAsync(string Email);
        public Task<ApplicationUser> GetUserByIdAsync(string userId);
        public Task<List<ApplicationUser>> GetByUsersByStatus(EmployeeStatus status);
        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        public Task<List<ApplicationUser>> GetAllUsersAsync();
    }
}
