
using AutoMapper;
using NotificationApi.Contracts.Request;
using NotificationApi.Services.Dto;

namespace NotificationApi.Mappers
{
    public class ContractMapping : Profile
    {
        public ContractMapping()
        {
            CreateMap<EmailRequest, EmailDto>();
        }
    }
}