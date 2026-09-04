namespace DugnadAppMvc.Models
{
    public class Arsstatistikk
    {
        public int Id { get; set; }

        // Startåret for dugnadsåret.
        // Eksempel: 2025 betyr dugnadsåret 2025/2026.
        public int Aar { get; set; }

        public int AntallAktiviteter { get; set; }

        public int AntallPameldinger { get; set; }

        public int AntallDeltakere { get; set; }

        public decimal AntallTimer { get; set; }

        public string DugnadsAar =>
            $"{Aar}/{Aar + 1}";
    }
}