using BlazorApp1.Server;
using BlazorApp1.Server.Hubs;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Ports;
using Microsoft.Extensions.Hosting.WindowsServices;
using BlazorApp1.Server.Test;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();


builder.Services.AddCors(options =>
{
    options.AddPolicy("NewPolicy", builder =>
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
builder.Services.AddHostedService<AppInitializationService>();
builder.Services.AddSingleton<AppInitializationService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.UseCors("NewPolicy");
app.MapHub<NotificationHub>("/notificationHub");
app.Run();


