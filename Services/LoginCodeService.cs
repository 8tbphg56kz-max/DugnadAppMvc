using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DugnadAppMvc.Services;

public class LoginCodeService
{
    private readonly ApplicationDbContext _context;

    public LoginCodeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateCodeAsync(string epost)
    {
        // Fjern gamle eller brukte koder for denne e-postadressen
        var gamleKoder = await _context.LoginCodes
            .Where(x => x.Epost == epost &&
                   (x.Brukt || x.Utloper < DateTime.UtcNow))
            .ToListAsync();

        if (gamleKoder.Any())
        {
            _context.LoginCodes.RemoveRange(gamleKoder);
        }

        // Lag en tilfeldig 6-sifret kode
        var kode = RandomNumberGenerator
            .GetInt32(100000, 999999)
            .ToString();

        var loginCode = new LoginCode
        {
            Epost = epost,
            Kode = kode,
            Opprettet = DateTime.UtcNow,
            Utloper = DateTime.UtcNow.AddMinutes(10),
            Brukt = false
        };

        _context.LoginCodes.Add(loginCode);

        await _context.SaveChangesAsync();

        return kode;
    }

    public async Task<LoginCode?> ValidateCodeAsync(string epost, string kode)
    {
        var loginCode = await _context.LoginCodes
            .FirstOrDefaultAsync(x =>
                x.Epost == epost &&
                x.Kode == kode &&
                !x.Brukt &&
                x.Utloper > DateTime.UtcNow);

        return loginCode;
    }

    public async Task MarkAsUsedAsync(LoginCode loginCode)
    {
        loginCode.Brukt = true;

        await _context.SaveChangesAsync();
    }
}