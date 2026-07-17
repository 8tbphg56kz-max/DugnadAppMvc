namespace DugnadAppMvc.ViewModels
{
    public class AdminDugnadstimeViewModel
    {
        public int Id { get; set; }

        public DateTime Registrert { get; set; }

        public string Dugnad { get; set; } = "";

        public string Beboer { get; set; } = "";

        public decimal Timer { get; set; }

        public string? Kommentar { get; set; }
    }
}