using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NotificationApi.Configurations;
using NotificationApi.Services.Exceptions;

namespace NotificationApi.Tests.Configurations;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task EmailDeliveryException_Returns502WithoutInternalDetails()
    {
        DefaultHttpContext context = CreateContext();
        var exception = new EmailDeliveryException("SES", new Exception("secret internal detail"));

        bool handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status502BadGateway, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        string body = ReadBody(context);
        Assert.Contains("Email could not be sent", body);
        Assert.DoesNotContain("secret internal detail", body);
    }

    [Fact]
    public async Task UnexpectedException_Returns500WithoutInternalDetails()
    {
        DefaultHttpContext context = CreateContext();

        bool handled = await _handler.TryHandleAsync(context, new InvalidOperationException("secret internal detail"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(ReadBody(context));
        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.DoesNotContain("secret internal detail", json.RootElement.GetRawText());
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/Notification/send-ses";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static string ReadBody(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return new StreamReader(context.Response.Body).ReadToEnd();
    }
}
