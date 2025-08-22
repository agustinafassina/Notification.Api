using AutoMapper;
using NotificationApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NotificationApi.Contracts.Request;
using NotificationApi.Services.Dto;
using System.Threading.Tasks;

namespace NotificationApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _itemService;
        private readonly IMapper _mapper;

        public NotificationController(INotificationService itemService, IMapper mapper)
        {
            _itemService = itemService;
            _mapper = mapper;
        }

        [HttpPost("send-aws")]
        public async Task<IActionResult> SendNotificationWithAWS([FromBody] EmailRequest request)
        {
            EmailDto emailRequest = _mapper.Map<EmailDto>(request);
            bool status = await _itemService.SendNotificationWithSeSAws(emailRequest);
            return Ok(status);
        }

        [HttpPost("send-gmail")]
        public async Task<IActionResult> SendNotificaionWithPersonalUser([FromBody] EmailRequest request)
        {
            try
            {
                EmailDto emailRequest = _mapper.Map<EmailDto>(request);
                bool status = await _itemService.SendNotificationWithSMTP(emailRequest);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            return Ok("v1.0.0");
        }
    }
}