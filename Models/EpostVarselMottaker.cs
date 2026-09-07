using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models;

public class EpostVarselMottaker
{
    public int Id { get; set; }

    public int EpostVarselId { get; set; }

    public EpostVarsel EpostVarsel { get; set; } = null!;

    public int BeboerId { get; set; }

    public Beboer Beboer { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Epostadresse { get; set; } = string.Empty;

    public DateTime? SendtDato { get; set; }

    public bool Sendt { get; set; }

    [StringLength(1000)]
    public string? Feilmelding { get; set; }
}