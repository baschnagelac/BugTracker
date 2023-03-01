using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Xml.Linq;

namespace BugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

        [Display(Name = "Archived?")]
        public bool Archived { get; set; }

        [Display(Name = "Project Archived?")]
        public bool ArchivedByProject { get; set; }

        //foreign keys  
        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketPriorityId { get; set; }
        public string? DeveloperUserId { get; set; }

        [Required]
        public string? SubmitterUserId { get; set; }

        //navigational properties 

        //many tickets one project 
        //public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();

        public virtual Project? Project { get; set; }   

        //one ticket one priority

        public virtual TicketPriority? TicketPriority { get; set; }

        //one ticket to one type

        public virtual TicketType? TicketType { get; set; }

        //one ticket to one status 

        public virtual TicketStatus? TicketStatus { get; set; }

        //many tickets to one dev 

        public virtual BTUser? DeveloperUser { get; set; }

        //many tickets to one sumbitter
        public virtual BTUser? SubitterUser { get; set; }

        //one ticket to many comments 

        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();

        //one ticket to many attachments
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();

        //one ticket to one history

        public virtual TicketHistory? History { get; set; }


    }
}
