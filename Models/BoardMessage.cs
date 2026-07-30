using System.ComponentModel.DataAnnotations;

public class BoardMessage
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Tittel { get; set; } = string.Empty;

    [Required]
    public string Innhold { get; set; } = string.Empty;

    public DateTime PublisertDato { get; set; } = DateTime.UtcNow;
}