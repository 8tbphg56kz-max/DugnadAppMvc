namespace DugnadAppMvc.Models
{
    public class Arsstatistikk
    {
        public int Id { get; set; }

        public int Aar { get; set; }

        public int AntallAktiviteter { get; set; }

        public int AntallPameldinger { get; set; }

        public int AntallDeltakere { get; set; }

        public decimal AntallTimer { get; set; }
    }
}