using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "Resumable File Scanning Service";
});

var hostingUrl = builder.Configuration["HostingUrl"];
if (string.IsNullOrWhiteSpace(hostingUrl) == false)
{
    builder.WebHost.UseUrls(hostingUrl);
}

var maxRequestLength = builder.Configuration.GetValue<long?>("MaxRequestLength");
if (maxRequestLength.HasValue)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = maxRequestLength.Value;
    });
}

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
