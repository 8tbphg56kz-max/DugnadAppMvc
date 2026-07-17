namespace DugnadAppMvc.ViewModels
{
    public class DashboardViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;

        public decimal TotalHours { get; set; }

        public int ActiveTasks { get; set; }

        public bool HasCommonDugnad { get; set; }

        public string? BoardMessage { get; set; }
    }
}