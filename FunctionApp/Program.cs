using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResumableFileTransfer.Entity;
using ResumableFileTransfer.FunctionApp;
using System;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true);

builder.Services.AddFunctionsWorkerDefaults();

Config.Initialize(builder.Configuration);
ResumableFileTransferDatabaseBootstrapper.Initialize(Config.ResumableFileTransferDBConnectionString);

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
