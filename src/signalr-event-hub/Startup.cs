using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using signalr_event_hub.Hubs;
using StackExchange.Redis;

namespace signalr_event_hub
{
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
                        services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
                {
                    builder
                        
                        .AllowAnyMethod().AllowCredentials()
                        .AllowAnyHeader().WithOrigins("http://localhost:5050");
                }));
            services.AddMvc();

            services.AddSignalR();
            services.AddMetrics();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("MyPolicy");
            app.UseMvc();

            app.UseSignalR(routes =>
            {
                routes.MapHub<EventHub>("/eventhub");
            });

        }
    }
}

