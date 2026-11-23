using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationApi.Services.Dto;
using NotificationApi.Services.Exceptions;
using NotificationApi.Services.Interfaces;
using NotificationApi.Services.Settings;

namespace NotificationApi.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly SMTPSetting _smtpSetting;
        // Lazy so that SMTP and /version keep working when AWS credentials are not configured.
        private readonly Lazy<IAmazonSimpleEmailService> _sesClient;
        private readonly AWSSetting _awsSetting;

        public NotificationService(IOptions<SMTPSetting> smtpSetting, Lazy<IAmazonSimpleEmailService> sesClient, IOptions<AWSSetting> awsSetting)
        {
            _smtpSetting = smtpSetting.Value ?? throw new ArgumentNullException(nameof(smtpSetting), "SMTP settings cannot be null.");
            _sesClient = sesClient;
            _awsSetting = awsSetting.Value ?? throw new ArgumentNullException(nameof(awsSetting), "AWS settings cannot be null.");
        }

        public async Task SendNotificationWithSeSAws(EmailDto request)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = _awsSetting.EmailFrom,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { request.RecipientEmail }
                },
                Message = new Message
                {
                    Subject = new Content(request.Subject),
                    Body = new Body
                    {
                        Html = new Content(request.Body)
                    }
                }
            };

            try
            {
                await _sesClient.Value.SendEmailAsync(sendRequest);
            }
            catch (Exception ex)
            {
                throw new EmailDeliveryException("SES", ex);
            }
        }

        public async Task SendNotificationWithSMTP(EmailDto request)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_smtpSetting.Name, _smtpSetting.Email));
            emailMessage.To.Add(new MailboxAddress(request.RecipientName, request.RecipientEmail));
            emailMessage.Subject = request.Subject;
            emailMessage.Body = new TextPart("html")
            {
                Text = request.Body
            };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new EmailDeliveryException("SMTP", ex);
            }
        }
    }
}
