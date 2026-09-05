using DugnadAppMvc.Models;

namespace DugnadAppMvc.ViewModels
{
    public class ArsstatistikkViewModel
    {
        public List<Arsstatistikk> Arsstatistikker { get; set; } = new();

        public List<ArsstatistikkBygg> ByggStatistikk { get; set; } = new();

        // Løpende statistikk for inneværende dugnadsår
        public int Aar { get; set; }

        public int AntallAktiviteter { get; set; }

        public int AntallPameldinger { get; set; }

        public int AntallDeltakere { get; set; }

        public decimal AntallTimer { get; set; }

        public List<ArsstatistikkBygg> PaagaaendeByggStatistikk { get; set; } = new();
    }
}