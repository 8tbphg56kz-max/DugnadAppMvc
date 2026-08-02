namespace DugnadAppMvc.ViewModels
{
    public class TimeforingHistorikkSideViewModel
    {
        public int AntallRegistreringer { get; set; }

        public decimal TotaltAntallTimer { get; set; }

        public List<TimeforingHistorikkViewModel> Historikk { get; set; } = [];
    }
}