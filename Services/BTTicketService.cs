using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
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

        //public async Task<bool> AddDevToTicketAsync(BTUser? dev, int? ticketId)
        //{
        //    try
        //    {
        //        Ticket? ticket = await GetTicketByIdAsync(ticketId, dev!.CompanyId);

        //        bool IsOnTicket = ticket.DeveloperUser.Any(m => m.Id == member.Id);

        //        if (!IsOnTicket)
        //        {
        //            ticket.DeveloperUser.Add(dev);
        //            await _context.SaveChangesAsync();
        //            return true;
        //        }

        //        return false;
        //    }

        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        public Task AddTicketAsync(Ticket? ticket)
        {
            throw new NotImplementedException();
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment? ticketAttachment)
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

        public async Task AddTicketCommentAsync(TicketComment? ticketComment)
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

        public Task DeleteTicketAsync(Ticket? ticket)
        {
            throw new NotImplementedException();
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int? ticketAttachmentId)
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

        public async Task<Ticket> GetTicketByIdAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                                 //.Where(p => p.CompanyId == companyId)
                                                 .Include(p => p.Title)
                                                 .Include(p => p.Description)                                           
                                                 .Include(p => p.TicketType)
                                                 .Include(p => p.TicketStatus)
                                                 .Include(p => p.TicketPriority)
                                                 .Include(p => p.Project)
                                                 .Include(p => p.Comments)
                                                 .Include(p => p.Attachments)
                                                 .Include(p => p.History)
                                                .FirstOrDefaultAsync(m => m.Id == ticketId);

                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<IEnumerable<Ticket>> GetTicketsAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTicketAsync(Ticket? ticket)
        {
            throw new NotImplementedException();
        }
    }
}
