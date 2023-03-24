using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;

        public BTProjectService(ApplicationDbContext context, IBTRolesService roleService, UserManager<BTUser> userManager)
        {
            _context = context;
            _rolesService = roleService;
            _userManager = userManager;

        }

        public async Task AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                foreach (string userId in userIds)
                {
                    BTUser? btUser = await _context.Users.FindAsync(userId);

                    if (project != null && btUser != null)
                    {
                        bool IsOnProject = project.Members.Any(m => m.Id == userId);

                        if (!IsOnProject)
                        {
                            project.Members.Add(btUser);
                        }
                        else
                        {
                            continue;
                        }

                    }

                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (!IsOnProject)
                {
                    project.Members.Add(member);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }

            catch (Exception)
            {

                throw;
            }
        }



        public Task AddProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                // Remove current PM 
                if (currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }

                // Add new/selected PM
                try
                {
                    await AddMemberToProjectAsync(selectedPM!, projectId);
                    return true;
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

        public Task DeleteProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public async Task<Project> GetProjectByIdAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Where(p => p.CompanyId == companyId)
                                                 .Include(p => p.Company)
                                                 .Include(p => p.Members)
                                                 .Include(p => p.ProjectPriority)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.SubmitterUser)
                                                .FirstOrDefaultAsync(m => m.Id == projectId);

                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .Include(p=>p.Tickets)
                                        
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync(int companyId)
        {



            try
            {
                List<Project> projects = new();
                projects = await _context.Projects
                                       .Where(c => c.CompanyId == companyId)
                                       .Include(c => c.Tickets)
                                       .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (IsOnProject)
                {
                    project.Members.Remove(member);
                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                foreach (BTUser member in project.Members)
                {
                    if (!await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        project.Members.Remove(member);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveMemberFromProjectAsync(member, projectId);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task UpdateProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsByCompanyAsync(int companyId)
        {
            try
            {
                List<Project> projects = new();
                projects = await _context.Projects
                                       .Where(c => c.CompanyId == companyId)
                                       .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetAllProjectsByPriorityAsync(int companyId, string projectPriority)
        {
            try
            {
                List<Project> projects = new();
                List<ProjectPriority> priorities = new();

                projects = await _context.Projects
                                       .Where(c => c.CompanyId == companyId)
                                       .Where(p => p.ProjectPriority!.Name == projectPriority)
                                       .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<BTUser>> GetAllProjectMembersByRoleAsync(int projectId, string roleName)
        {
            try
            {
                List<BTUser> result = new();
                List<BTUser> users = new();

                Project? project = await _context.Projects
                                                 .Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);
                foreach (BTUser member in project!.Members)
                {
                   if(await _rolesService.IsUserInRoleAsync(member, roleName))
                    {
                        result.Add(member);
                    }
                }


                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projects = await _context.Projects
                                              .Where(p => p.Archived == false && p.CompanyId == companyId)
                                              .Include(p => p.Members)
                                              .Include(p => p.Members)
                                              .Include(p => p.ProjectPriority)
                                              .Include(p => p.Tickets)
                                              .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
