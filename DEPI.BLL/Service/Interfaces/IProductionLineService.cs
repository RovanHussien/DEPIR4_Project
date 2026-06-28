using DEPI.BLL.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Interfaces
{
    public interface IProductionLineService
    {
        Task<List<ProductionLineDto>> GetAllProductionLinesAsync();
        Task<ProductionLineDto> GetProductionLineByIdAsync(int id);
        Task<List<ProductionLineDto>> GetProductionLinesByDepartmentAsync(int departmentId);
        Task<ProductionLineDto> AddProductionLineAsync(ProductionLineDto productionLineDto);
        Task<ProductionLineDto> UpdateProductionLineAsync(int id, ProductionLineDto productionLineDto);
        Task<bool> DeleteProductionLineAsync(int id);
    }
}
