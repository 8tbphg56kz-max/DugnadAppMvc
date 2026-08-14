namespace DugnadAppMvc.ViewModels
{
    public class ArsAvslutningViewModel
    {
        public int Aar { get; set; }

        public int AntallDugnader { get; set; }

        public int AntallOppgaver { get; set; }

        public int AntallPameldinger { get; set; }

        public int AntallTimeforinger { get; set; }

        public int AntallDeltakere { get; set; }

        public decimal AntallTimer { get; set; }

        public bool UtbetalingForetatt { get; set; }

        public bool BekreftSletting { get; set; }
    }
}