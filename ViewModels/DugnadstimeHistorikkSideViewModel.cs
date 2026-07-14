namespace DugnadAppMvc.ViewModels
{
    public class DugnadstimeHistorikkSideViewModel
    {
        public int AntallRegistreringer { get; set; }

        public decimal TotaltAntallTimer { get; set; }

        public List<DugnadstimeHistorikkViewModel> Historikk { get; set; } = [];
    }
}