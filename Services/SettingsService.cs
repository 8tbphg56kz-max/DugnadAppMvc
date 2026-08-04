using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc.Services;

public class SettingsService
{
    private readonly ApplicationDbContext _context;

    public SettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Innstillinger?> GetAsync()
    {
        return await _context.Innstillinger.FirstOrDefaultAsync();
    }
}