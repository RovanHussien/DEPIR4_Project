using DEPI.DAL.DbContext;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Implementation
{
    public class ProductionLineRepo : IProductionLineRepo
    {
        private readonly ApplicationDbContext _context;

        public ProductionLineRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductionLine>> GetAllProductionLinesAsync()
        {
            return await _context.ProductionLines.Include(p => p.Department).ToListAsync();
        }

        public async Task<ProductionLine> GetProductionLineByIdAsync(int id)
        {
            var productionLine = await _context.ProductionLines
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.ProductionLineId == id);
            if (productionLine == null)
                throw new Exception("Production line not found");
            return productionLine;
        }

        public async Task<List<ProductionLine>> GetProductionLinesByDepartmentAsync(int departmentId)
        {
            return await _context.ProductionLines
                .Where(p => p.DepartmentId == departmentId)
                .Include(p => p.Department)
                .ToListAsync();
        }

        public async Task<ProductionLine> AddProductionLineAsync(ProductionLine productionLine)
        {
            _context.ProductionLines.Add(productionLine);
            await _context.SaveChangesAsync();
            return productionLine;
        }

        public async Task<ProductionLine> UpdateProductionLineAsync(int id, ProductionLine productionLine)
        {
            var existingLine = await _context.ProductionLines.FirstOrDefaultAsync(p => p.ProductionLineId == id);
            if (existingLine == null)
                throw new Exception("Production line not found");

            existingLine.Name = productionLine.Name;
            existingLine.DepartmentId = productionLine.DepartmentId;

            _context.ProductionLines.Update(existingLine);
            await _context.SaveChangesAsync();
            return existingLine;
        }

        public async Task<bool> DeleteProductionLineAsync(int id)
        {
            var productionLine = await _context.ProductionLines.FirstOrDefaultAsync(p => p.ProductionLineId == id);
            if (productionLine == null)
                throw new Exception("Production line not found");

            _context.ProductionLines.Remove(productionLine);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
