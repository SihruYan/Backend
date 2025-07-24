using Backend.DILibrary;
using Backend.Environment;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

DotNetEnv.Env.Load(); 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();


// 限制 同一個 ip 在短時間內不能練續送請求
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("FormSubmitPolicy", context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
        });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Try again later.", token);
    };
});

builder.Services.AddRDS(builder.Configuration);
builder.Services.AddInfrastructureServices();
builder.Services.Configure<FormNotifyOptions>(
    builder.Configuration.GetSection("FormNotify"));



var app = builder.Build();
app.UseRouting();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapControllers();
app.Run();
