using AutoMapper;
using NotificationApi.Contracts.Request;
using NotificationApi.Mappers;
using NotificationApi.Services.Dto;

namespace NotificationApi.Tests.Mappers;

public class ContractMappingTests
{
    [Fact]
    public void EmailRequest_MapsToEmailDto()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<ContractMapping>());
        configuration.AssertConfigurationIsValid();

        IMapper? mapper = configuration.CreateMapper();
        EmailRequest? request = new()
        {
            RecipientName = "Jane Doe",
            RecipientEmail = "jane@example.com",
            Subject = "Welcome",
            Body = "Hello Jane"
        };

        EmailDto? dto = mapper.Map<EmailDto>(request);

        Assert.Equal(request.RecipientName, dto.RecipientName);
        Assert.Equal(request.RecipientEmail, dto.RecipientEmail);
        Assert.Equal(request.Subject, dto.Subject);
        Assert.Equal(request.Body, dto.Body);
    }
}
