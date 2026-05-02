using DEPI.DAL.DbContext;
using DEPI.DAL.Enums;
using DEPI.DAL.Models;
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
    public class UserRepo : IUserRepo
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public UserRepo(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<ApplicationUser> GetByEmailAsync(string Email)
        {
            var user =  await _userManager.FindByEmailAsync(Email);
            return user?? throw new Exception("User not found");
        }

        public async Task<List<ApplicationUser>> GetByUsersByStatus(EmployeeStatus status)
        {
            var users = await _context.Users.Where(u => u.Status == status).ToListAsync();
            return users;
        }

        public async Task<IdentityResult> UpdateUserAsync(string Email, ApplicationUser user)
        {
            var existingUser = await _userManager.FindByEmailAsync(Email);
            if (existingUser == null)
            {
                throw new Exception("User not found");
            }

            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Status = user.Status;

            return await _userManager.UpdateAsync(existingUser);
        }
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user ?? throw new Exception("User not found");
        }
        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CheckPasswordAsync(user, password);
            return result;
        }
        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            var users = await _context.Users.Include(e => e.Employee).ToListAsync();
            return users;
        }

    }
}
