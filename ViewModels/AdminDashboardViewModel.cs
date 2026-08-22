namespace DugnadAppMvc.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int AntallBeboere { get; set; }

        public int AntallAktiveDugnader { get; set; }

        public int AntallUtforteDugnader { get; set; }

        public decimal RegistrerteTimer { get; set; }

        public int Dugnadsbudsjett { get; set; }

        public decimal ForelopigTimepris { get; set; }

        public List<KommendeDugnadViewModel> KommendeDugnader { get; set; } = new();

        //public List<SisteRegistreringViewModel> SisteRegistreringer { get; set; } = new();
    }
}