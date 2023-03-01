using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Invite
    {
        // primary key 
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        //foreign keys 

        public int CompanyId { get; set; }
        public int ProjectId { get; set; }

        [Required]
        public int InvitorId { get; set; }
        public int InviteeId { get; set; }

        [Required]
        public string? InviteeEmail { get; set; }

        [Required]
        public string? InviteeFirstName { get; set; }

        [Required]
        public string? InviteeLastName { get; set; }

        [Required]
        [Display(Name = "Invite Message")]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Message { get; set; }

        [Display(Name = "Valid?")]
        public bool IsValid { get; set; }

        //navigational properties 

        //1 to 1

        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set; }

        public virtual string? Invitor { get; set; }

        public virtual string? Invitee { get; set; }


    }
}
