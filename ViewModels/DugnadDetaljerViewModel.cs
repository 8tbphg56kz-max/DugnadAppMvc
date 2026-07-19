using DugnadAppMvc.Models;

namespace DugnadAppMvc.ViewModels
{
    public class DugnadDetaljerViewModel
    {
        public int DugnadId { get; set; }

        public string Tittel { get; set; } = string.Empty;

        public DateOnly? Dato { get; set; }

        public int AntallRegistreringer { get; set; }

        public decimal TotaleTimer { get; set; }

        public decimal TotalVerdi { get; set; }

        public List<Dugnadstime> Dugnadstimer { get; set; } = new();
    }
}