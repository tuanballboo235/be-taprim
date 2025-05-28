using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TAPrim.Models;
using TAPrim.Application.Services.ServiceImpl;
using TAPrim.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container.
builder.Services.AddControllers();

// Cấu hình Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TaprimContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));
// Đăng ký các dịch vụ thông qua Reflection
var assembly = Assembly.GetExecutingAssembly();
foreach (var type in assembly.GetTypes())
{
	// Kiểm tra nếu là class (không phải abstract hoặc BackgroundService)
	if (type.IsClass && !type.IsAbstract && !typeof(BackgroundService).IsAssignableFrom(type))
	{
		// Lấy interface đầu tiên mà lớp implement
		var interfaceType = type.GetInterfaces().FirstOrDefault();
		if (interfaceType != null)
		{
			builder.Services.AddScoped(interfaceType, type);  // Đăng ký dịch vụ theo interface và lớp thực thi
		}
	}
}
builder.Services.AddHttpClient();  // Đăng ký HttpClient vào DI container
builder.Services.AddScoped<INetflixService, NetflixService>();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowReactDev",
		builder =>
		{
			builder
				.WithOrigins("http://localhost:5173") // ✅ port React đang chạy
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials(); // nếu dùng cookies/auth
		});
});

var app = builder.Build();
app.UseCors("AllowReactDev");
// Cấu hình pipeline HTTP request.
if (app.Environment.IsDevelopment())
{
	// Cấu hình Swagger UI chỉ trong môi trường Development
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseStaticFiles(); // Cho phép truy cập file tĩnh trong wwwroot

app.UseHttpsRedirection();

// Sử dụng Authorization (nếu cần)
app.UseAuthorization();

// Ánh xạ các controller
app.MapControllers();

app.Run();
