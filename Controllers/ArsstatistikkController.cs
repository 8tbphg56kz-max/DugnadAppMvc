using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = IdentityRoles.BoardAccess)]
public class ArsstatistikkController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArsstatistikkController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var arsstatistikker = await _context.Arsstatistikker
            .OrderByDescending(a => a.Aar)
            .ToListAsync();

        var byggStatistikk = await _context.ArsstatistikkBygg
            .OrderByDescending(b => b.Aar)
            .ThenBy(b => b.ByggKode)
            .ToListAsync();

        var model = new ArsstatistikkViewModel
        {
            Arsstatistikker = arsstatistikker,
            ByggStatistikk = byggStatistikk
        };

        return View(model);
    }
}