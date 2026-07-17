namespace DugnadAppMvc.ViewModels
{
    public class KommendeDugnadViewModel
    {
        public int Id { get; set; }

        public string Tittel { get; set; } = string.Empty;

        public DateOnly StartDato { get; set; }
    }
}