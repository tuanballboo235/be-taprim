using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TAPrim.Application.DTOs.Telegram;
using TAPrim.Application.Services;

namespace TAPrim.API.Controllers
{
	[ApiController]
	public class TelegramBotController : ControllerBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _config;
		private readonly ITelegramBotService _telegramBotService;

		public TelegramBotController(IHttpClientFactory httpClientFactory, IConfiguration config, ITelegramBotService telegramBotService)
		{
			_httpClientFactory = httpClientFactory;
			_config = config;
			_telegramBotService = telegramBotService;
		}

		[HttpPost("telegram")]
		public async Task<IActionResult> Webhook([FromBody] TelegramUpdate update)
		{
			if (update.Message?.Text == "/start")
			{
				await SendMainMenu(update.Message.Chat.Id);
				return Ok();
			}

			if (update.Message?.Text == "🛒 Mua hàng")
			{
				await SendProductList(update.Message.Chat.Id);
				return Ok();
			}

			if (update.Callback_Query != null)
			{
				await HandleCallback(update.Callback_Query);
				return Ok();
			}

			return Ok();
		}

		private async Task SendMainMenu(long chatId)
		{
			await SendTelegramRequest("sendMessage", new
			{
				chat_id = chatId,
				text = "Chào bạn 👋\nVui lòng chọn chức năng:",
				reply_markup = new
				{
					keyboard = new[]
						{
							new[] { new { text = "🛒 Mua hàng", callback_data = "MENU_MUA_HANG" } },
							new[]
							{
								new { text = "📦 Đơn hàng", callback_data = "MENU_DON_HANG" },
								new { text = "💰 Số dư", callback_data = "MENU_SO_DU" },
								new { text = "💳 Nạp tiền", callback_data = "MENU_NAP_TIEN" },
								new { text = "☎️ Hỗ trợ", callback_data = "MENU_HO_TRO" }
							}
						}

				}
			});
		}

		private async Task SendProductList(long chatId)
		{
			await SendTelegramRequest("sendMessage", new
			{
				chat_id = chatId,
				text = "Vui lòng chọn sản phẩm:",
				reply_markup = new
				{
					inline_keyboard = new[]
					{
						new[]
						{
							new { text = "Netflix 1 tháng", callback_data = "PRODUCT_1" }
						},
						new[]
						{
							new { text = "Spotify 3 tháng", callback_data = "PRODUCT_2" }
						},
						new[]
						{
							new { text = "YouTube Premium", callback_data = "PRODUCT_3" }
						}
					}
				}
			});
		}

		private async Task HandleCallback(TelegramCallbackQuery callback)
		{
			await SendTelegramRequest("answerCallbackQuery", new
			{
				callback_query_id = callback.Id,
				text = "Đang xử lý đơn hàng cho bạn...", // Hiện thông báo nhỏ
				show_alert = false // false: hiện thông báo tự tắt ở mép màn hình, true: hiện Popup bắt bấm OK
			});


			var chatId = callback.Message.Chat.Id;
			var data = callback.Data;

			if (data == "PRODUCT_1")
			{
				await SendTelegramRequest("sendMessage", new
				{
					chat_id = chatId,
					text = "Bạn đã chọn Netflix 1 tháng.\nGiá: 99.000đ\n\nBạn có muốn mua không?",
					reply_markup = new
					{
						inline_keyboard = new[]
						{
							new[]
							{
								new { text = "✅ Mua ngay", callback_data = "BUY_PRODUCT_1" }
							},
							new[]
							{
								new { text = "⬅️ Quay lại", callback_data = "BACK_PRODUCT_LIST" }
							}
						}
					}
				});
			}

			if (data == "PRODUCT_2")
			{
				await SendTelegramRequest("sendMessage", new
				{
					chat_id = chatId,
					text = "Bạn đã chọn Spotify 3 tháng.\nGiá: 129.000đ"
				});
			}

			if (data == "PRODUCT_3")
			{
				await SendTelegramRequest("sendMessage", new
				{
					chat_id = chatId,
					text = "Bạn đã chọn YouTube Premium.\nGiá: 149.000đ"
				});
			}

			if (data == "BACK_PRODUCT_LIST")
			{
				await SendProductList(chatId);
			}
		}

		private async Task SendTelegramRequest(string method, object payload)
		{
			var botToken = _config["TelegramBotToken"];
			var client = _httpClientFactory.CreateClient();

			var url = $"https://api.telegram.org/bot{botToken}/{method}";
			await client.PostAsJsonAsync(url, payload);
		}
	}
}