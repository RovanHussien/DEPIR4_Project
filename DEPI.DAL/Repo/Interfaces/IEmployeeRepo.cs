using DEPI.DAL.Enums;
using DEPI.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Interfaces
{
    public interface IEmployeeRepo
    {
        public  Task<List<Employee>> GetAllEmployees();
        public Task<Employee> GetEmployeeById(string id);
        public Task<IdentityResult> AddEmployee(Employee employee);
        public Task<IdentityResult> UpdateEmployeeAsync(string id, Employee employee);
        public Task<IdentityResult> DeleteEmployeeAsync(string id);
    }
}
