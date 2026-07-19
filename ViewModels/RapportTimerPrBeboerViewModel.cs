namespace DugnadAppMvc.ViewModels
{
    public class RapportTimerPrBeboerViewModel
    {
        public int BeboerId { get; set; }

        public string Navn { get; set; } = string.Empty;

        public string Leilighetsnummer { get; set; } = string.Empty;

        public int AntallRegistreringer { get; set; }

        public decimal TotaleTimer { get; set; }

        public decimal TotalVerdi { get; set; }
    }
}