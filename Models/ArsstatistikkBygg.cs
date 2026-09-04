namespace DugnadAppMvc.Models
{
    public class ArsstatistikkBygg
    {
        public int Id { get; set; }

        // Startåret for dugnadsåret.
        // Eksempel: 2025 = 2025/2026
        public int Aar { get; set; }

        public string ByggKode { get; set; } = "";

        public int AntallLeiligheter { get; set; }

        public decimal AndelLeiligheter { get; set; }

        public decimal Dugnadstimer { get; set; }

        public decimal AndelDugnadstimer { get; set; }

        public decimal Dugnadsindeks { get; set; }

        public string DugnadsAar =>
            $"{Aar}/{Aar + 1}";
    }
}