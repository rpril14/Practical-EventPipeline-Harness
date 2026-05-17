using EventPipeline.Data;
using EventPipeline.Services.Orders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseMySql(
        builder.Configuration.GetConnectionString("Default")!,
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("Default")!)));
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();
app.MapControllers();
app.Run();
