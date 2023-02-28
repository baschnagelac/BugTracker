using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketComment
    {
        //primary key
        public int Id { get; set; }

        [Required]
        [Display(Name = "Comment")]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Comment { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        //foreign keys 

        public int TicketId { get; set; }

        public string? UserId { get; set; }

        //navigational properties T

        //one ticket to one comment (can have multiple comments tho?)
        public virtual Ticket? Ticket { get; set; }

        //one user to one comment 
        public virtual BTUser? User { get; set; }

    }
}
