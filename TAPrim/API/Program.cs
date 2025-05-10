using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Đăng ký các dịch vụ thông qua Reflection
var assembly = Assembly.GetExecutingAssembly();
foreach (var type in assembly.GetTypes())
{
	if (type.IsClass && !type.IsAbstract && !typeof(BackgroundService).IsAssignableFrom(type))
	{
		var interfaceType = type.GetInterfaces().FirstOrDefault();
		if (interfaceType != null)
		{
			builder.Services.AddScoped(interfaceType, type);  // Đăng ký theo interface và lớp thực thi
		}
	}
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
