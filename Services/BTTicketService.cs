using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
		private readonly ApplicationDbContext _context;

		public BTTicketService(ApplicationDbContext context)
		{
			_context = context;
		}
		public Task AddTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
			try
			{
				await _context.AddAsync(ticketAttachment);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

        public Task AddTicketCommentAsync(TicketComment ticketComment)
        {
			//try
			//{
			//	_context.Add(comment);
			//	await _context.SaveChangesAsync();
			//}
			//catch (Exception)
			//{

			//	throw;
			//}
			throw new NotImplementedException();
		}

        public Task DeleteTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Task<Ticket> GetTicketById(int companyId, int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Ticket>> GetTicketsAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }
    }
}
