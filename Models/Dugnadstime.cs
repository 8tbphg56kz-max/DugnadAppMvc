namespace DugnadAppMvc.Models
{
    public class Dugnadstime
    {
        public int Id { get; set; }

        // Hvilken dugnad timene gjelder
        public int DugnadId { get; set; }
        public Dugnad Dugnad { get; set; } = null!;

        // Hvem som registrerte timene
        public int BeboerId { get; set; }
        public Beboer Beboer { get; set; } = null!;

        // Antall timer
        public decimal Timer { get; set; }

        // Når timene ble registrert
        public DateTime Registrert { get; set; } = DateTime.UtcNow;

        // Valgfri kommentar
        public string? Kommentar { get; set; }
    }
}