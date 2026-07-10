using DEPI.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Interfaces
{
    public interface IAttendanceRepo
    {
        Task<Attendance?> GetTodayAttendanceAsync(string employeeSsn);
        Task<List<Attendance>> GetAttendanceByEmployeeAsync(string employeeSsn, DateTime? from, DateTime? to);
        Task<List<Attendance>> GetAllAttendanceByDateAsync(DateTime date);
        Task<List<Attendance>> GetAttendanceByEmployeeSsnsAsync(List<string> employeeSsns, DateTime? date);
        Task AddAttendanceAsync(Attendance attendance);
        Task UpdateAttendanceAsync(Attendance attendance);
        Task<Employee?> GetEmployeeByFingerprintIdAsync(string fingerprintId);
        Task<List<Employee>> GetEmployeesWithNoCheckInTodayAsync();
    }
}
