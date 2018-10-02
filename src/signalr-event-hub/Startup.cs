using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

            
            var key = Encoding.ASCII.GetBytes("MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDFJyYvNjbm4oNpOV02fVwIpg63Afl3FC0GjvrjM3Q1ni7ClOyrxXK5zbgtQFkyVhl/CMgNXjXL013Y5SIPGBsQ5rFVQjAnwZDhgSD5R1eY55QvE3QjF2XuszQedGyUaG7dRRu/00GFYIEF7CcDdOveiiDOvOeMNtb4kztqGO7Xr2oBQNEzWLKEPBq7zucrj9nKOKl1/RlNa+eiOV+9elaytIKJdhr8lKCjzAPPeQfJY7eybUwcoWzlZl8b0nU6fvJKoDNTMk4yEK3SbeFvxOKYOGj2e6FGTKMiXkhfvEDAkJLJwKkdVWSFRjgb/7PA1v259KmV3gk3T1rQ66+29OrRAgMBAAECggEAe6n21Z5YCbMDYrlMsqUnWXVvvXNLm1nYdEizLlhUCF3UTtFDMuuC7vEPGbNHP7+p9nj3owr5C4TlVOtE1dr0/0D08tm1gvpzej+ZA0OwuoRn+q9lJa3Djlpx0riMcvqer8Rth4Fnk9XYmHJsdkqcuNZDheoQA29SoFEZ7478IeUXAFyDRpS0EQlmUlTn5P4fEb8z3vSc7q2aY5wFUiaUKX0ugPK+777gi38h7fIwdNPY01k6jiF/97UcP/ANwt94aR3bu37mhuFeinVUi0tfjHo4LGL6P7exggK4sOQLLd2JIq3TEgMOmcALpx2LBFdXu7QjYd9wbzZwDx+aI0fQAQKBgQDzrQS6cQEGw60la1u85yi7g1Bca0RfyR+yYl6qH4H4rZGIXF9oTgZ9P0B46ZHwbgAekV+JKWZrPt26VBF1GyEo8fF6WJGQ/F1qw2Q5flMR2ueQIPrEFWA/g1TEFFVc6Fp2TS4xBtbWJg4FEbHu4xBycZYzUElqvD+So3nob6GukQKBgQDPH8b9Qcg5AYO6VD2xxAdwSBBlXZG3YQJknuVXGFQlbJRXuYxfnc3uUdb2DduK/sv18uH7mXRccJ8v/ucaKP+0t6AR+VdRFtzAzjKjoTwkPR4zcLkLaELk1/rgtokEKnWwE7UiBl1Fx3ntmK6OU5wr9UUiXMdyLJopV3g/RuoYQQKBgFRafbuI6QENdf/xJUXEg849y/DiVT4PYsCe2wRredO7Shj5WTHDaO2smsYAnTus6K+sRXU29rSDg8A/3/c5GAaTkrN2u5WEN1aBI03f1CPnMqgrMoP0nmf+L7bdDxvld4Nifm4MXwytCcdpc74troDfn05OKcwgNKWvn8D9++txAoGBAMXHXkgvLHXi0Fp4XoEE4uWAqsdgVeh5pcNXRz+nZ5Jk4DH0Z+pV0XKki1NhYCaVr0UnrEqH+ejbUeaOzTbZt3JldWA0bABuiFVDkG9XYwpnohMUrF4MLPRAmLtDEgr8UGzWJLxcv2wGUpNinCwkApinGGD7nyeTF5IqiBRELv0BAoGBAKUAFQLup+WiegQZBgplLje5wtu0fZs1rbDBG3dXEbwI1RHyMTx/Egt271+WwjcBVHKcYmHTOvdUmSzoRZJDrQpEB4EFEoUYYKlJ3+Udu8q5jcRzMs3KhTMfnriOKp6C3yRcfPcanhLfTpcTccHetUZRNxwgs9MK3LJh6pVScm+5");
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/eventhub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };

            });

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
            app.UseAuthentication();
            app.UseMvc();
            app.UseSignalR(routes =>
            {
                routes.MapHub<EventHub>("/eventhub");
                routes.MapHub<AuthenticationHub>("/authenticationhub");
            });

        }
    }
}

