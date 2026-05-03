
using Microsoft.Extensions.Options;
using TAPrim.Application.DTOs.Telegram;

namespace TAPrim.Infrastructure.Telegram
{
	public sealed class TelegramWebhookHostedService : IHostedService
	{
		private readonly HttpClient _httpClient;
		private readonly TelegramOptions _options;
		private readonly ILogger<TelegramWebhookHostedService> _logger;

		public TelegramWebhookHostedService(
		  IHttpClientFactory httpClientFactory,
		  IOptions<TelegramOptions> options,
		  ILogger<TelegramWebhookHostedService> logger)
		{
			_httpClient = httpClientFactory.CreateClient();
			_options = options.Value;
			_logger = logger;
		}
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(_options.TelegramSecretToken) ||
						string.IsNullOrWhiteSpace(_options.Telegram__WebhookUrl))
			{
				_logger.LogWarning("Telegram webhook chưa được cấu hình đầy đủ.");
				return;
			}

			var url =
				$"https://api.telegram.org/bot{_options.TelegramBotToken}/setWebhook";

			var payload = new
			{
				url = _options.Telegram__WebhookUrl,
				secret_token = _options.TelegramSecretToken,
				drop_pending_updates = false
			};

			try
			{
				var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
				var content = await response.Content.ReadAsStringAsync(cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					_logger.LogInformation("Set Telegram webhook thành công: {WebhookUrl}", _options.Telegram__WebhookUrl);
					_logger.LogInformation("Telegram response: {Response}", content);
				}
				else
				{
					_logger.LogError("Set Telegram webhook thất bại. Status: {StatusCode}. Response: {Response}",
						response.StatusCode, content);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi gọi setWebhook.");
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
