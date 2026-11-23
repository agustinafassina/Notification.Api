using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NotificationApi.Services.Exceptions;

namespace NotificationApi.Configurations
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ProblemDetails problem;

            if (exception is EmailDeliveryException deliveryException)
            {
                _logger.LogError(exception, "Email delivery through {Provider} failed", deliveryException.Provider);
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "Email could not be sent",
                    Detail = $"The {deliveryException.Provider} provider failed to send the email."
                };
            }
            else
            {
                _logger.LogError(exception, "Unhandled exception");
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred"
                };
            }

            problem.Instance = httpContext.Request.Path;
            httpContext.Response.StatusCode = problem.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", cancellationToken);
            return true;
        }
    }
}