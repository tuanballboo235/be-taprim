using TAPrim.Application.DTOs.Telegram;

namespace TAPrim.Application.Services
{
	public interface ITelegramBotService
	{
		 Task ProcessUserRequestChatting(TelegramUpdate telegramUpdate); 
	}
}
