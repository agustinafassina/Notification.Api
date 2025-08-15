using AutoMapper;
using NotificationApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("send-aws")]
        public async Task<IActionResult> SendNotificationWithAWS()
        {
            return Ok("Cleooo :!");
        }

        [HttpGet("send-gmail")]
        public IActionResult SendNotificaionWithPersonalUser()
        {
            IEnumerable<Services.Dto.ItemDto>? items = _itemService.GetAllItems();
            return Ok(items);
        }
    }
}