namespace DugnadAppMvc.Models
{
    public class Leilighet
    {
        public int Id { get; set; }

        public int Seksjonsnummer { get; set; }

        public string Leilighetsnummer { get; set; } = "";

        public ICollection<Beboer> Beboere { get; set; } = new List<Beboer>();
        public string Visningsnavn =>
    $"Seksjon {Seksjonsnummer} - {Leilighetsnummer}";
    }
}