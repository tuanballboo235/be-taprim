using System.Reflection;
using System.Text;
using BasketballAcademyManagementSystemAPI.Common.Helpers;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TAPrim.API.Middleware;
using TAPrim.Application.DTOs.Common;
using TAPrim.Application.DTOs.Telegram;
using TAPrim.Application.Services;
using TAPrim.Application.Services.ServiceImpl;
using TAPrim.Infrastructure.Telegram;
using TAPrim.Models;
using TAPrim.Shared.Helpers;

Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables();

if (builder.Environment.IsProduction())
{
	builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

// =========================
// Options
// =========================
builder.Services.Configure<TelegramOptions>(
	builder.Configuration.GetSection("Telegram"));

builder.Services.Configure<VietQrDto>(
	builder.Configuration.GetSection("VietQr"));

// Nếu vẫn muốn override từ env thủ công
builder.Services.Configure<VietQrDto>(options =>
{
	options.ClientId = Environment.GetEnvironmentVariable("VietQr__ClientId");
	options.ApiKey = Environment.GetEnvironmentVariable("VietQr__ApiKey");
});

// =========================
// Framework Services
// =========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// =========================
// Database
// =========================
builder.Services.AddDbContext<TaprimContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("MyCnn")
		?? Environment.GetEnvironmentVariable("ConnectionStrings__MyCnn")));

// =========================
// Cache / Redis
// =========================
builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration =
		builder.Configuration.GetConnectionString("Redis")
		?? Environment.GetEnvironmentVariable("ConnectionStrings__Redis");

	options.InstanceName = "NetflixLimiter:";
});

// =========================
// Application Services
// =========================
var assemblies = new[]
{
	Assembly.GetExecutingAssembly()
};

foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
{
	if (type.IsClass
		&& !type.IsAbstract
		&& !typeof(IHostedService).IsAssignableFrom(type)
		&& !typeof(BackgroundService).IsAssignableFrom(type))
	{
		var interfaceType = type.GetInterface($"I{type.Name}");

		if (interfaceType != null)
		{
			builder.Services.AddScoped(interfaceType, type);
		}
	}
}

builder.Services.AddScoped<IJwtSService, JwtService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,

			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
			),

			ClockSkew = TimeSpan.Zero
		};
	});
builder.Services.AddAuthorization();

builder.Services.AddScoped<EmailHelper>();
builder.Services.AddScoped<TransactionCodeHelper>();

// =========================
// Hosted Services
// =========================
builder.Services.AddHostedService<TelegramWebhookHostedService>();

// =========================
// CORS
// =========================
const string reactDevCorsPolicy = "AllowReactDev";

builder.Services.AddCors(options =>
{
	options.AddPolicy(reactDevCorsPolicy, policy =>
	{
		var configuredOrigins = builder.Configuration
			.GetSection("Cors:AllowedOrigins")
			.Get<string[]>() ?? Array.Empty<string>();

		var allowedOrigins = new[]
			{
				"http://localhost:5173",
				"http://127.0.0.1:5173"
			}
			.Concat(configuredOrigins)
			.Where(origin => !string.IsNullOrWhiteSpace(origin))
			.Select(origin => origin.Trim().TrimEnd('/'))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToArray();

		policy.SetIsOriginAllowed(origin =>
			{
				if (string.IsNullOrWhiteSpace(origin)
					|| !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
				{
					return false;
				}

				var normalizedOrigin = origin.Trim().TrimEnd('/');
				var host = uri.Host;

				return allowedOrigins.Contains(normalizedOrigin, StringComparer.OrdinalIgnoreCase)
					|| (uri.Scheme == "http"
						&& (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
							|| host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
					|| (uri.Scheme == "https"
						&& (host.EndsWith(".ngrok-free.dev", StringComparison.OrdinalIgnoreCase)
							|| host.EndsWith(".ngrok.app", StringComparison.OrdinalIgnoreCase)
							|| host.EndsWith(".ngrok.io", StringComparison.OrdinalIgnoreCase)));
			})
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

var app = builder.Build();
// =========================
// Middleware Pipeline
// =========================
app.UseStaticFiles();

app.UseRouting();
app.UseCors(reactDevCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TelegramWebhookAuthMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

// =========================
// Database Init
// =========================
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<TaprimContext>();
	// db.Database.Migrate();
}

app.Run();
