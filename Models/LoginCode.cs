namespace DugnadAppMvc.Models
{
    public class LoginCode
    {
        public int Id { get; set; }

        public string Epost { get; set; } = "";

        public string Kode { get; set; } = "";

        public DateTime Opprettet { get; set; } = DateTime.UtcNow;

        public DateTime Utloper { get; set; }

        public bool Brukt { get; set; } = false;
    }
}