using NotificationApi.Services.Dto;

namespace NotificationApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationWithSeSAws(EmailDto emailRequest);
        Task SendNotificationWithSMTP(EmailDto emailRequest);
    }
}