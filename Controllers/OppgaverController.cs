using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Models.Enums;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = IdentityRoles.BoardAccess)]
public class OppgaverController : Controller
{
    private readonly ApplicationDbContext _context;

    public OppgaverController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: OPPGAVES
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Oppgaver.ToListAsync());
    }

    // GET: OPPGAVES/Details/5
    public async Task<IActionResult> Details(int? id)
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

    // GET: OPPGAVES/Create
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

    // POST: OPPGAVES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

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

    // GET: OPPGAVES/Edit/5
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

    // POST: OPPGAVES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

    // GET: OPPGAVES/Delete/5
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

    // POST: OPPGAVES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var oppgave = await _context.Oppgaver.FindAsync(id);
        if (oppgave != null)
        {
            _context.Oppgaver.Remove(oppgave);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool OppgaveExists(int? id)
    {
        return _context.Oppgaver.Any(e => e.Id == id);
    }

    [Authorize]
    public async Task<IActionResult> Mine()
    {
        var oppgaver = await _context.Oppgaver
            .Where(o => !o.ErUtført)
            .OrderBy(o => o.Frist)
            .ToListAsync();

        return View(oppgaver);
    }
}
