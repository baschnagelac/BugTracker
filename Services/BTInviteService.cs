using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace BugTracker.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AcceptInviteAsync(Guid? token, string? userId)
        {
            try
            {
                if (token != null && !string.IsNullOrEmpty(userId))
                {
                    Invite? invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

                    if (invite == null)
                    {
                        return false;
                    }

                    try
                    {
                        invite.IsValid = false;

                        invite.InviteeId = userId;

                        await _context.SaveChangesAsync();

                        return true;
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddNewInviteAsync(Invite? invite)
        {
            try
            {
                try
                {
                    if (invite != null)
                    {
                        await _context.AddAsync(invite);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid? token, string? email, int? companyId)
        {
            try
            {
                if (token != null && !string.IsNullOrEmpty(email))
                {
                    bool result = await _context.Invites
                                                .Where(i => i.CompanyId == companyId)
                                                .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

                    return result;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite?> GetInviteAsync(int? inviteId, int? companyId)
        {
            try
            {
                if (inviteId != null && companyId != null)
                {
                    Invite? invite = await _context.Invites
                                     .Where(i => i.CompanyId == companyId)
                                     .Include(i => i.Company)
                                     .Include(i => i.Project)
                                     .Include(i => i.Invitor)
                                     .FirstOrDefaultAsync(i => i.Id == inviteId);

                    return invite; 
                }
                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Invite?> GetInviteAsync(Guid? token, string? email, int? companyId)
        {
            try
            {
                if (token != null && !string.IsNullOrEmpty(email))
                {
                    Invite? invite = await _context.Invites
                                                   .Where(i => i.CompanyId == companyId)
                                                   .Include(i => i.Company)
                                                   .Include(i => i.Project)
                                                   .Include(i => i.Invitor)
                                                   .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

                    return invite;
                }

                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            try
            {
                if (token == null)
                {
                    return false;
                }

                bool result = false;

                Invite? invite = await _context.Invites
                                               .FirstOrDefaultAsync(i => i.CompanyToken == token);

                if (invite != null)
                {
                    // Determine invite date 
                    DateTime inviteDate = invite.InviteDate;

                    // Custom validation of invite based on the date it was issued 
                    //In this case we are allowing an invite to be valid for 7 days 

                    bool validDate = (DateTime.Now - inviteDate).TotalDays <= 7;

                    if (validDate)
                    {
                        result = invite.IsValid;
                    }
                }
                return result;


            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
