using DEPI.DAL.DbContext;
using DEPI.DAL.Model;
using DEPI.DAL.Repo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEPI.DAL.Repo.Implementation
{
    public class SwapRequestRepo : ISwapRequestRepo
    {
        private readonly ApplicationDbContext _context;

        public SwapRequestRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddSwapRequestAsync(SwapRequest request)
        {
            try
            {
                await _context.SwapRequests.AddAsync(request);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<SwapRequest> GetSwapRequestByIdAsync(int swapId)
        {
            return await _context.SwapRequests.FindAsync(swapId);
        }

        public async Task<bool> UpdateSwapRequestAsync(SwapRequest swapRequest)
        {
            _context.SwapRequests.Update(swapRequest);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
