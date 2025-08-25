
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationApi.Services.Dto;
using NotificationApi.Services.Interfaces;
using NotificationApi.Services.Settings;

namespace NotificationApi.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly SMTPSetting _smtpSetting;
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly AWSSetting _awsSetting;
        public NotificationService(IOptions<SMTPSetting> smtpSetting, IAmazonSimpleEmailService sesClient, IOptions<AWSSetting> awsSetting)
        {
            _smtpSetting = smtpSetting.Value ?? throw new ArgumentNullException(nameof(smtpSetting), "SMPT settings cannot be null.");
            _sesClient = sesClient;
            _awsSetting = awsSetting.Value ?? throw new ArgumentNullException(nameof(awsSetting), "AWS settings cannot be null.");
        }

        public async Task<string> SendNotificationWithSeSAws(EmailDto request)
        {
            try
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
                            Text = new Content(request.Body)
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);
                return response.HttpStatusCode.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> SendNotificationWithSMTP(EmailDto request)
        {
            try
                {
                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress("Agushinaa", _smtpSetting.Email));
                    emailMessage.To.Add(new MailboxAddress(request.RecipientName, request.RecipientEmail));
                    emailMessage.Subject = "Welcome " + request.RecipientName;

                    emailMessage.Body = new TextPart("html")
                    {
                        Text = request.Body
                    };

                    using var client = new SmtpClient();
                    await client.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, SecureSocketOptions.Auto);
                    await client.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                    return "Email sent successfully";
                }
                catch (Exception ex)
                {
                    return "Error sending email: " + ex.Message;
                }
        }
    }
}
