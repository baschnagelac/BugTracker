using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
	public interface IBTProjectService
	{

	

		public Task AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId);
		public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId);

		public Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId);
		public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);
		public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);		
		public Task<BTUser> GetProjectManagerAsync(int? projectId);

		
		public Task RemoveProjectManagerAsync(int? projectId);
		public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);



        public Task<IEnumerable<Project>> GetAllProjectsByCompanyAsync(int companyId);
        public Task<IEnumerable<Project>> GetAllProjectsByPriorityAsync(int companyId, string projectPriority);
        public Task<IEnumerable<BTUser>> GetAllProjectMembersByRoleAsync(int projectId, string roleName);




        //      //get projectS
        public Task<IEnumerable<Project>> GetAllProjectsAsync(int companyId);

		////add project async 
		//public Task AddProjectAsync(Project project);

		////update project async 
		//public Task UpdateProjectAsync(Project project);

		////delete project async 
		//public Task DeleteProjectAsync(Project project);




	}
}
