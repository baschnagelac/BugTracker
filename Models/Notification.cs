using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Notification
    {
        //primary key
        public int Id { get; set; }


        // foreign keys 
        public int ProjectId { get; set; }

        public int TicketId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [Display(Name = "Notification")]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Message { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        // foreign keys 
        [Required]
        public string? SenderId { get; set; }

        [Required]
        public string? RecipientId { get; set; }

        public int NotificationTypeId { get; set; }

        [Display(Name = "Seen?")]
        public bool HasBeenViewed { get; set; }

        // navigational properties

        //1 to 1 

        public virtual NotificationType? NotificationType { get; set; }
        
        public virtual Ticket? Ticket { get; set; } 
        
        public virtual Project? Project { get; set; }


        public virtual BTUser? Sender { get; set; }

        public virtual BTUser? Recipient { get; set; }


    }
}
