using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTNotificationService
    {
        public Task AddNotificationAsync(Notification? notification, int? projectId);

        public Task AdminNotificationAsync(Notification? notification, int? companyId);

        public Task<List<Notification>> GetNotificationsByUserIdAsync(string? userId);

        public Task<bool> SendAdminEmailNotificationAsync(Notification? notification, string? emailSubject, int? companyId);

        public Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject);
    }
}
