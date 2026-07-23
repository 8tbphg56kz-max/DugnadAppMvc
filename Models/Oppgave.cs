using System.ComponentModel.DataAnnotations;

public class Oppgave
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Navn")]
    public string Navn { get; set; } = string.Empty;

    [Display(Name = "Beskrivelse")]
    public string? Beskrivelse { get; set; }

    [Display(Name = "Fra dato")]
    [DataType(DataType.Date)]
    public DateTime FraDato { get; set; }

    [Display(Name = "Frist")]
    [DataType(DataType.Date)]
    public DateTime Frist { get; set; }

    [Display(Name = "Antall personer")]
    public int AntallPersoner { get; set; } = 1;

    [Display(Name = "Kan registrere timer")]
    public bool KanRegistrereTimer { get; set; } = true;

    [Display(Name = "Krever bekreftelse")]
    public bool KreverBekreftelse { get; set; } = true;

    [Display(Name = "Utstyr")]
    public string? Utstyr { get; set; }

    [Display(Name = "Oppbevaring av utstyr")]
    public string? UtstyrPlassering { get; set; }
}