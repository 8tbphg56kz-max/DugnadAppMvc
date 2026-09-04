using DugnadAppMvc.Models;

namespace DugnadAppMvc.ViewModels
{
    public class ArsstatistikkViewModel
    {
        public List<Arsstatistikk> Arsstatistikker { get; set; } = new();

        public List<ArsstatistikkBygg> ByggStatistikk { get; set; } = new();
    }
}