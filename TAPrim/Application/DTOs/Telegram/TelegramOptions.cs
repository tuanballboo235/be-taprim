namespace TAPrim.Application.DTOs.Telegram
{
	public class TelegramOptions
    {
        public string BotToken { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public string SecretToken { get; set; } = string.Empty;
        public string PublicBaseUrl { get; set; } = string.Empty;
    }

}
