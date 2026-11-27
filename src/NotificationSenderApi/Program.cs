using Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Data.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("NotificationDatabase");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string is not configured");
}

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<AuditTimestampsInterceptor>();
builder.Services.AddDbContext<NotificationDbContext>((serviceProvider, options) =>
    options.UseNpgsql(connectionString)
    .AddInterceptors(serviceProvider.GetRequiredService<AuditTimestampsInterceptor>()));
builder.Services.AddControllers();
builder.Services.AddOpenApi();
var app = builder.Build();

// Configure the HTTP request pipeline.

if(app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
