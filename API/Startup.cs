using ResumableFileTransfer.API.Interfaces;
using ResumableFileTransfer.API.Middlewares;
using ResumableFileTransfer.API.Models;
using ResumableFileTransfer.API.Providers;
using ResumableFileTransfer.Entity;
using ResumableFileTransfer.RfuCore.Interfaces;
using ResumableFileTransfer.RfuCore.Models;
using ResumableFileTransfer.RfuCore.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ResumableFileTransfer.API
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Environment = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IAPIConfiguration>(new APIConfiguration(Configuration));
            services.AddScoped(x => ResumableFileTransferDbContextFactory.Create(x.GetRequiredService<IAPIConfiguration>().ResumableFileTransferDBConnectionString));
            services.AddScoped<IDBContext, DBContext>();
            services.AddSingleton<IRfuConfiguration>(x => CreateConfiguration(x.GetRequiredService<IAPIConfiguration>()));

            if (Environment.IsDevelopment())
            {
                // add swagger service
                services.AddSwaggerGen(s =>
                {
                    s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                    {
                        Version = "v1",
                        Title = "Resumable File Transfer API",
                        Description = "",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact() { Email = "" }
                    });

                    var path = Path.Combine(AppContext.BaseDirectory, "API.xml");
                    s.IncludeXmlComments(path);
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IAPIConfiguration configuration)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            ResumableFileTransferDatabaseBootstrapper.Initialize(configuration.ResumableFileTransferDBConnectionString);
            CommonHelper.Initialize(configuration.EncryptionKey, configuration.RuntimefilesStorageConnectionString);
            Consts.Initialize();

            // custom logger
            loggerFactory.AddAzureLogger(new AzureLoggerConfiguration() { LogLevel = LogLevel.Information, AppName = configuration.AppName });

            // custom error handler
            app.UseErrorHandler();

            // use jwt
            app.UseAuth(new JwtOption() { SecretKey = configuration.JWTSecretKey, ExpireMinutes = configuration.JWTExpireMinutes });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // for getting client ip in controller
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();

            // Enable CORS
            app.UseCors(builder => builder
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowAnyOrigin()
               .WithExposedHeaders(HeadersConstant.Items));

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (Environment.IsDevelopment())
            {
                // use swagger
                app.UseSwagger();
                app.UseSwaggerUI(s =>
                {
                    s.SwaggerEndpoint("v1/swagger.json", "Resumable File Transfer API");
                });
            }
        }

        private RfuConfiguration CreateConfiguration(IAPIConfiguration configuration)
        {
            RfuConfiguration config = new RfuConfiguration()
            {
                MaxFileSize = configuration.MaxFileSize,
                MaxFileNameLength = configuration.MaxFileNameLength,
                UploadBasePath = configuration.AzureBlobUploadBasePath,
                InvalidCharactersInName = configuration.InvalidCharactersInName
            };

            if (configuration.StorageType == StorageType.AzureFileShare)
            {
                config.TargetBasePath = configuration.AzureFileShareTargetBasePath;
                config.DataStore = new AzureFileShareStore(configuration.RuntimefilesStorageConnectionString);
            }
            else if (configuration.StorageType == StorageType.AzureBlob)
            {
                config.TargetBasePath = configuration.AzureBlobTargetBasePath;
                config.DataStore = new AzureBlobStore(configuration.RuntimefilesStorageConnectionString);
            }

            return config;
        }
    }
}
