using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CO.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public object AzureADDefaults { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Removed for brevity - Gets conn string

            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
            services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                // This is an Azure AD v2.0 Web API
                options.Authority += "/v2.0";

                // The valid audiences are both the Client ID (options.Audience) and api://{ClientID}
                options.TokenValidationParameters.ValidAudiences = new string[] { options.Audience, $"api://{options.Audience}" };

                // Instead of using the default validation (validating against a single tenant, as we do in line of business apps),
                // we inject our own multitenant validation logic (which even accepts both V1 and V2 tokens)
                options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.ValidateAadIssuer;

                // If you want to debug, or just understand the JwtBearer events, uncomment the following line of code
                // options.Events = JwtBearerMiddlewareDiagnostics.Subscribe(options.Events);
            });

            // Removed for brevity - Add dbcontext etc.

            services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireNonAlphanumeric = true;
                o.Password.RequiredLength = 8;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Authentication/Login";
                options.AccessDeniedPath = "/Authentication/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            // Add application services.
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented;
            });
            services.AddSingleton(Configuration);

            // Removed for brevity - Caching

            // Add Session
            services.AddSession();
            services.AddSingleton<HttpContextAccessor>();
            services.AddScoped<SessionManager>();

            // Removed for brevity - Emails etup

            // Removed for brevity - Add helpers
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UsePathBase("/core");

            // Removed for brevity 

            // Removed for brevity - setup CORS to allow API access

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "areaRoute",
                    "{area}/{controller=Default}/{action=Index}/{id?}");

                routes.MapRoute(
                    "defaultRoute",
                    "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}