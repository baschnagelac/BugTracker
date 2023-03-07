using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
	public interface IBTTicketService
	{
		//get ticket by id 
		public Task<Ticket> GetTicketById(int companyId, int id);

		//get TicketS
		public Task<IEnumerable<Ticket>> GetTicketsAsync(int companyId);
		
		//add ticket async 
		public Task AddTicketAsync(Ticket ticket);
		
		//update ticket async 

		public Task UpdateTicketAsync(Ticket ticket);

		//delete ticket async 
		public Task DeleteTicketAsync(Ticket ticket);

		//addticket attachment async 
		public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

		//add ticket comment 
		public Task AddTicketCommentAsync(TicketComment ticketComment);

	
		
	}
}
