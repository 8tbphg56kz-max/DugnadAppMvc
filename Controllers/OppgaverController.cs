using DugnadAppMvc.Data;
using DugnadAppMvc.Helpers;
using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models;
using DugnadAppMvc.Models.Enums;
using DugnadAppMvc.Services;
using DugnadAppMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class OppgaverController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly OppgaveBildeService _bildeService;

    public OppgaverController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        OppgaveBildeService bildeService)
    {
        _context = context;
        _userManager = userManager;
        _bildeService = bildeService;
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
    KanUtføresFlereGanger = false
};

        return View(model);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("Id,Navn,Beskrivelse,FraDato,Frist,AntallPersoner,KanRegistrereTimer,KanUtføresFlereGanger,Utstyr,UtstyrPlassering,Prioritet")] Oppgave oppgave,
    List<IFormFile>? bilder)
    {
        if (ModelState.IsValid)
        {
            oppgave.FraDato =
                DateTime.SpecifyKind(
                    oppgave.FraDato,
                    DateTimeKind.Utc);

            oppgave.Frist =
                DateTime.SpecifyKind(
                    oppgave.Frist,
                    DateTimeKind.Utc);

            oppgave.ErUtført = false;
            oppgave.Opprettet = DateTime.UtcNow;

            _context.Add(oppgave);

            await _context.SaveChangesAsync();

            try
            {
                var lagredeBilder =
                    await _bildeService.LagreBilderAsync(
                        oppgave.Id,
                        bilder);

                if (lagredeBilder.Count > 0)
                {
                    _context.OppgaveBilder.AddRange(lagredeBilder);

                    await _context.SaveChangesAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);

                _context.Oppgaver.Remove(oppgave);
                await _context.SaveChangesAsync();

                return View(oppgave);
            }

            return RedirectToAction(nameof(Index));
        }

        return View(oppgave);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
     int? id,
     [Bind("Id,Navn,Beskrivelse,FraDato,Frist,AntallPersoner,KanRegistrereTimer,KanUtføresFlereGanger,Utstyr,UtstyrPlassering,Prioritet,ErUtført")] Oppgave oppgave,
     List<IFormFile>? bilder)
    {
        if (id != oppgave.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            oppgave.Bilder = await _context.OppgaveBilder
                .Where(b => b.OppgaveId == oppgave.Id)
                .ToListAsync();

            return View(oppgave);
        }

        try
        {
            oppgave.FraDato =
                DateTime.SpecifyKind(
                    oppgave.FraDato,
                    DateTimeKind.Utc);

            oppgave.Frist =
                DateTime.SpecifyKind(
                    oppgave.Frist,
                    DateTimeKind.Utc);

            // Hent eksisterende oppgave slik at vi ikke overskriver
            // navigasjonsegenskaper eller andre data.
            var eksisterendeOppgave =
                await _context.Oppgaver
                    .FirstOrDefaultAsync(o => o.Id == oppgave.Id);

            if (eksisterendeOppgave == null)
            {
                return NotFound();
            }

            eksisterendeOppgave.Navn = oppgave.Navn;
            eksisterendeOppgave.Beskrivelse = oppgave.Beskrivelse;
            eksisterendeOppgave.FraDato = oppgave.FraDato;
            eksisterendeOppgave.Frist = oppgave.Frist;
            eksisterendeOppgave.AntallPersoner = oppgave.AntallPersoner;
            eksisterendeOppgave.KanRegistrereTimer = oppgave.KanRegistrereTimer;
            eksisterendeOppgave.KanUtføresFlereGanger = oppgave.KanUtføresFlereGanger;
            eksisterendeOppgave.Utstyr = oppgave.Utstyr;
            eksisterendeOppgave.UtstyrPlassering = oppgave.UtstyrPlassering;
            eksisterendeOppgave.Prioritet = oppgave.Prioritet;
            eksisterendeOppgave.ErUtført = oppgave.ErUtført;

            await _context.SaveChangesAsync();

            try
            {
                var eksisterendeAntall =
                    await _context.OppgaveBilder
                        .CountAsync(b => b.OppgaveId == oppgave.Id);

                var nyeBilder =
                    bilder?
                        .Where(f => f != null && f.Length > 0)
                        .ToList()
                    ?? new List<IFormFile>();

                if (eksisterendeAntall + nyeBilder.Count > 5)
                {
                    throw new InvalidOperationException(
                        $"Oppgaven kan ha maksimalt 5 bilder. " +
                        $"Det finnes allerede {eksisterendeAntall} bilder.");
                }

                var lagredeBilder =
                    await _bildeService.LagreBilderAsync(
                        oppgave.Id,
                        nyeBilder);

                if (lagredeBilder.Count > 0)
                {
                    _context.OppgaveBilder.AddRange(lagredeBilder);
                    await _context.SaveChangesAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);

                oppgave.Bilder = await _context.OppgaveBilder
                    .Where(b => b.OppgaveId == oppgave.Id)
                    .ToListAsync();

                return View(oppgave);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OppgaveExists(oppgave.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var oppgave = await _context.Oppgaver
            .Include(o => o.Bilder)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (oppgave == null)
        {
            return NotFound();
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

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var oppgave = await _context.Oppgaver
            .FirstOrDefaultAsync(o => o.Id == id);

        if (oppgave == null)
            return NotFound();

        bool harTimeforinger = await _context.Timeforinger
            .AnyAsync(t => t.OppgaveId == id);

        if (harTimeforinger)
        {
            TempData["Error"] =
                "Oppgaven kan ikke slettes fordi det finnes registrerte timeføringer på den.";

            return RedirectToAction(nameof(Delete), new { id });
        }

        // Hent alle bilder som tilhører oppgaven
        var bilder = await _context.OppgaveBilder
            .Where(b => b.OppgaveId == id)
            .ToListAsync();

        // Slett de fysiske bildefilene
        foreach (var bilde in bilder)
        {
            _bildeService.SlettBilde(bilde);
        }

        // Endringslogger som peker på oppgaven må frikobles først
        var endringslogger = await _context.Endringslogger
            .Where(e => e.OppgaveId == id)
            .ToListAsync();

        foreach (var logg in endringslogger)
        {
            logg.OppgaveId = null;
        }

        // Oppgaven slettes.
        // OppgaveBilder slettes automatisk fra databasen
        // på grunn av Cascade-relasjonen.
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
          .Include(o => o.Bilder)
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

        var endringslogg = new Endringslogg
        {
            Tidspunkt = DateTime.UtcNow,
            BrukerId = currentUser.Id,
            Handling = "Påmeldt av beboer",
            Begrunnelse = "Beboeren meldte seg selv på oppgaven.",
            OppgaveId = oppgave.Id,
            BeboerId = beboer.Id,
            Aktivitet = oppgave.Navn
        };

        _context.Endringslogger.Add(endringslogg);

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
        {
            model.TimerAlternativer = TimerAlternativerHelper.Hent();
            return View(model);
        }

        if (!decimal.TryParse(
            model.AntallTimer,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var antallTimer))
        {
            ModelState.AddModelError(
                nameof(model.AntallTimer),
                "Ugyldig timetall.");

            model.TimerAlternativer = TimerAlternativerHelper.Hent();
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
            .Include(p => p.Oppgave)
            .FirstOrDefaultAsync(p =>
                p.OppgaveId == model.OppgaveId &&
                p.BeboerId == beboer.Id);

        if (pamelding == null)
            return NotFound();

        var oppgave = pamelding.Oppgave;

        // Gjentakende oppgave:
        // Påmeldingen skal bestå, og hver registrering
        // oppretter en ny timeføring.
        if (oppgave.KanUtføresFlereGanger)
        {
            if (!oppgave.KanRegistrereTimer)
            {
                TempData["Error"] = "Det er ikke mulig å registrere timer på denne oppgaven.";
                return RedirectToAction(nameof(Vis), new { id = model.OppgaveId });
            }

            _context.Timeforinger.Add(new Timeforing
            {
                OppgaveId = model.OppgaveId,
                BeboerId = beboer.Id,
                AntallTimer = antallTimer,
                Kommentar = model.Kommentar,
                RegistrertDato = DateTime.UtcNow
            });

            // Status beholdes som Pameldt.
            // Beboeren kan derfor registrere timer igjen senere.

            await _context.SaveChangesAsync();

            TempData["Success"] = "Timene er registrert.";
            return RedirectToAction(nameof(Vis), new { id = model.OppgaveId });
        }

        // Vanlig engangsoppgave:
        // Behold dagens eksisterende flyt.
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
    public async Task<IActionResult> MeldAvSomAdministrator(
    int pameldingId,
    string? begrunnelse)
    {
        if (string.IsNullOrWhiteSpace(begrunnelse))
        {
            TempData["ErrorMessage"] =
                "Du må oppgi en begrunnelse for avmeldingen.";

            return RedirectToAction(
                nameof(AdministrerPameldinger),
                new
                {
                    id = await _context.OppgavePameldinger
                        .Where(p => p.Id == pameldingId)
                        .Select(p => p.OppgaveId)
                        .FirstOrDefaultAsync()
                });
        }

        var pamelding = await _context.OppgavePameldinger
            .Include(p => p.Oppgave)
            .Include(p => p.Beboer)
            .FirstOrDefaultAsync(p => p.Id == pameldingId);

        if (pamelding == null)
            return NotFound();

        var oppgaveId = pamelding.OppgaveId;

        var harTimer = await _context.Timeforinger
            .AnyAsync(t =>
                t.OppgaveId == oppgaveId &&
                t.BeboerId == pamelding.BeboerId);

        // For vanlige engangsoppgaver skal vi fortsatt
        // hindre avmelding dersom det finnes timer.
        //
        // For gjentakende oppgaver kan beboeren meldes av
        // selv om det finnes tidligere timeføringer.
        if (harTimer && !pamelding.Oppgave.KanUtføresFlereGanger)
        {
            TempData["ErrorMessage"] =
                "Kan ikke melde av beboeren fordi det er registrert timer på oppgaven.";

            return RedirectToAction(
                nameof(AdministrerPameldinger),
                new { id = oppgaveId });
        }

        var brukerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(brukerId))
            return Forbid();

        var endringslogg = new Endringslogg
        {
            Tidspunkt = DateTime.UtcNow,
            BrukerId = brukerId,
            Handling = "Avmeldt av styret",
            Begrunnelse = begrunnelse.Trim(),
            OppgaveId = oppgaveId,
            BeboerId = pamelding.BeboerId,
            Aktivitet = pamelding.Oppgave?.Navn
        };

        _context.Endringslogger.Add(endringslogg);

        // Selve påmeldingen fjernes.
        // Eventuelle tidligere timeføringer beholdes.
        _context.OppgavePameldinger.Remove(pamelding);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Beboeren ble meldt av.";

        return RedirectToAction(
            nameof(AdministrerPameldinger),
            new { id = oppgaveId });
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MeldPaSomAdministrator(
    int oppgaveId,
    int? valgtBeboerId,
    string? begrunnelse)
    {
        if (!valgtBeboerId.HasValue)
        {
            TempData["Error"] = "Du må velge en beboer.";

            return RedirectToAction(
                nameof(AdministrerPameldinger),
                new { id = oppgaveId });
        }

        if (string.IsNullOrWhiteSpace(begrunnelse))
        {
            TempData["Error"] =
                "Du må oppgi en begrunnelse for påmeldingen.";

            return RedirectToAction(
                nameof(AdministrerPameldinger),
                new { id = oppgaveId });
        }

        var oppgave = await _context.Oppgaver
            .Include(o => o.Pameldinger)
            .FirstOrDefaultAsync(o => o.Id == oppgaveId);

        if (oppgave == null)
        {
            return NotFound();
        }

        var beboer = await _context.Beboere
            .FindAsync(valgtBeboerId.Value);

        if (beboer == null)
        {
            return NotFound();
        }

        if (oppgave.Pameldinger.Any(p => p.BeboerId == valgtBeboerId.Value))
        {
            TempData["Info"] =
                "Beboeren er allerede påmeldt.";

            return RedirectToAction(
                nameof(AdministrerPameldinger),
                new { id = oppgaveId });
        }

        if (oppgave.Pameldinger.Count >= oppgave.AntallPersoner)
        {
            TempData["Error"] =
                "Oppgaven er fulltegnet.";

            return RedirectToAction(
                nameof(AdministrerPameldinger),
                new { id = oppgaveId });
        }

        // Hent innlogget bruker direkte fra Identity-claim
        var brukerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(brukerId))
        {
            return Forbid();
        }

        // Opprett påmelding
        _context.OppgavePameldinger.Add(new OppgavePamelding
        {
            OppgaveId = oppgaveId,
            BeboerId = valgtBeboerId.Value,
            PameldtDato = DateTime.UtcNow,
            Status = OppgaveStatus.Pameldt
        });

        // Opprett endringslogg
        var endringslogg = new Endringslogg
        {
            Tidspunkt = DateTime.UtcNow,
            BrukerId = brukerId,
            Handling = "Påmeldt av styret",
            Begrunnelse = begrunnelse.Trim(),
            OppgaveId = oppgaveId,
            BeboerId = beboer.Id,
            Aktivitet = oppgave.Navn
        };

        _context.Endringslogger.Add(endringslogg);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Beboeren ble meldt på.";

        return RedirectToAction(
            nameof(AdministrerPameldinger),
            new { id = oppgaveId });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Bilde(int id)
    {
        var bilde = await _context.OppgaveBilder
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bilde == null)
            return NotFound();

        var filbane = _bildeService.HentFilbane(bilde.Filnavn);

        if (!System.IO.File.Exists(filbane))
            return NotFound();

        var utvidelse = Path.GetExtension(bilde.Filnavn)
            .ToLowerInvariant();

        var contentType = utvidelse switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filbane, contentType);
    }

    [Authorize(Roles = IdentityRoles.BoardAccess)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SlettBilde(int id)
    {
        var bilde = await _context.OppgaveBilder
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bilde == null)
        {
            return NotFound();
        }

        var oppgaveId = bilde.OppgaveId;

        // Slett fysisk bildefil
        _bildeService.SlettBilde(bilde);

        // Slett databasepost
        _context.OppgaveBilder.Remove(bilde);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Bildet ble slettet.";

        return RedirectToAction(nameof(Edit), new { id = oppgaveId });
    }
}
