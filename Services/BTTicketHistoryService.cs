using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;

namespace BugTracker.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {

        public readonly ApplicationDbContext _context;

        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket? oldTicket, Ticket? newTicket, string? userId)
        {
            try
            {
                if (oldTicket == null && newTicket != null)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = string.Empty,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        UserId = userId,
                        Description = "New Ticket Created"
                    };

                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();

                }
                else if (oldTicket != null && newTicket != null)
                {
                    // check ticket title 
                    if (oldTicket.Title != newTicket.Title)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Title",
                            OldValue = oldTicket.Title,
                            NewValue = newTicket.Title,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Ticket Title Updated  from: {oldTicket.Title} to : {newTicket.Title}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }
                    // check ticket Description
                    if (oldTicket.Description != newTicket.Description)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Description",
                            OldValue = oldTicket.Description,
                            NewValue = newTicket.Description,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Ticket Description Updated  from: {oldTicket.Description} to : {newTicket.Description}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }
                    // check ticket Priority 
                    if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Priority",
                            OldValue = oldTicket.TicketPriority?.Name,
                            NewValue = newTicket.TicketPriority?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Ticket Priority Updated  from: {oldTicket.TicketPriority?.Name} to : {newTicket.TicketPriority?.Name}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }

                    // check ticket Type
                    if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Type",
                            OldValue = oldTicket.TicketType?.Name,
                            NewValue = newTicket.TicketType?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Ticket Type Updated  from: {oldTicket.TicketType?.Name} to : {newTicket.TicketType?.Name}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }
                    // check ticket Status 
                    if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Status",
                            OldValue = oldTicket.TicketStatus?.Name,
                            NewValue = newTicket.TicketStatus?.Name,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Ticket Status Updated  from: {oldTicket.TicketStatus?.Name} to : {newTicket.TicketStatus?.Name}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }
                    // check ticket developer
                    if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                    {
                        TicketHistory history = new()
                        {
                            TicketId = newTicket.Id,
                            PropertyName = "Developer",
                            OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                            NewValue = newTicket.DeveloperUser?.FullName,
                            Created = DataUtility.GetPostGresDate(DateTime.Now),
                            UserId = userId,
                            Description = $"Ticket Developer Updated  from: {oldTicket.DeveloperUser?.FullName} to : {newTicket.DeveloperUser?.FullName}"
                        };

                        await _context.TicketHistories.AddAsync(history);

                    }

                    try
                    {
                        //save the ticket history dataset to the database
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddHistoryAsync(int? ticketId, string? model, string? userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FindAsync(ticketId);

                string description = model!.ToLower().Replace("ticket", "");

                description = $"New {description} added to ticket: {ticket!.Title}";

                if (ticket != null)
                {

                    TicketHistory history = new()
                    {
                        TicketId = ticket.Id,
                        PropertyName = model,
                        OldValue = string.Empty,
                        NewValue = string.Empty,
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        UserId = userId,
                        Description = description
                    };

                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int? companyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int? projectId, int? companyId)
        {
            throw new NotImplementedException();
        }
    }
}
