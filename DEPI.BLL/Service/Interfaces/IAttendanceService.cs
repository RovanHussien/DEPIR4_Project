using DEPI.BLL.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceResultDto> RecordCheckInAsync(string fingerprintId);
        Task<AttendanceResultDto> RecordCheckOutAsync(string fingerprintId);

        Task MarkAbsenteesAsync();

        Task<AttendanceSummaryDto> GetAttendanceSummaryByDateAsync(DateTime date);
        Task<List<AttendanceRecordDto>> GetAttendanceByEmployeeSsnsAsync(List<string> employeeSsns, DateTime? date);
        Task<List<AttendanceRecordDto>> GetManagerAttendanceAsync(DateTime? date);
        Task<AttendanceRecordDto?> GetTodayAttendanceForEmployeeAsync(string employeeSsn);
    }
}
