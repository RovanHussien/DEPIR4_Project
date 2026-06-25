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
    public class VacationRequestRepo : IVacationRequestRepo
    {
        private readonly ApplicationDbContext _context;

       
        public VacationRequestRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddVacationRequestAsync(VacationRequest request)
        {
            try
            {
                
                await _context.VacationRequests.AddAsync(request);

                
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                
                return false;
            }
        }
    }
}
