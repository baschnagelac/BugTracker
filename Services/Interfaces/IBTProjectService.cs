using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
	public interface IBTProjectService
	{

	

		public Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId);		
		public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);
		public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);		
		public Task<BTUser> GetProjectManagerAsync(int? projectId);

		
		public Task RemoveProjectManagerAsync(int? projectId);
		public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);






  //      //get projectS
  //      public Task<IEnumerable<Project>> GetProjectsAsync(int companyId);

		////add project async 
		//public Task AddProjectAsync(Project project);

		////update project async 
		//public Task UpdateProjectAsync(Project project);

		////delete project async 
		//public Task DeleteProjectAsync(Project project);


		

	}
}
