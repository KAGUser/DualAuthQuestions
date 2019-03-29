using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public void ConfigureServices(IServiceCollection services)
        {
            // Removed for brevity - Gets conn string

            // Enable Dual Authentication 
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie()
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;

                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Tokens:Issuer"],
                        ValidAudience = Configuration["Tokens:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
                    };
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

            // I have tried the below to make dual auth work but no luck
            //services.AddAuthorization(options =>
            //{
            //    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
            //        CookieAuthenticationDefaults.AuthenticationScheme,
            //        JwtBearerDefaults.AuthenticationScheme);
            //    defaultAuthorizationPolicyBuilder =
            //        defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            //    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            //});

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