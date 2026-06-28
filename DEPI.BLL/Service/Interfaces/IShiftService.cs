using DEPI.BLL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IShiftService
    {
        Task<List<ShiftDto>> GetAllShiftsAsync();
        Task<ShiftDto> GetShiftByIdAsync(int id);
        Task<ShiftDto> AddShiftAsync(ShiftDto shiftDto);
        Task<ShiftDto> UpdateShiftAsync(int id, ShiftDto shiftDto);
        Task<bool> DeleteShiftAsync(int id);
    }
}
