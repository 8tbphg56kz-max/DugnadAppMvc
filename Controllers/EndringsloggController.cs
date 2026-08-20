using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Controllers
{
    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public class EndringsloggController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EndringsloggController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logg = await _context.Endringslogger
                .Include(e => e.Bruker)
                .Include(e => e.Beboer)
                .OrderByDescending(e => e.Tidspunkt)
                .ToListAsync();

            return View(logg);
        }
    }
}