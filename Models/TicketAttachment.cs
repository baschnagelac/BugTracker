using BugTracker.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        //primary key
        public int Id { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        //foreign keys 

        public int TicketId { get; set; }

        [Required]
        public string? BTUserId { get; set; }

		[NotMapped]
		[DisplayName("Select a file")]
		[DataType(DataType.Upload)]
		[MaxFileSize(1024 * 1024)]
		[AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
		public virtual IFormFile? FormFile { get; set; }
        public byte[]? FileData { get; set; }
        public string? FileType { get; set; }

        //navigational properties 

        //one ticket to many attachments 
        public virtual Ticket? Ticket { get; set; }

        //one ticket to many users? one user? going with 1
        public virtual BTUser? BTUser { get; set; }
    }
}
