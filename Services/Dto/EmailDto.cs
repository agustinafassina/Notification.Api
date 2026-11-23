namespace NotificationApi.Services.Dto
{
    public class EmailDto
    {
        public string? RecipientName { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
