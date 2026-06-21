using System.Collections.Concurrent;
using TAPrim.Application.DTOs.Telegram;
using TAPrim.Application.Services;
using TAPrim.Infrastructure.Repositories;
using TAPrim.Shared.Constants;

namespace TAPrim.Application.Services.ServiceImpl
{
	public class TelegramBotService : ITelegramBotService
	{
		private static readonly ConcurrentDictionary<long, string> WaitingQuantityByChat = new();

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _config;
		private readonly IProductRepository _productRepository;
		private readonly IProductService _productService;

		public TelegramBotService(
			IHttpClientFactory httpClientFactory,
			IConfiguration config,
			IProductRepository productRepository,
			IProductService productService)
		{
			_productRepository = productRepository;
			_productService = productService;
			_httpClientFactory = httpClientFactory;
			_config = config;
		}

		public async Task ProcessUserRequestChatting(TelegramUpdate update)
		{
			if (update.Message?.Text == "/start")
			{
				WaitingQuantityByChat.TryRemove(update.Message.Chat.Id, out _);
				await SendMainMenu(update.Message.Chat.Id);
				return;
			}

			if (update.Message?.Text != null && WaitingQuantityByChat.ContainsKey(update.Message.Chat.Id))
			{
				await HandleQuantityInput(update.Message);
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
					WaitingQuantityByChat.TryRemove(chatId, out _);
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
					await ShowProductDetailByProductOptionIdentifier(chatId, messageId, id);
					break;

				case "cancel":
					WaitingQuantityByChat.TryRemove(chatId, out _);
					await SendTelegramRequest("deleteMessage", new { chat_id = chatId, message_id = messageId });
					await SendProductList(chatId);
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
					WaitingQuantityByChat.TryRemove(chatId, out _);
					await SendTelegramRequest("deleteMessage", new { chat_id = chatId, message_id = messageId });
					await SendProductList(chatId);
					break;

				case "main":
					WaitingQuantityByChat.TryRemove(chatId, out _);
					await EditMainMenu(chatId, messageId);
					break;
			}
		}

		private async Task HandleQuantityInput(TelegramMessage message)
		{
			var chatId = message.Chat!.Id;

			if (!WaitingQuantityByChat.TryGetValue(chatId, out var productOptionIdentifier))
			{
				return;
			}

			if (!int.TryParse(message.Text?.Trim(), out var quantity) || quantity < 1)
			{
				await SendTelegramRequest("sendMessage", new
				{
					chat_id = chatId,
					text = "❌ Số lượng không hợp lệ. Vui lòng nhập số nguyên dương."
				});
				return;
			}

			var response = await _productService.GetProductOptionDetailByIdentifierAsync(productOptionIdentifier);
			if (response.Status != ApiResponseStatusConstant.SuccessStatus || response.Data == null)
			{
				WaitingQuantityByChat.TryRemove(chatId, out _);
				await SendTelegramRequest("sendMessage", new
				{
					chat_id = chatId,
					text = response.Message ?? "Không tìm thấy sản phẩm."
				});
				return;
			}

			var detail = response.Data;
			if (quantity > detail.MaxQuantity)
			{
				await SendTelegramRequest("sendMessage", new
				{
					chat_id = chatId,
					text = $"❌ Số lượng vượt quá tồn kho. Vui lòng nhập từ 1 đến {detail.MaxQuantity}."
				});
				return;
			}

			WaitingQuantityByChat.TryRemove(chatId, out _);

			var totalAmount = (detail.Price ?? 0) * quantity;
			await SendTelegramRequest("sendMessage", new
			{
				chat_id = chatId,
				text = $"""
					✅ Đã chọn {quantity} tài khoản
					📦 Sản phẩm: {detail.ProductOptionName}
					💰 Tổng tiền: {totalAmount:N0}đ

					(Tiếp theo: tạo đơn hàng & thanh toán)
					""",
				reply_markup = BackProductsMarkup()
			});
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

		private async Task SendProductList(long chatId)
		{
			var productOptionTele = await _productRepository.GetListProductTele();
			await SendTelegramRequest("sendMessage", new
			{
				chat_id = chatId,
				text = "Vui lòng chọn sản phẩm:",
				reply_markup = new
				{
					inline_keyboard = productOptionTele
						.Select(x => new[]
						{
							new
							{
								text = x.ProductOptionName,
								callback_data = $"product:view:{x.ProductOptionIdentifier}"
							}
						}).ToArray()
				}
			});
		}

		private async Task EditProductList(long chatId, int messageId)
		{
			var productOptionTele = await _productRepository.GetListProductTele();
			await EditTelegramMessage(
				chatId,
				messageId,
				"Vui lòng chọn sản phẩm:",
				new
				{
					inline_keyboard = productOptionTele
						.Select(x => new[]
						{
							new
							{
								text = x.ProductOptionName,
								callback_data = $"product:view:{x.ProductOptionIdentifier}"
							}
						}).ToArray()
				});
		}

		private async Task ShowProductDetailByProductOptionIdentifier(long chatId, int messageId, string productOptionIdentifier)
		{
			var response = await _productService.GetProductOptionDetailByIdentifierAsync(productOptionIdentifier);
			if (response.Status != ApiResponseStatusConstant.SuccessStatus || response.Data == null)
			{
				await EditTelegramMessage(
					chatId,
					messageId,
					response.Message ?? "❌ Không tìm thấy sản phẩm.",
					BackProductsMarkup());
				return;
			}

			var detail = response.Data;
			var markup = detail.CanPurchase
				? CancelQuantityMarkup()
				: BackProductsMarkup();

			if (detail.CanPurchase)
			{
				WaitingQuantityByChat[chatId] = productOptionIdentifier;
			}
			else
			{
				WaitingQuantityByChat.TryRemove(chatId, out _);
			}

			await SendTelegramRequest("deleteMessage", new
			{
				chat_id = chatId,
				message_id = messageId
			});

			var imageUrl = BuildImageUrl(detail.ProductOptionImage);
			if (!string.IsNullOrWhiteSpace(imageUrl))
			{
				await SendTelegramRequest("sendPhoto", new
				{
					chat_id = chatId,
					photo = imageUrl,
					caption = detail.TelegramHtmlMessage,
					parse_mode = "HTML",
					reply_markup = markup
				});
				return;
			}

			await SendTelegramRequest("sendMessage", new
			{
				chat_id = chatId,
				text = detail.TelegramHtmlMessage,
				parse_mode = "HTML",
				reply_markup = markup
			});
		}

		private string? BuildImageUrl(string? relativePath)
		{
			if (string.IsNullOrWhiteSpace(relativePath))
			{
				return null;
			}

			if (relativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				return relativePath;
			}

			var baseUrl = _config["Telegram:PublicBaseUrl"]?.TrimEnd('/');
			if (string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(_config["Telegram:WebhookUrl"]))
			{
				var webhookUri = new Uri(_config["Telegram:WebhookUrl"]!);
				baseUrl = $"{webhookUri.Scheme}://{webhookUri.Authority}";
			}

			return string.IsNullOrWhiteSpace(baseUrl)
				? null
				: $"{baseUrl}/{relativePath.TrimStart('/')}";
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

		private static object CancelQuantityMarkup()
		{
			return new
			{
				inline_keyboard = new[]
				{
					new[]
					{
						new { text = "❌ Hủy", callback_data = "product:cancel:0" }
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
