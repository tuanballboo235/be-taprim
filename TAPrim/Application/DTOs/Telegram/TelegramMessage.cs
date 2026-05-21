using static TAPrim.API.Controllers.TelegramBotController;

namespace TAPrim.Application.DTOs.Telegram
{
	public class TelegramMessage
	{
		public int Message_Id { get; set; }
		public TelegramUser? From { get; set; }
		public TelegramChat Chat { get; set; } = null!;
		public string? Text { get; set; }
		public long Date { get; set; }
	}
}
