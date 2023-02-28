using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketPriority
    {
        
        //primary key
        public int Id { get; set; }

        [Display(Name = "Priority Name")]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Name { get; set; }
    }
}
