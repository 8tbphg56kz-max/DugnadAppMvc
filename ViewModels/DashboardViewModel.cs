using DugnadAppMvc.Models;
namespace DugnadAppMvc.ViewModels
{
    public class DashboardViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public decimal TotalHours { get; set; }

        public int ActiveTasks { get; set; }

        public bool HasCommonDugnad { get; set; }

        public string? BoardMessage { get; set; }

        public List<Oppgave> MineOppgaver { get; set; } = new();

        public List<Oppgave> LedigeOppgaver { get; set; } = new();

        public int TotalActiveTasks { get; set; }

        public Dugnad? NesteDugnad { get; set; }

        public int AntallRegistreringerPaAktivDugnad { get; set; }
    }
}
