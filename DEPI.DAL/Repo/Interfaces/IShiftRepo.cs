using DEPI.DAL.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Interfaces
{
    public interface IShiftRepo
    {
        Task<IEnumerable<Shift>> GetAllShiftsAsync();
        Task<Shift> GetShiftByIdAsync(int id);
        Task AddShiftAsync(Shift shift);
        Task UpdateShiftAsync(Shift shift);
        Task DeleteShiftAsync(int id);
    }
}
