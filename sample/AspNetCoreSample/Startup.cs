using System;
using AspNetCoreSample.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AspNetCoreSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }

        public IConfiguration Configuration { get; }
        public IServiceProvider ServiceProvider { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RequestTelemetryEnricherOptions>(Configuration);
            /*
             * Read ApplicationInsights configuration from appsettings.json,
             * the environment config and environment variables.
             *
             * This overload will configure TelemetryConfiguration.Active which the
             * Serilog extension method will use. Explicitly passing Configuration
             * will cause TelemetryConfiguration.Active to not be set.
             */
            services.AddApplicationInsightsTelemetry();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.ApplicationInsightsTraces()
                .CreateLogger();

            services.AddMvc(o =>
            {
                o.Filters.Add<LoggingExceptionFilter>();
                o.Filters.Add<RequestTelemetryEnricher>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<Startup>>();

            /*
             * BeginScope is stored in Custom dimensions.
             * To look for messages enriched with this BeginScope()
             * Use this application insights query.:
             *
             *  traces
             *  | where customDimensions.SourceContext == 'AspNetCoreSample.Startup'
             *  | project timestamp, message, customDimensions.SourceContext, customDimensions
             *
             *  
             */
            using (logger.BeginScope(new { env.ApplicationName, env.ContentRootPath, env.EnvironmentName, env.WebRootPath }))
            {
                if (env.IsDevelopment())
                {
                    logger.LogTrace("Configuring Development Environment");
                    app.UseBrowserLink();
                    app.UseDeveloperExceptionPage();

                }
                else
                {
                    logger.LogTrace("Configuring non-development environment");
                    app.UseExceptionHandler("/Error");
                }

                logger.LogTrace("Enabing static file support");
                app.UseStaticFiles();

                logger.LogTrace("Enabling MVC");
                app.UseMvc();
            }
        }
    }
}
