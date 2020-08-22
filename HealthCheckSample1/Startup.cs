using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthCheckSample1
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            // Health
            services.AddHealthChecks()
                .AddUrlGroup(uri: new Uri("http://localhost:5000/WeatherForecast"), name: "Pocasi")
                .AddMySql(Configuration["ConnectionString"], name: "MySQL")
                .AddCheck<CustomHealthCheck>(name: "Custom")
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Health
            app.UseHealthChecks("/health", CreateHealthCheckOptions());
            
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private static HealthCheckOptions CreateHealthCheckOptions()
        {
            var result = new HealthCheckOptions();

            result.ResponseWriter = async (c, r) =>
            {
                c.Response.ContentType = MediaTypeNames.Application.Json;

                var jsonresult = System.Text.Json.JsonSerializer.Serialize(new
                {
                    checks = r.Entries.Select(e =>
                        new {
                            description  = e.Key,
                            status       = e.Value.Status.ToString(),
                            responseTime = e.Value.Duration.TotalMilliseconds
                        }),
                    totalResponseTime = r.TotalDuration.TotalMilliseconds
                });

                await c.Response.WriteAsync(jsonresult);
            };
            
            return result;
        }
    }
}