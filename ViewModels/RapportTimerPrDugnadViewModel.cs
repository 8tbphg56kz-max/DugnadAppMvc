namespace DugnadAppMvc.ViewModels
{
    public class RapportTimerPrDugnadViewModel
    {
        public DateTime Dato { get; set; }

        public string Dugnad { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int Registreringer { get; set; }

        public decimal Timer { get; set; }

        public decimal Verdi { get; set; }
    }
}