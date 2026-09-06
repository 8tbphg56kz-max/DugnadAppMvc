using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models;

public class DugnadBilde
{
    public int Id { get; set; }

    public int DugnadId { get; set; }

    public Dugnad Dugnad { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Filnavn { get; set; } = string.Empty;

    [StringLength(255)]
    public string? OriginaltFilnavn { get; set; }

    public DateTime LastetOpp { get; set; } = DateTime.UtcNow;
}