using DugnadAppMvc.Infrastructure.Identity;
using DugnadAppMvc.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DugnadAppMvc.Models
{
    public class Oppgave : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Oppgaven må ha et navn.")]
        [StringLength(100, ErrorMessage = "Navnet kan ikke være lengre enn 100 tegn.")]
        [Display(Name = "Navn")]
        public string Navn { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Beskrivelsen kan ikke være lengre enn 2000 tegn.")]
        [Display(Name = "Beskrivelse")]
        public string? Beskrivelse { get; set; }

        [Required(ErrorMessage = "Fra dato må angis.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fra dato")]
        public DateTime FraDato { get; set; }

        [Required(ErrorMessage = "Frist må angis.")]
        [DataType(DataType.Date)]
        [Display(Name = "Frist")]
        public DateTime Frist { get; set; }

        [Range(1, 100,
            ErrorMessage = "Antall personer må være mellom 1 og 100.")]
        [Display(Name = "Antall personer")]
        public int AntallPersoner { get; set; } = 1;

        [Display(Name = "Kan registrere timer")]
        public bool KanRegistrereTimer { get; set; } = true;

        [Display(Name = "Krever bekreftelse")]
        public bool KreverBekreftelse { get; set; } = true;

        [StringLength(500, ErrorMessage = "Utstyr kan ikke være lengre enn 500 tegn.")]
        [Display(Name = "Utstyr")]
        public string? Utstyr { get; set; }

        [StringLength(500, ErrorMessage = "Oppbevaring av utstyr kan ikke være lengre enn 500 tegn.")]
        [Display(Name = "Oppbevaring av utstyr")]
        public string? UtstyrPlassering { get; set; }

        [Display(Name = "Utført")]
        public bool ErUtført { get; set; }

        [Display(Name = "Prioritet")]
        public OppgavePrioritet Prioritet { get; set; } = OppgavePrioritet.Normal;

        [Display(Name = "Opprettet")]
        public DateTime Opprettet { get; set; } = DateTime.UtcNow;

        public string? OpprettetAvId { get; set; }
        public ApplicationUser? OpprettetAv { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Frist < FraDato)
            {
                yield return new ValidationResult(
                    "Frist kan ikke være tidligere enn fra dato.",
                    new[] { nameof(Frist) });
            }
        }

        [NotMapped]
        public bool ErPameldt { get; set; }

        public ICollection<OppgavePamelding> Pameldinger { get; set; }
    = new List<OppgavePamelding>();
    }
}