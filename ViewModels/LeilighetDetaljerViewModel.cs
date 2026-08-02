using DugnadAppMvc.Models;

namespace DugnadAppMvc.ViewModels
{
    public class LeilighetDetaljerViewModel
    {
        public int LeilighetId { get; set; }

        public string Leilighetsnummer { get; set; } = string.Empty;

        public int AntallRegistreringer { get; set; }

        public decimal TotaleTimer { get; set; }

        public decimal TotalVerdi { get; set; }

        public List<Timeforing> Dugnadstimer { get; set; } = [];

        public string BeboerNavn { get; set; } = string.Empty;
    }
}