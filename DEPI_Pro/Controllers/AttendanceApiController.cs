using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DEPI.PLL.Controllers
{
  
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceApiController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceApiController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

  
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] FingerprintDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.FingerprintId))
                return BadRequest(new AttendanceResultDto { Success = false, Message = "Fingerprint ID is required." });

            var result = await _attendanceService.RecordCheckInAsync(dto.FingerprintId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromBody] FingerprintDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.FingerprintId))
                return BadRequest(new AttendanceResultDto { Success = false, Message = "Fingerprint ID is required." });

            var result = await _attendanceService.RecordCheckOutAsync(dto.FingerprintId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
