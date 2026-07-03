namespace DugnadAppMvc.Models
{
    public class Deltakelse
    {
        public int Id { get; set; }

        // Dugnaden
        public int DugnadId { get; set; }
        public Dugnad Dugnad { get; set; } = null!;

        // Beboeren
        public int BeboerId { get; set; }
        public Beboer Beboer { get; set; } = null!;

        // Når deltakelsen ble registrert
        public DateTime Registrert { get; set; } = DateTime.UtcNow;

        // Eventuell kommentar
        public string? Kommentar { get; set; }
    }
}