using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Moq;
using NotificationApi.Services.Dto;
using NotificationApi.Services.Implementations;
using NotificationApi.Services.Settings;

namespace NotificationApi.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IAmazonSimpleEmailService> _sesClientMock = new();

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenSmtpSettingsAreNull()
    {
        IOptions<AWSSetting>? awsSettings = Options.Create(new AWSSetting { Region = "us-west-1", EmailFrom = "from@test.com" });

        Assert.Throws<ArgumentNullException>(() =>
            new NotificationService(Options.Create((SMTPSetting)null!), _sesClientMock.Object, awsSettings));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenAwsSettingsAreNull()
    {
        IOptions<SMTPSetting>? smtpSettings = Options.Create(new SMTPSetting
        {
            Host = "smtp.test.com",
            Port = 587,
            Email = "user@test.com",
            Password = "password",
            Name = "Test"
        });

        Assert.Throws<ArgumentNullException>(() =>
            new NotificationService(smtpSettings, _sesClientMock.Object, Options.Create((AWSSetting)null!)));
    }

    [Fact]
    public async Task SendNotificationWithSeSAws_ReturnsHttpStatusCode_WhenSesSucceeds()
    {
        SendEmailRequest? capturedRequest = null;

        _sesClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .Callback<SendEmailRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new SendEmailResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        NotificationService? service = CreateService();
        EmailDto email = CreateEmailDto();

        string? result = await service.SendNotificationWithSeSAws(email);

        Assert.Equal("OK", result);
        Assert.NotNull(capturedRequest);
        Assert.Equal("from@test.com", capturedRequest!.Source);
        Assert.Equal(email.RecipientEmail, capturedRequest.Destination.ToAddresses.Single());
        Assert.Equal(email.Subject, capturedRequest.Message.Subject.Data);
        Assert.Equal(email.Body, capturedRequest.Message.Body.Text.Data);
    }

    [Fact]
    public async Task SendNotificationWithSeSAws_ReturnsErrorMessage_WhenSesFails()
    {
        _sesClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ThrowsAsync(new InvalidOperationException("SES unavailable"));

        NotificationService? service = CreateService();

        string? result = await service.SendNotificationWithSeSAws(CreateEmailDto());

        Assert.Equal("SES unavailable", result);
    }

    [Fact]
    public async Task SendNotificationWithSMTP_ReturnsErrorMessage_WhenConnectionFails()
    {
        NotificationService? service = CreateService(smtpSettings: new SMTPSetting
        {
            Host = "invalid-host.test",
            Port = 587,
            Email = "user@test.com",
            Password = "password",
            Name = "Test"
        });

        string? result = await service.SendNotificationWithSMTP(CreateEmailDto());

        Assert.StartsWith("Error sending email:", result);
    }

    private NotificationService CreateService(
        SMTPSetting? smtpSettings = null,
        AWSSetting? awsSettings = null)
    {
        smtpSettings ??= new SMTPSetting
        {
            Host = "smtp.test.com",
            Port = 587,
            Email = "user@test.com",
            Password = "password",
            Name = "Test"
        };

        awsSettings ??= new AWSSetting
        {
            Region = "us-west-1",
            EmailFrom = "from@test.com"
        };

        return new NotificationService(
            Options.Create(smtpSettings),
            _sesClientMock.Object,
            Options.Create(awsSettings));
    }

    private static EmailDto CreateEmailDto() => new()
    {
        RecipientName = "John Doe",
        RecipientEmail = "john@example.com",
        Subject = "Test subject",
        Body = "Test body"
    };
}
