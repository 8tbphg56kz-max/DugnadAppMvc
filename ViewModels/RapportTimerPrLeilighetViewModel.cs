namespace DugnadAppMvc.ViewModels
{
    public class RapportTimerPrLeilighetViewModel
    {
        public int LeilighetId { get; set; }

        public string Leilighetsnummer { get; set; } = string.Empty;

        public int AntallRegistreringer { get; set; }

        public decimal TotaleTimer { get; set; } 
    }
}