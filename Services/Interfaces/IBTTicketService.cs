using BugTracker.Models;
using System.ComponentModel.Design;

namespace BugTracker.Services.Interfaces
{
	public interface IBTTicketService
	{
		//get ticket by id 
		public Task<Ticket> GetTicketByIdAsync(int? ticketId, int? projectId);

		//get TicketS
		public Task<IEnumerable<Ticket>> GetTicketsAsync(int? companyId);
		
		//add ticket async 
		public Task AddTicketAsync(Ticket? ticket);
		
		//update ticket async 

		public Task UpdateTicketAsync(Ticket? ticket);

		//delete ticket async 
		public Task DeleteTicketAsync(Ticket? ticket);

		//addticket attachment async 
		public Task AddTicketAttachmentAsync(TicketAttachment? ticketAttachment);

		//add ticket comment 
		public Task AddTicketCommentAsync(TicketComment? ticketComment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int? ticketAttachmentId);

		public Task AddDevToTicketAsync(BTUser? dev, string? selectedUserId, int? ticketId);


		public Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId);

    }
}
