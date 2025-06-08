using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TAPrim.Models;
using TAPrim.Application.Services.ServiceImpl;
using TAPrim.Application.Services;
using TAPrim.Application.DTOs;
using TAPrim.Shared.Helpers;

var builder = WebApplication.CreateBuilder(args);

// ✅ Lắng nghe đúng cổng Docker expose
if (builder.Environment.IsProduction())
{
	builder.WebHost.UseUrls("http://0.0.0.0:8080");
}


// Thêm các dịch vụ vào container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Cấu hình DbContext
builder.Services.AddDbContext<TaprimContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// ✅ Đăng ký dịch vụ tự động qua reflection
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

builder.Services.AddHttpClient();

// ✅ CORS chỉ bật trong dev
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy
			.WithOrigins("http://localhost:5173", "http://103.238.235.227:8080") // 👈 sửa theo IP React app
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});


// ✅ Đăng ký config và helpers
builder.Services.Configure<VietQrDto>(builder.Configuration.GetSection("VietQr"));
builder.Services.AddScoped<TransactionCodeHelper>();

var app = builder.Build();

// ✅ Tự động migrate DB nếu cần
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<TaprimContext>();
	db.Database.Migrate(); // hoặc db.EnsureCreated() nếu không dùng migration
}

// ✅ Middleware pipeline
app.UseRouting();

// ✅ Luôn bật Swagger ở mọi môi trường
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend"); // 👈 không cần if


app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();
app.Run();
