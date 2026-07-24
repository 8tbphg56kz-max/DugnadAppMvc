using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Services
{
    public class SameieService
    {
        private readonly ApplicationDbContext _context;

        public SameieService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Innstillinger?> GetInnstillingerAsync()
        {
            return await _context.Innstillinger
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }
    }
}