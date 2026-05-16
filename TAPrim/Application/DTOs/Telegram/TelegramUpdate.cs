using static TAPrim.API.Controllers.TelegramBotController;

namespace TAPrim.Application.DTOs.Telegram
{
	public class TelegramUpdate
	{
		public long Update_Id { get; set; }
		public TelegramMessage? Message { get; set; }
		public TelegramCallbackQuery? Callback_Query { get; set; }
	}
}
