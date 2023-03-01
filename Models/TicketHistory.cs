using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        //primary key 
        public int Id { get; set; }

        //foreign key 
        public int TicketId { get; set; }

        [Display(Name = "Property Name")]
        public string? PropertyName { get; set; }

        public string? Description { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        [Required]
        public string? UserId { get; set; }

        //navigational properties 

        //one history to one ticket 
        public virtual Ticket? Ticket { get; set; }

        //one history to one user
        public virtual BTUser? User { get; set; }


    }
}
