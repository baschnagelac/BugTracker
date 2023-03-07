using BugTracker.Models;
using BugTracker.Services.Interfaces;

namespace BugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        public Task AddProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public Task<Project> GetProjectById(int companyId, int Id)
        {
            throw new NotImplementedException();
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
