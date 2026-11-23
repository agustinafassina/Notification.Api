using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationApi.Contracts.Request;
using NotificationApi.Controllers;
using NotificationApi.Services.Dto;
using NotificationApi.Services.Exceptions;
using NotificationApi.Services.Interfaces;

namespace NotificationApi.Tests.Controllers;

public class NotificationControllerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly NotificationController _controller;

    public NotificationControllerTests()
    {
        _controller = new NotificationController(_notificationServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public void Controller_RequiresAuthorization()
    {
        Assert.NotNull(typeof(NotificationController).GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void GetVersion_AllowsAnonymous()
    {
        MethodInfo method = typeof(NotificationController).GetMethod(nameof(NotificationController.GetVersion))!;

        Assert.NotNull(method.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void GetVersion_ReturnsOkWithVersion()
    {
        IActionResult result = _controller.GetVersion();

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("v1.0.0", okResult.Value);
    }

    [Fact]
    public async Task SendNotificationWithAWS_ReturnsOk_WhenServiceSucceeds()
    {
        EmailRequest request = CreateEmailRequest();
        EmailDto emailDto = CreateEmailDto();
        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);

        IActionResult result = await _controller.SendNotificationWithAWS(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Email sent successfully", GetMessage(okResult));
        _notificationServiceMock.Verify(s => s.SendNotificationWithSeSAws(emailDto), Times.Once);
    }

    [Fact]
    public async Task SendNotificationWithAWS_PropagatesDeliveryException()
    {
        EmailRequest request = CreateEmailRequest();
        EmailDto emailDto = CreateEmailDto();
        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);
        _notificationServiceMock
            .Setup(s => s.SendNotificationWithSeSAws(emailDto))
            .ThrowsAsync(new EmailDeliveryException("SES", new Exception("SES unavailable")));

        await Assert.ThrowsAsync<EmailDeliveryException>(() => _controller.SendNotificationWithAWS(request));
    }

    [Fact]
    public async Task SendNotificaionWithPersonalUser_ReturnsOk_WhenServiceSucceeds()
    {
        EmailRequest request = CreateEmailRequest();
        EmailDto emailDto = CreateEmailDto();
        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);

        IActionResult result = await _controller.SendNotificaionWithPersonalUser(request);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Email sent successfully", GetMessage(okResult));
        _notificationServiceMock.Verify(s => s.SendNotificationWithSMTP(emailDto), Times.Once);
    }

    [Fact]
    public async Task SendNotificaionWithPersonalUser_PropagatesDeliveryException()
    {
        EmailRequest request = CreateEmailRequest();
        EmailDto emailDto = CreateEmailDto();
        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);
        _notificationServiceMock
            .Setup(s => s.SendNotificationWithSMTP(emailDto))
            .ThrowsAsync(new EmailDeliveryException("SMTP", new Exception("SMTP failure")));

        await Assert.ThrowsAsync<EmailDeliveryException>(() => _controller.SendNotificaionWithPersonalUser(request));
    }

    private static object? GetMessage(OkObjectResult result) =>
        result.Value!.GetType().GetProperty("message")!.GetValue(result.Value);

    private static EmailRequest CreateEmailRequest() => new()
    {
        RecipientName = "John Doe",
        RecipientEmail = "john@example.com",
        Subject = "Test subject",
        Body = "<p>Test body</p>"
    };

    private static EmailDto CreateEmailDto() => new()
    {
        RecipientName = "John Doe",
        RecipientEmail = "john@example.com",
        Subject = "Test subject",
        Body = "<p>Test body</p>"
    };
}
