using DugnadAppMvc.Data;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.Models.Enums;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DugnadAppMvc.Helpers;

public class OppgaverController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OppgaverController(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(OppgaveIndexViewModel model)
    {
        var query = _context.Oppgaver
            .Include(o => o.Pameldinger)
            .AsQueryable();

        if (model.ErUtfort.HasValue)
        {
            query = query.Where(o => o.ErUtført == model.ErUtfort.Value);
        }

        model.Oppgaver = await query
            .OrderBy(o => o.Prioritet)
            .ThenBy(o => o.Frist)
            .ToListAsync();

        return View(model);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public IActionResult Create()
    {
        var model = new Oppgave
        {
            FraDato = DateTime.Today,
            Frist = DateTime.Today.AddDays(14),
            Prioritet = OppgavePrioritet.Normal,
            AntallPersoner = 1,
            KanRegistrereTimer = true,
            KreverBekreftelse = true
        };

        return View(model);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Navn,Beskrivelse,FraDato,Frist,AntallPersoner,KanRegistrereTimer,KreverBekreftelse,Utstyr,UtstyrPlassering,Prioritet")] Oppgave oppgave)
    {
        if (ModelState.IsValid)
        {
            oppgave.FraDato = DateTime.SpecifyKind(oppgave.FraDato, DateTimeKind.Utc);
            oppgave.Frist = DateTime.SpecifyKind(oppgave.Frist, DateTimeKind.Utc);

            oppgave.ErUtført = false;
            oppgave.Opprettet = DateTime.UtcNow;

            _context.Add(oppgave);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(oppgave);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var oppgave = await _context.Oppgaver.FindAsync(id);
        if (oppgave == null)
        {
            return NotFound();
        }
        return View(oppgave);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Navn,Beskrivelse,FraDato,Frist,AntallPersoner,KanRegistrereTimer,KreverBekreftelse,Utstyr,UtstyrPlassering,Prioritet,ErUtført")] Oppgave oppgave)
    {
        if (id != oppgave.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                oppgave.FraDato = DateTime.SpecifyKind(oppgave.FraDato, DateTimeKind.Utc);
                oppgave.Frist = DateTime.SpecifyKind(oppgave.Frist, DateTimeKind.Utc);

                _context.Update(oppgave);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OppgaveExists(oppgave.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(oppgave);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var oppgave = await _context.Oppgaver
            .FirstOrDefaultAsync(m => m.Id == id);
        if (oppgave == null)
        {
            return NotFound();
        }

        return View(oppgave);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var oppgave = await _context.Oppgaver.FindAsync(id);

        if (oppgave == null)
        {
            return NotFound();
        }

        // Sjekk om oppgaven har timeføringer
        bool harTimeforinger = await _context.Timeforinger
            .AnyAsync(t => t.OppgaveId == id);

        if (harTimeforinger)
        {
            TempData["Error"] =
                "Oppgaven kan ikke slettes fordi det finnes registrerte timeføringer på den.";

            return RedirectToAction(nameof(Delete), new { id });
        }

        _context.Oppgaver.Remove(oppgave);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Oppgaven ble slettet.";

        return RedirectToAction(nameof(Index));
    }

    private bool OppgaveExists(int id)
    {
        return _context.Oppgaver.Any(e => e.Id == id);
    }

    [Authorize]
    public async Task<IActionResult> Mine()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return Challenge();
        }

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
        {
            return NotFound();
        }

        var oppgaver = await _context.Oppgaver
            .Include(o => o.Pameldinger)
            .Where(o => !o.ErUtført)
            .OrderBy(o => o.Frist)
            .ToListAsync();

        var model = oppgaver.Select(o => new OppgaveMineViewModel
        {
            Oppgave = o,
            AntallPameldte = o.Pameldinger.Count,
            ErPameldt = o.Pameldinger.Any(p => p.BeboerId == beboer.Id)
        }).ToList();

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Vis(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
            return NotFound();

        var oppgave = await _context.Oppgaver
            .Include(o => o.Pameldinger)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (oppgave == null)
            return NotFound();

        var pamelding = oppgave.Pameldinger
    .FirstOrDefault(p => p.BeboerId == beboer.Id);

        ViewBag.ErPameldt = pamelding != null;
        ViewBag.Pamelding = pamelding;

        return View(oppgave);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]    
    public async Task<IActionResult> MeldPa(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
            return NotFound();

        var oppgave = await _context.Oppgaver
            .Include(o => o.Pameldinger)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (oppgave == null)
            return NotFound();

        // Er brukeren allerede påmeldt?
        if (oppgave.Pameldinger.Any(p => p.BeboerId == beboer.Id))
        {
            TempData["Info"] = "Du er allerede påmeldt denne oppgaven.";
            return RedirectToAction(nameof(Vis), new { id });
        }

        // Er oppgaven full?
        if (oppgave.Pameldinger.Count >= oppgave.AntallPersoner)
        {
            TempData["Error"] = "Oppgaven er fulltegnet.";
            return RedirectToAction(nameof(Vis), new { id });
        }

        _context.OppgavePameldinger.Add(new OppgavePamelding
        {
            OppgaveId = oppgave.Id,
            BeboerId = beboer.Id,
            PameldtDato = DateTime.UtcNow,
            Status = OppgaveStatus.Pameldt
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Du er nå påmeldt oppgaven.";

        return RedirectToAction(nameof(Vis), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrekkPamelding(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
            return NotFound();

        var pamelding = await _context.OppgavePameldinger
            .FirstOrDefaultAsync(p =>
                p.OppgaveId == id &&
                p.BeboerId == beboer.Id);

        if (pamelding != null)
        {
            _context.OppgavePameldinger.Remove(pamelding);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Vis), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrekkPameldingFraMine(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
            return NotFound();

        var pamelding = await _context.OppgavePameldinger
            .FirstOrDefaultAsync(p =>
                p.OppgaveId == id &&
                p.BeboerId == beboer.Id);

        if (pamelding != null)
        {
            _context.OppgavePameldinger.Remove(pamelding);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MeldPaFraMine(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
            return NotFound();

        var oppgave = await _context.Oppgaver
            .Include(o => o.Pameldinger)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (oppgave == null)
            return NotFound();

        if (!oppgave.Pameldinger.Any(p => p.BeboerId == beboer.Id)
            && oppgave.Pameldinger.Count < oppgave.AntallPersoner)
        {
            _context.OppgavePameldinger.Add(new OppgavePamelding
            {
                OppgaveId = oppgave.Id,
                BeboerId = beboer.Id,
                PameldtDato = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkerSomUtfort(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
            return NotFound();

        var pamelding = await _context.OppgavePameldinger
            .FirstOrDefaultAsync(p =>
                p.OppgaveId == id &&
                p.BeboerId == beboer.Id);

        if (pamelding == null)
        {
            TempData["Error"] = "Du er ikke påmeldt denne oppgaven.";
            return RedirectToAction(nameof(Vis), new { id });
        }

        if (pamelding.Status == OppgaveStatus.Utfort)
        {
            TempData["Info"] = "Oppgaven er allerede markert som utført.";
            return RedirectToAction(nameof(Vis), new { id });
        }

        pamelding.Status = OppgaveStatus.Utfort;
        pamelding.UtfortDato = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "✔ Oppgaven er utført. Du kan nå registrere timer.";

        return RedirectToAction(nameof(Vis), new { id });
    }

    [Authorize]
    public async Task<IActionResult> RegistrerTimer(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
            return NotFound();

        var oppgave = await _context.Oppgaver
            .FirstOrDefaultAsync(o => o.Id == id);

        if (oppgave == null)
            return NotFound();

        var model = new RegistrerTimerViewModel
        {
            OppgaveId = oppgave.Id,
            OppgaveNavn = oppgave.Navn,
            TimerAlternativer = TimerAlternativerHelper.Hent()
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrerTimer(RegistrerTimerViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!decimal.TryParse(
        model.AntallTimer,
        System.Globalization.NumberStyles.Number,
        System.Globalization.CultureInfo.InvariantCulture,
        out var antallTimer))
        {
            ModelState.AddModelError(nameof(model.AntallTimer), "Ugyldig timetall.");
            return View(model);
        }

        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
            return Challenge();

        var beboer = await _context.Beboere
            .FirstOrDefaultAsync(b => b.ApplicationUserId == currentUser.Id);

        if (beboer == null)
            return NotFound();

        var pamelding = await _context.OppgavePameldinger
            .FirstOrDefaultAsync(p =>
                p.OppgaveId == model.OppgaveId &&
                p.BeboerId == beboer.Id);

        if (pamelding == null)
            return NotFound();

        if (pamelding.Status != OppgaveStatus.Utfort)
        {
            return Forbid();
        }

        _context.Timeforinger.Add(new Timeforing
        {
            OppgaveId = model.OppgaveId,
            BeboerId = beboer.Id,
            AntallTimer = antallTimer,
            Kommentar = model.Kommentar,
            RegistrertDato = DateTime.UtcNow
        });

        pamelding.Status = OppgaveStatus.TimerRegistrert;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Timene er registrert.";

        return RedirectToAction(nameof(Vis), new { id = model.OppgaveId });
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public async Task<IActionResult> AdministrerPameldinger(int id)
    {
        var oppgave = await _context.Oppgaver
            .Include(o => o.Pameldinger)
                .ThenInclude(p => p.Beboer)
                    .ThenInclude(b => b.Leilighet)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (oppgave == null)
        {
            return NotFound();
        }

        var pameldteIder = oppgave.Pameldinger
            .Select(p => p.BeboerId)
            .ToList();

        var ledigeBeboere = await _context.Beboere
            .Where(b => !pameldteIder.Contains(b.Id))
            .OrderBy(b => b.Etternavn)
            .ThenBy(b => b.Fornavn)
            .ToListAsync();

        // Hent timeføringer for denne oppgaven
        var timeforinger = await _context.Timeforinger
            .Where(t => t.OppgaveId == id)
            .ToListAsync();

        var pameldingStatus = oppgave.Pameldinger
            .Select(p =>
            {
                var timer = timeforinger
                    .Where(t => t.BeboerId == p.BeboerId)
                    .ToList();

                var harTimer = timer.Any();

                var antallTimer = harTimer
                    ? timer.Sum(t => t.AntallTimer)
                    : (decimal?)null;

                string status;

                if (p.Status == OppgaveStatus.TimerRegistrert && harTimer)
                {
                    status = "Fullført";
                }
                else if (p.Status == OppgaveStatus.Utfort)
                {
                    status = "Utført";
                }
                else
                {
                    status = "Påmeldt";
                }

                return new PameldingStatusViewModel
                {
                    PameldingId = p.Id,
                    BeboerId = p.BeboerId,

                    Navn = $"{p.Beboer.Fornavn} {p.Beboer.Etternavn}",

                    Leilighetsnummer = p.Beboer.Leilighet?.Leilighetsnummer,

                    PameldtDato = p.PameldtDato,

                    UtfortDato = p.UtfortDato,

                    ErUtfort = p.Status == OppgaveStatus.Utfort
                               || p.Status == OppgaveStatus.TimerRegistrert,

                    HarRegistrertTimer = harTimer,

                    AntallTimer = antallTimer,

                    Status = status
                };
            })
            .ToList();

        var model = new AdministrerPameldingerViewModel
        {
            Oppgave = oppgave,

            LedigeBeboere = ledigeBeboere
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Fornavn + " " + b.Etternavn
                })
                .ToList(),

            PameldingStatus = pameldingStatus
        };

        return View(model);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MeldAvSomAdministrator(int pameldingId)
    {
        var pamelding = await _context.OppgavePameldinger
            .FirstOrDefaultAsync(p => p.Id == pameldingId);

        if (pamelding == null)
        {
            return NotFound();
        }

        var oppgaveId = pamelding.OppgaveId;

        // Sjekk om det finnes registrerte timer på oppgaven for denne beboeren
        var harTimer = await _context.Timeforinger.AnyAsync(t =>
            t.OppgaveId == pamelding.OppgaveId &&
            t.BeboerId == pamelding.BeboerId);

        if (harTimer)
        {
            TempData["ErrorMessage"] =
                "Kan ikke melde av beboeren fordi det er registrert timer på oppgaven.";

            return RedirectToAction(nameof(AdministrerPameldinger), new { id = oppgaveId });
        }

        _context.OppgavePameldinger.Remove(pamelding);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Beboeren ble meldt av.";

        return RedirectToAction(nameof(AdministrerPameldinger), new { id = oppgaveId });
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MeldPaSomAdministrator(int oppgaveId, int? valgtBeboerId)
    {

        if (!valgtBeboerId.HasValue)
        {
            TempData["Error"] = "Du må velge en beboer.";
            return RedirectToAction(nameof(AdministrerPameldinger), new { id = oppgaveId });
        }


        var oppgave = await _context.Oppgaver
            .Include(o => o.Pameldinger)
            .FirstOrDefaultAsync(o => o.Id == oppgaveId);

        if (oppgave == null)
        {
            return NotFound();
        }

        // Finnes beboeren?
        var beboer = await _context.Beboere
            .FindAsync(valgtBeboerId);

        if (beboer == null)
        {
            return NotFound();
        }

        // Allerede påmeldt?
        if (oppgave.Pameldinger.Any(p => p.BeboerId == valgtBeboerId))
        {
            TempData["Info"] = "Beboeren er allerede påmeldt.";
            return RedirectToAction(nameof(AdministrerPameldinger), new { id = oppgaveId });
        }

        // Full oppgave?
        if (oppgave.Pameldinger.Count >= oppgave.AntallPersoner)
        {
            TempData["Error"] = "Oppgaven er fulltegnet.";
            return RedirectToAction(nameof(AdministrerPameldinger), new { id = oppgaveId });
        }

        _context.OppgavePameldinger.Add(new OppgavePamelding
        {
            OppgaveId = oppgaveId,
            BeboerId = valgtBeboerId.Value,
            PameldtDato = DateTime.UtcNow,
            Status = OppgaveStatus.Pameldt
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Beboeren ble meldt på.";

        return RedirectToAction(nameof(AdministrerPameldinger), new { id = oppgaveId });
    }
}
