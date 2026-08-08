using DugnadAppMvc.Models;

public class KontoadministrasjonViewModel
{
    public string BrukerId { get; set; } = string.Empty;

    public string Navn { get; set; } = string.Empty;

    public string Epost { get; set; } = string.Empty;

    public bool ErAktiv { get; set; }

    public bool EpostBekreftet { get; set; }

    public bool ErLåst { get; set; }

    public DateTime? SisteInnlogging { get; set; }

    public IList<string> Roller { get; set; } = new List<string>();

    public List<ApplicationUser> VentendeKontoer { get; set; } = new();
}