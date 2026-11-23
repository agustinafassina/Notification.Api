using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Moq;
using NotificationApi.Services.Dto;
using NotificationApi.Services.Exceptions;
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
            new NotificationService(Options.Create((SMTPSetting)null!), new Lazy<IAmazonSimpleEmailService>(_sesClientMock.Object), awsSettings));
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
            new NotificationService(smtpSettings, new Lazy<IAmazonSimpleEmailService>(_sesClientMock.Object), Options.Create((AWSSetting)null!)));
    }

    [Fact]
    public async Task SendNotificationWithSeSAws_SendsExpectedRequest_WhenSesSucceeds()
    {
        SendEmailRequest? capturedRequest = null;

        _sesClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .Callback<SendEmailRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new SendEmailResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });

        NotificationService? service = CreateService();
        EmailDto email = CreateEmailDto();

        await service.SendNotificationWithSeSAws(email);

        Assert.NotNull(capturedRequest);
        Assert.Equal("from@test.com", capturedRequest!.Source);
        Assert.Equal(email.RecipientEmail, capturedRequest.Destination.ToAddresses.Single());
        Assert.Equal(email.Subject, capturedRequest.Message.Subject.Data);
        Assert.Equal(email.Body, capturedRequest.Message.Body.Html.Data);
    }

    [Fact]
    public async Task SendNotificationWithSeSAws_ThrowsEmailDeliveryException_WhenSesFails()
    {
        _sesClientMock
            .Setup(client => client.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ThrowsAsync(new InvalidOperationException("SES unavailable"));

        NotificationService? service = CreateService();

        EmailDeliveryException ex = await Assert.ThrowsAsync<EmailDeliveryException>(
            () => service.SendNotificationWithSeSAws(CreateEmailDto()));

        Assert.Equal("SES", ex.Provider);
        Assert.Equal("SES unavailable", ex.InnerException!.Message);
    }

    [Fact]
    public async Task SendNotificationWithSMTP_ThrowsEmailDeliveryException_WhenConnectionFails()
    {
        NotificationService? service = CreateService(smtpSettings: new SMTPSetting
        {
            Host = "invalid-host.test",
            Port = 587,
            Email = "user@test.com",
            Password = "password",
            Name = "Test"
        });

        EmailDeliveryException ex = await Assert.ThrowsAsync<EmailDeliveryException>(
            () => service.SendNotificationWithSMTP(CreateEmailDto()));

        Assert.Equal("SMTP", ex.Provider);
    }

    [Fact]
    public async Task SendNotificationWithSeSAws_ThrowsEmailDeliveryException_WhenSesClientCannotBeCreated()
    {
        var failingClient = new Lazy<IAmazonSimpleEmailService>(
            () => throw new InvalidOperationException("Failed to resolve AWS credentials"));
        var service = new NotificationService(
            Options.Create(new SMTPSetting()),
            failingClient,
            Options.Create(new AWSSetting { EmailFrom = "from@test.com" }));

        EmailDeliveryException ex = await Assert.ThrowsAsync<EmailDeliveryException>(
            () => service.SendNotificationWithSeSAws(CreateEmailDto()));

        Assert.Equal("SES", ex.Provider);
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
            new Lazy<IAmazonSimpleEmailService>(_sesClientMock.Object),
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
