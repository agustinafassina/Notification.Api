using System.ComponentModel.DataAnnotations;
using NotificationApi.Contracts.Request;

namespace NotificationApi.Tests.Contracts;

public class EmailRequestValidationTests
{
    [Fact]
    public void ValidRequest_PassesValidation()
    {
        Assert.Empty(Validate(CreateValidRequest()));
    }

    [Fact]
    public void RecipientName_IsOptional()
    {
        EmailRequest request = CreateValidRequest();
        request.RecipientName = null;

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void InvalidRecipientEmail_FailsValidation(string email)
    {
        EmailRequest request = CreateValidRequest();
        request.RecipientEmail = email;

        Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(EmailRequest.RecipientEmail)));
    }

    [Fact]
    public void MissingSubjectAndBody_FailValidation()
    {
        EmailRequest request = CreateValidRequest();
        request.Subject = "";
        request.Body = "";

        List<ValidationResult> results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EmailRequest.Subject)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EmailRequest.Body)));
    }

    private static List<ValidationResult> Validate(EmailRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }

    private static EmailRequest CreateValidRequest() => new()
    {
        RecipientName = "John Doe",
        RecipientEmail = "john@example.com",
        Subject = "Hello",
        Body = "<p>Hi</p>"
    };
}
