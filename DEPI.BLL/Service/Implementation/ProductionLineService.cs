using DEPI.BLL.DTO;
using DEPI.BLL.Service.Interfaces;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.BLL.Service.Implementation
{
    public class ProductionLineService : IProductionLineService
    {
        private readonly IProductionLineRepo _productionLineRepo;

        public ProductionLineService(IProductionLineRepo productionLineRepo)
        {
            _productionLineRepo = productionLineRepo;
        }

        public async Task<List<ProductionLineDto>> GetAllProductionLinesAsync()
        {
            var lines = await _productionLineRepo.GetAllProductionLinesAsync();
            return lines.Select(l => new ProductionLineDto
            {
                ProductionLineId = l.ProductionLineId,
                Name = l.Name,
                DepartmentId = l.DepartmentId ?? 0,
                DepartmentName = l.Department?.Name ?? "Not Assigned"
            }).ToList();
        }

        public async Task<ProductionLineDto> GetProductionLineByIdAsync(int id)
        {
            var line = await _productionLineRepo.GetProductionLineByIdAsync(id);
            return new ProductionLineDto
            {
                ProductionLineId = line.ProductionLineId,
                Name = line.Name,
                DepartmentId = line.DepartmentId ?? 0,
                DepartmentName = line.Department?.Name ?? "Not Assigned"
            };
        }

        public async Task<List<ProductionLineDto>> GetProductionLinesByDepartmentAsync(int departmentId)
        {
            var lines = await _productionLineRepo.GetProductionLinesByDepartmentAsync(departmentId);
            return lines.Select(l => new ProductionLineDto
            {
                ProductionLineId = l.ProductionLineId,
                Name = l.Name,
                DepartmentId = l.DepartmentId ?? 0,
                DepartmentName = l.Department?.Name ?? "Not Assigned"
            }).ToList();
        }

        public async Task<ProductionLineDto> AddProductionLineAsync(ProductionLineDto productionLineDto)
        {
            var line = new ProductionLine
            {
                Name = productionLineDto.Name,
                DepartmentId = productionLineDto.DepartmentId > 0 ? productionLineDto.DepartmentId : null
            };
            var newLine = await _productionLineRepo.AddProductionLineAsync(line);
            return new ProductionLineDto
            {
                ProductionLineId = newLine.ProductionLineId,
                Name = newLine.Name,
                DepartmentId = newLine.DepartmentId ?? 0
            };
        }

        public async Task<ProductionLineDto> UpdateProductionLineAsync(int id, ProductionLineDto productionLineDto)
        {
            var line = new ProductionLine
            {
                ProductionLineId = id,
                Name = productionLineDto.Name,
                DepartmentId = productionLineDto.DepartmentId > 0 ? productionLineDto.DepartmentId : null
            };
            var updatedLine = await _productionLineRepo.UpdateProductionLineAsync(id, line);
            return new ProductionLineDto
            {
                ProductionLineId = updatedLine.ProductionLineId,
                Name = updatedLine.Name,
                DepartmentId = updatedLine.DepartmentId ?? 0
            };
        }

        public async Task<bool> DeleteProductionLineAsync(int id)
        {
            return await _productionLineRepo.DeleteProductionLineAsync(id);
        }
    }
}
