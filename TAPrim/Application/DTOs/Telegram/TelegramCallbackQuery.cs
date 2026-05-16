using static TAPrim.API.Controllers.TelegramBotController;

namespace TAPrim.Application.DTOs.Telegram
{
	public class TelegramCallbackQuery
	{
		public string Id { get; set; } = string.Empty;
		public TelegramUser From { get; set; } = null!;
		public TelegramMessage Message { get; set; } = null!;
		public string Data { get; set; } = string.Empty;
	}
}
