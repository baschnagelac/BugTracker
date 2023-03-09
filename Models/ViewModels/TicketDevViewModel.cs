using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels
{
    public class TicketDevViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? DevList { get; set; }
        public string? SelectedDev { get; set; }

    }
}
