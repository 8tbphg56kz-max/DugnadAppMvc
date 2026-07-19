namespace DugnadAppMvc.ViewModels
{
    public class RapportTimerPrLeilighetViewModel
    {
        public int LeilighetId { get; set; }

        public string Leilighetsnummer { get; set; } = "";

        public int AntallRegistreringer { get; set; }

        public decimal TotaleTimer { get; set; }

        public decimal TotalVerdi { get; set; }
        public decimal Verdi { get; set; }

        // Nøkkeltall for rapporten
        public decimal Timeverdi { get; set; }

        public decimal TotaleTimerAlle { get; set; }

        public int Dugnadsbudsjett { get; set; }

        public string Visningsnavn { get; set; } = "";
    }
}