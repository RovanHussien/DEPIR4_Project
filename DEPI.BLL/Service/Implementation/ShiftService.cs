using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Implementation
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepo _shiftRepo;

        public ShiftService(IShiftRepo shiftRepo)
        {
            _shiftRepo = shiftRepo;
        }

        public async Task<List<ShiftDto>> GetAllShiftsAsync()
        {
            var shifts = await _shiftRepo.GetAllShiftsAsync();
            return shifts.Select(s => new ShiftDto
            {
                ShiftId = s.ShiftId,
                Name = s.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList();
        }

        public async Task<ShiftDto> GetShiftByIdAsync(int id)
        {
            var shift = await _shiftRepo.GetShiftByIdAsync(id);
            if (shift == null) return null;

            return new ShiftDto
            {
                ShiftId = shift.ShiftId,
                Name = shift.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            };
        }

        public async Task<ShiftDto> AddShiftAsync(ShiftDto shiftDto)
        {
            var shift = new Shift
            {
                Name = shiftDto.Name,
                StartTime = shiftDto.StartTime,
                EndTime = shiftDto.EndTime
            };

            await _shiftRepo.AddShiftAsync(shift);
            shiftDto.ShiftId = shift.ShiftId;

            return shiftDto;
        }

        public async Task<ShiftDto> UpdateShiftAsync(int id, ShiftDto shiftDto)
        {
            var shift = await _shiftRepo.GetShiftByIdAsync(id);
            if (shift == null) return null;

            shift.Name = shiftDto.Name;
            shift.StartTime = shiftDto.StartTime;
            shift.EndTime = shiftDto.EndTime;

            await _shiftRepo.UpdateShiftAsync(shift);

            return shiftDto;
        }

        public async Task<bool> DeleteShiftAsync(int id)
        {
            var shift = await _shiftRepo.GetShiftByIdAsync(id);
            if (shift == null) return false;

            await _shiftRepo.DeleteShiftAsync(id);
            return true;
        }
    }
}
