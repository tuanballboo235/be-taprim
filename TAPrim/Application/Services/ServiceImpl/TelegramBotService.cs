using TAPrim.Application.DTOs.Telegram;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class TelegramBotService : ITelegramBotService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _config;

		public TelegramBotService(IHttpClientFactory httpClientFactory, IConfiguration config)
		{
			_httpClientFactory = httpClientFactory;
			_config = config;
		}

		public async Task ProcessUserRequestChatting(TelegramUpdate update)
		{
			if (update.Message?.Text == "/start")
			{
				await SendMainMenu(update.Message.Chat.Id);
				return;
			}

			if (update.Callback_Query != null)
			{
				await HandleCallback(update.Callback_Query);
			}
		}

		private async Task HandleCallback(TelegramCallbackQuery callback)
		{
			await SendTelegramRequest("answerCallbackQuery", new
			{
				callback_query_id = callback.Id,
				text = "Đang xử lý...",
				show_alert = false
			});

			if (callback.Message == null || string.IsNullOrWhiteSpace(callback.Data))
			{
				return;
			}

			var chatId = callback.Message.Chat.Id;
			var messageId = callback.Message.Message_Id;

			var parts = callback.Data.Split(':');

			var module = parts[0];
			var action = parts.Length > 1 ? parts[1] : string.Empty;
			var id = parts.Length > 2 ? parts[2] : string.Empty;

			switch (module)
			{
				case "menu":
					await HandleMenuCallback(chatId, messageId, action);
					break;

				case "product":
					await HandleProductCallback(chatId, messageId, action, id);
					break;

				case "back":
					await HandleBackCallback(chatId, messageId, action);
					break;
			}
		}

		private async Task HandleMenuCallback(long chatId, int messageId, string action)
		{
			switch (action)
			{
				case "products":
					await EditProductList(chatId, messageId);
					break;

				case "orders":
					await EditTelegramMessage(chatId, messageId, "📦 Danh sách đơn hàng của bạn", BackMainMarkup());
					break;

				case "balance":
					await EditTelegramMessage(chatId, messageId, "💰 Số dư hiện tại: 0đ", BackMainMarkup());
					break;

				case "topup":
					await EditTelegramMessage(chatId, messageId, "💳 Vui lòng chọn số tiền cần nạp", BackMainMarkup());
					break;

				case "support":
					await EditTelegramMessage(chatId, messageId, "☎️ Liên hệ admin: @youradmin", BackMainMarkup());
					break;
			}
		}

		private async Task HandleProductCallback(long chatId, int messageId, string action, string id)
		{
			switch (action)
			{
				case "view":
					await EditProductDetail(chatId, messageId, id);
					break;

				case "buy":
					await EditTelegramMessage(chatId, messageId, $"✅ Đang tạo đơn hàng cho sản phẩm #{id}", BackProductsMarkup());
					break;
			}
		}

		private async Task HandleBackCallback(long chatId, int messageId, string action)
		{
			switch (action)
			{
				case "products":
					await EditProductList(chatId, messageId);
					break;

				case "main":
					await EditMainMenu(chatId, messageId);
					break;
			}
		}

		private async Task SendMainMenu(long chatId)
		{
			await SendTelegramRequest("sendMessage", new
			{
				chat_id = chatId,
				text = "Chào bạn 👋\nVui lòng chọn chức năng:",
				reply_markup = MainMenuMarkup()
			});
		}

		private async Task EditMainMenu(long chatId, int messageId)
		{
			await EditTelegramMessage(
				chatId,
				messageId,
				"Chào bạn 👋\nVui lòng chọn chức năng:",
				MainMenuMarkup()
			);
		}

		private async Task EditProductList(long chatId, int messageId)
		{
			await EditTelegramMessage(
				chatId,
				messageId,
				"Vui lòng chọn sản phẩm:",
				new
				{
					inline_keyboard = new[]
					{
						new[]
						{
							new { text = "Netflix 1 tháng", callback_data = "product:view:1" }
						},
						new[]
						{
							new { text = "Spotify 3 tháng", callback_data = "product:view:2" }
						},
						new[]
						{
							new { text = "YouTube Premium", callback_data = "product:view:3" }
						},
						new[]
						{
							new { text = "⬅️ Quay lại", callback_data = "back:main" }
						}
					}
				}
			);
		}

		private async Task EditProductDetail(long chatId, int messageId, string id)
		{
			switch (id)
			{
				case "1":
					await EditTelegramMessage(chatId, messageId, """
					🎬 Netflix 1 tháng
					💰 Giá: 99.000đ

					Bạn có muốn mua không?
					""", BuyMarkup(id));
					break;

				case "2":
					await EditTelegramMessage(chatId, messageId, """
					🎵 Spotify 3 tháng
					💰 Giá: 129.000đ

					Bạn có muốn mua không?
					""", BuyMarkup(id));
					break;

				case "3":
					await EditTelegramMessage(chatId, messageId, """
					▶️ YouTube Premium
					💰 Giá: 149.000đ

					Bạn có muốn mua không?
					""", BuyMarkup(id));
					break;
			}
		}

		private object MainMenuMarkup()
		{
			return new
			{
				inline_keyboard = new[]
				{
					new[]
					{
						new { text = "🛒 Mua hàng", callback_data = "menu:products" }
					},
					new[]
					{
						new { text = "📦 Đơn hàng", callback_data = "menu:orders" },
						new { text = "💰 Số dư", callback_data = "menu:balance" }
					},
					new[]
					{
						new { text = "💳 Nạp tiền", callback_data = "menu:topup" },
						new { text = "☎️ Hỗ trợ", callback_data = "menu:support" }
					}
				}
			};
		}

		private object BuyMarkup(string id)
		{
			return new
			{
				inline_keyboard = new[]
				{
					new[]
					{
						new { text = "✅ Mua ngay", callback_data = $"product:buy:{id}" }
					},
					new[]
					{
						new { text = "⬅️ Quay lại", callback_data = "back:products" }
					}
				}
			};
		}

		private object BackMainMarkup()
		{
			return new
			{
				inline_keyboard = new[]
				{
					new[]
					{
						new { text = "⬅️ Quay lại", callback_data = "back:main" }
					}
				}
			};
		}

		private object BackProductsMarkup()
		{
			return new
			{
				inline_keyboard = new[]
				{
					new[]
					{
						new { text = "⬅️ Quay lại sản phẩm", callback_data = "back:products" }
					}
				}
			};
		}

		private async Task EditTelegramMessage(long chatId, int messageId, string text, object? replyMarkup = null)
		{
			await SendTelegramRequest("editMessageText", new
			{
				chat_id = chatId,
				message_id = messageId,
				text,
				reply_markup = replyMarkup
			});
		}

		private async Task SendTelegramRequest(string method, object payload)
		{
			var botToken = _config["Telegram:BotToken"];
			var client = _httpClientFactory.CreateClient();

			var url = $"https://api.telegram.org/bot{botToken}/{method}";
			await client.PostAsJsonAsync(url, payload);
		}
	}
}