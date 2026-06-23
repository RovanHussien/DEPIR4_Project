using DEPI.DAL.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Interfaces
{
    public interface IProductionLineRepo
    {
        Task<List<ProductionLine>> GetAllProductionLinesAsync();
        Task<ProductionLine> GetProductionLineByIdAsync(int id);
        Task<List<ProductionLine>> GetProductionLinesByDepartmentAsync(int departmentId);
        Task<ProductionLine> AddProductionLineAsync(ProductionLine productionLine);
        Task<ProductionLine> UpdateProductionLineAsync(int id, ProductionLine productionLine);
        Task<bool> DeleteProductionLineAsync(int id);
    }
}
