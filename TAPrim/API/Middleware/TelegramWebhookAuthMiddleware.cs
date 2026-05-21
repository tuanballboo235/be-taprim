namespace TAPrim.API.Middleware
{
	public class TelegramWebhookAuthMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IConfiguration _config;

		public TelegramWebhookAuthMiddleware(RequestDelegate next, IConfiguration config)
		{
			_next = next;
			_config = config;
		}

		public async Task Invoke(HttpContext context)
		{
			if (context.Request.Path.StartsWithSegments("/telegram/telegram"))
			{
				var expected = _config["Telegram:SecretToken"];
				var actual = context.Request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault();

				if (string.IsNullOrEmpty(expected) || actual != expected)
				{
					context.Response.StatusCode = 401;
					await context.Response.WriteAsync("Unauthorized");
					return;
				}
			}

			await _next(context);
		}
	}
}
