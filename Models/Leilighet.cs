namespace DugnadAppMvc.Models
{
    public class Leilighet
    {
        public int Id { get; set; }

        public string Leilighetsnummer { get; set; } = "";

        public ICollection<Beboer> Beboere { get; set; } = new List<Beboer>();

        public string Visningsnavn => Leilighetsnummer;
    }
}