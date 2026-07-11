using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationApi.Contracts.Request;
using NotificationApi.Controllers;
using NotificationApi.Services.Dto;
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
    public void GetVersion_ReturnsOkWithVersion()
    {
        IActionResult? result = _controller.GetVersion();

        OkObjectResult? okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("v1.0.0", okResult.Value);
    }

    [Fact]
    public async Task SendNotificationWithAWS_ReturnsOk_WhenServiceSucceeds()
    {
        EmailRequest request = CreateEmailRequest();
        EmailDto? emailDto = CreateEmailDto();

        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);
        _notificationServiceMock
            .Setup(s => s.SendNotificationWithSeSAws(emailDto))
            .ReturnsAsync("OK");

        IActionResult? result = await _controller.SendNotificationWithAWS(request);

        OkObjectResult? okResult = Assert.IsType<OkObjectResult>(result);
        object? message = okResult.Value!.GetType().GetProperty("message")!.GetValue(okResult.Value);
        Assert.Equal("OK", message);
    }

    [Fact]
    public async Task SendNotificationWithAWS_ReturnsBadRequest_WhenArgumentExceptionIsThrown()
    {
        EmailRequest? request = CreateEmailRequest();
        EmailDto emailDto = CreateEmailDto();

        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);
        _notificationServiceMock
            .Setup(s => s.SendNotificationWithSeSAws(emailDto))
            .ThrowsAsync(new ArgumentException("Invalid email"));

        IActionResult? result = await _controller.SendNotificationWithAWS(request);

        BadRequestObjectResult? badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        object? error = badRequestResult.Value!.GetType().GetProperty("error")!.GetValue(badRequestResult.Value);
        Assert.Equal("Invalid email", error);
    }

    [Fact]
    public async Task SendNotificationWithAWS_ReturnsInternalServerError_WhenUnexpectedExceptionIsThrown()
    {
        EmailRequest? request = CreateEmailRequest();
        EmailDto? emailDto = CreateEmailDto();

        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);
        _notificationServiceMock
            .Setup(s => s.SendNotificationWithSeSAws(emailDto))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        IActionResult? result = await _controller.SendNotificationWithAWS(request);

        ObjectResult? objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        object? error = objectResult.Value!.GetType().GetProperty("error")!.GetValue(objectResult.Value);
        Assert.Equal("An error occurred while sending the email", error);
    }

    [Fact]
    public async Task SendNotificaionWithPersonalUser_ReturnsOk_WhenServiceSucceeds()
    {
        EmailRequest? request = CreateEmailRequest();
        EmailDto emailDto = CreateEmailDto();

        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);
        _notificationServiceMock
            .Setup(s => s.SendNotificationWithSMTP(emailDto))
            .ReturnsAsync("Email sent successfully");

        IActionResult? result = await _controller.SendNotificaionWithPersonalUser(request);

        OkObjectResult? okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Email sent successfully", okResult.Value);
    }

    [Fact]
    public async Task SendNotificaionWithPersonalUser_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        EmailRequest request = CreateEmailRequest();
        EmailDto emailDto = CreateEmailDto();

        _mapperMock.Setup(m => m.Map<EmailDto>(request)).Returns(emailDto);
        _notificationServiceMock
            .Setup(s => s.SendNotificationWithSMTP(emailDto))
            .ThrowsAsync(new Exception("SMTP failure"));

        IActionResult? result = await _controller.SendNotificaionWithPersonalUser(request);

        BadRequestObjectResult? badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        object? error = badRequestResult.Value!.GetType().GetProperty("error")!.GetValue(badRequestResult.Value);
        Assert.Equal("SMTP failure", error);
    }

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
