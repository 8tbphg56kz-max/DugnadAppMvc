namespace DugnadAppMvc.ViewModels
{
    public class PameldingStatusViewModel
    {
        public int PameldingId { get; set; }

        public int BeboerId { get; set; }

        public string Navn { get; set; } = string.Empty;

        public string? Leilighetsnummer { get; set; }

        public DateTime PameldtDato { get; set; }

        public DateTime? UtfortDato { get; set; }

        public bool ErUtfort { get; set; }

        public bool HarRegistrertTimer { get; set; }

        public decimal? AntallTimer { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}