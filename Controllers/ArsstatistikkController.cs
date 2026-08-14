using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = IdentityRoles.AdminAccess)]
public class ArsstatistikkController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArsstatistikkController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var statistikk = await _context.Arsstatistikker
            .OrderByDescending(a => a.Aar)
            .ToListAsync();

        return View(statistikk);
    }
}