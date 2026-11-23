using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationApi.Contracts.Request;
using NotificationApi.Services.Dto;
using NotificationApi.Services.Interfaces;

namespace NotificationApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/[controller]")]
    public class NotificationController : ControllerBase
    {
        private const string SentMessage = "Email sent successfully";

        private readonly INotificationService _itemService;
        private readonly IMapper _mapper;

        public NotificationController(INotificationService itemService, IMapper mapper)
        {
            _itemService = itemService;
            _mapper = mapper;
        }

        // Failures surface as EmailDeliveryException and are turned into a 502 by GlobalExceptionHandler.
        [HttpPost("send-ses")]
        public async Task<IActionResult> SendNotificationWithAWS([FromBody] EmailRequest request)
        {
            EmailDto emailRequest = _mapper.Map<EmailDto>(request);
            await _itemService.SendNotificationWithSeSAws(emailRequest);
            return Ok(new { message = SentMessage });
        }

        [HttpPost("send-smpt")]
        public async Task<IActionResult> SendNotificaionWithPersonalUser([FromBody] EmailRequest request)
        {
            EmailDto emailRequest = _mapper.Map<EmailDto>(request);
            await _itemService.SendNotificationWithSMTP(emailRequest);
            return Ok(new { message = SentMessage });
        }

        [AllowAnonymous]
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            return Ok("v1.0.0");
        }
    }
}
