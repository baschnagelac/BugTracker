using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
		private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public BTTicketService(ApplicationDbContext context, UserManager<BTUser> userManager)
		{
			_context = context;
            _userManager = userManager;
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

        public async Task AddTicketCommentAsync(TicketComment ticketComment)
        {
			try
			{



                _context.Add(ticketComment);
                    
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{

				throw;
			}
		}

        public Task DeleteTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.BTUser!)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
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
