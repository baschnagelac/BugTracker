using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }
        public Task AddProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public async Task<Project> GetProjectById(int companyId, int id)
        {
            Project? project = await _context.Projects
                                   .Where(p => p.CompanyId == companyId)
                                   .Include(p => p.Company)
                                   .Include(p => p.ProjectPriority)
                                   .Include(p => p.Members)
                                   .Include(p => p.Tickets)
                                       .ThenInclude(t => t.DeveloperUser)
                                   .Include(p => p.Tickets)
                                       .ThenInclude(t => t.SubmitterUser)
                                   .FirstOrDefaultAsync(m => m.Id == id);

            return project;
        }

        public Task<IEnumerable<Project>> GetProjectsAsync(int companyId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }
    }
}
