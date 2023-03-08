using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<Company> GetCompanyInfoAsync(int? companyId);

        public Task<List<BTUser>> GetMemberAsync(int? companyId);
    }
}
