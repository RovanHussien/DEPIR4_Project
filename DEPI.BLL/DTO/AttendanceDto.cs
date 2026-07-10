using DEPI.DAL.Enums;
using System;
using System.Collections.Generic;

namespace DEPI.BLL.DTO
{
 
    public class FingerprintDto
    {
        public string FingerprintId { get; set; }
    }
    public class AttendanceResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string EmployeeName { get; set; }
        public string Status { get; set; }
        public DateTime? Time { get; set; }
    }
    public class AttendanceRecordDto
    {
        public int AttendanceId { get; set; }
        public string EmployeeSsn { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; }
        public string StatusBadgeClass { get; set; }
        public string ShiftName { get; set; }
        public string Notes { get; set; }
    }
    public class AttendanceSummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalEmployees { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int OnLeaveCount { get; set; }
        public List<AttendanceRecordDto> Records { get; set; } = new List<AttendanceRecordDto>();
    }
}
