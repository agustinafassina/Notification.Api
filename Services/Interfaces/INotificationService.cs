using NotificationApi.Services.Dto;

namespace NotificationApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendNotificationWithAWS(EmailDto emailRequest);
        Task<bool> SendNotificationWithGmail(EmailDto emailRequest);
    }
}