namespace TAPrim.Application.DTOs.Telegram
{
	public class TelegramOptions
	{
		public string TelegramBotToken { get; set; } = string.Empty;
		public string TelegramWebhookUrl { get; set; } = string.Empty;
		public string TelegramSecretToken { get; set; } = string.Empty;
	}
}
