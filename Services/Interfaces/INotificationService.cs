using NotificationApi.Services.Dto;

namespace NotificationApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task<string> SendNotificationWithSeSAws(EmailDto emailRequest);
        Task<string> SendNotificationWithSMTP(EmailDto emailRequest);
    }
}