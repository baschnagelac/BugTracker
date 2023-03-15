using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Project
    {
        //primary key
        public int Id { get; set; }

        //foreign key 
        public int CompanyId { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        [StringLength(9000, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Project Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        public int ProjectPriorityId { get; set; }

        [NotMapped]
        public virtual IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        [Display(Name = "Archived?")]
        public bool Archived { get; set; }

        //navigational properties 

        //one project to one company

        public virtual Company? Company { get; set; }

        //one project to one priority 

        public virtual ProjectPriority? ProjectPriority { get; set; }

        //one project to many members

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        //one project to many tickets 
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();


    }
}
