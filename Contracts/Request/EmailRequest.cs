using System.ComponentModel.DataAnnotations;

namespace NotificationApi.Contracts.Request
{
    public class EmailRequest
    {
        [StringLength(200)]
        public string? RecipientName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(320)]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(998)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;
    }
}