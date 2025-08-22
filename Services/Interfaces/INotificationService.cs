using NotificationApi.Services.Dto;

namespace NotificationApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendNotificationWithSeSAws(EmailDto emailRequest);
        Task<bool> SendNotificationWithSMTP(EmailDto emailRequest);
    }
}