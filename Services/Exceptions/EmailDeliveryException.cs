namespace NotificationApi.Services.Exceptions
{
    public class EmailDeliveryException : Exception
    {
        public string Provider { get; }

        public EmailDeliveryException(string provider, Exception innerException)
            : base($"Failed to send email through {provider}.", innerException)
        {
            Provider = provider;
        }
    }
}
