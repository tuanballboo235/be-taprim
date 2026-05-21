using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TAPrim.Application.DTOs.Telegram;
using TAPrim.Application.Services;

namespace TAPrim.API.Controllers
{
	[ApiController]
	public class TelegramBotController : ControllerBase
	{
	
		private readonly ITelegramBotService _telegramBotService;

		public TelegramBotController(IHttpClientFactory httpClientFactory, IConfiguration config, ITelegramBotService telegramBotService)
		{
			_telegramBotService = telegramBotService;
		}

		[HttpPost("telegram")]
		public async Task<IActionResult> Webhook([FromBody] TelegramUpdate update)
		{
			await _telegramBotService.ProcessUserRequestChatting(update);
			return Ok();
		}


	}
}