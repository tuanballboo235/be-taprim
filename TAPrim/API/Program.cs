using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TAPrim.Models;
using TAPrim.Shared.Helpers;
using DotNetEnv;
using TAPrim.Application.DTOs.Common;
using BasketballAcademyManagementSystemAPI.Common.Helpers;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

// ✅ Lắng nghe đúng cổng khi chạy trong Docker production
if (builder.Environment.IsProduction())
{
	builder.WebHost.UseUrls("http://0.0.0.0:8080");
}
/* Cấu hình env
 */
// Nếu cần, vẫn có thể load .env (nếu không inject từ system env)
Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

// Load cấu hình
builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
	.AddEnvironmentVariables(); // load biến từ system hoặc từ .env đã Load()

//==========================================

builder.Services.AddAutoMapper(typeof(Program));

// ✅ Đăng ký dịch vụ & middleware
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Cấu hình DbContext
builder.Services.AddDbContext<TaprimContext>(options =>
	options.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__MyCnn")));

builder.Services.Configure<VietQrDto>(
	builder.Configuration.GetSection("VietQr")
);
// ✅ Đăng ký config section VietQr
builder.Services.Configure<VietQrDto>(options =>
{
	options.ClientId = Environment.GetEnvironmentVariable("VietQr__ClientId");
	options.ApiKey = Environment.GetEnvironmentVariable("VietQr__ApiKey");
});
builder.Services.AddMemoryCache();

builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = Environment.GetEnvironmentVariable("ConnectionStrings__Redis"); // ← THAY ĐOẠN NÀY
	options.InstanceName = "NetflixLimiter:";
});
Console.WriteLine("Redis config: " + builder.Configuration["ConnectionStrings__Redis"]);


// ✅ Đăng ký dịch vụ qua reflection
var assembly = Assembly.GetExecutingAssembly();
foreach (var type in assembly.GetTypes())
{
	if (type.IsClass && !type.IsAbstract && !typeof(BackgroundService).IsAssignableFrom(type))
	{
		var interfaceType = type.GetInterfaces().FirstOrDefault();
		if (interfaceType != null)
		{
			builder.Services.AddScoped(interfaceType, type);
		}
	}
}
builder.Services.AddScoped<EmailHelper>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<TransactionCodeHelper>();

// ✅ CORS - chỉ dùng khi dev hoặc cần allow FE IP cụ thể
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins(
				"http://localhost:5173",
				"http://103.238.235.227:8080"
			)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

var app = builder.Build();

// ✅ Auto migrate DB nếu cần
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<TaprimContext>();
	db.Database.Migrate(); // hoặc EnsureCreated()
}

app.UseStaticFiles();
app.UseRouting();

// ✅ Swagger (có thể ẩn nếu cần)
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();
app.Run();
