namespace DugnadAppMvc.Models
{
    public class Endringslogg
    {
        public int Id { get; set; }

        public DateTime Tidspunkt { get; set; } = DateTime.UtcNow;

        public string BrukerId { get; set; } = null!;
        public ApplicationUser Bruker { get; set; } = null!;

        public string Handling { get; set; } = null!;

        public string Begrunnelse { get; set; } = null!;

        public int TimeforingId { get; set; }

        public int BeboerId { get; set; }

        public string? Aktivitet { get; set; }

        public decimal? GamleTimer { get; set; }
        public decimal? NyeTimer { get; set; }

        public string? GammelKommentar { get; set; }
        public string? NyKommentar { get; set; }
        public Beboer Beboer { get; set; } = null!;        
    }
}