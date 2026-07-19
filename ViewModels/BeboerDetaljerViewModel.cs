using DugnadAppMvc.Models;

namespace DugnadAppMvc.ViewModels
{
    public class BeboerDetaljerViewModel
    {
        public int BeboerId { get; set; }

        public string Navn { get; set; } = string.Empty;

        public string Leilighetsnummer { get; set; } = string.Empty;

        public int AntallRegistreringer { get; set; }

        public decimal TotaleTimer { get; set; }

        public decimal TotalVerdi { get; set; }

        public List<Dugnadstime> Dugnadstimer { get; set; } = new();
    }
}