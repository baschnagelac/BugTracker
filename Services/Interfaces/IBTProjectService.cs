using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
	public interface IBTProjectService
	{

		//get project by id 
		public Task<Project> GetProjectById(int companyId, int Id);

		//get projectS
		public Task<IEnumerable<Project>> GetProjectsAsync(int companyId);

		//add project async 
		public Task AddProjectAsync(Project project);

		//update project async 
		public Task UpdateProjectAsync(Project project);

		//delete project async 
		public Task DeleteProjectAsync(Project project);


		//add members to a project?? ---- Not sure if this is necessary 

	}
}
