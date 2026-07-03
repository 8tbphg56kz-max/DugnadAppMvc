using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace DugnadAppMvc.Models
{
    public class Dugnad
    {
        public int Id { get; set; }

        public string Tittel { get; set; } = "";

        public string? Beskrivelse { get; set; }

        public DateOnly StartDato { get; set; }

        public DateOnly SluttDato { get; set; }

        public bool KreverPamelding { get; set; }

        public int? MaksAntallDeltakere { get; set; }

        public bool ErSynlig { get; set; } = true;

        [ValidateNever]
        public ICollection<Deltakelse> Deltakelser { get; set; } = new List<Deltakelse>();
    }
}