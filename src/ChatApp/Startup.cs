using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Razor;
using ChatApp.Data;
using ChatApp.Services;
using ChatApp.Services.RoomwebSocket;
using ChatApp.Config;
using ChatApp.SharedResources;
using ChatApp.IdentityLocaleError;

namespace ChatApp
{
    public class Startup
    {
        private readonly string XSRF_TOKEN_NAME = "XSRF-TOKEN";

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // Add framework services.
            services.AddAntiforgery(options =>
                {
                    options.HeaderName = "X-" + XSRF_TOKEN_NAME;
                });

            services.AddSession();

            services.AddMemoryCache();

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;

                    options.User.RequireUniqueEmail = true;

                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddErrorDescriber<IdentityLocaleErrorDescriber>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    options.AccessType = Configuration["Authentication:Google:AccessType"];
                    options.SaveTokens = true;
                });

            // Localization
            services.AddLocalization(options => { options.ResourcesPath = "Resources"; });

            // Mvc
            services.AddMvc(options =>
            {
                options.Conventions.Add(new FeatureConvention());
            }).AddRazorOptions(options =>
            {
                // {0} - Action Name
                // {1} - Controller Name
                // {2} - Area Name
                // {3} - Feature Name
                // Replace normal view location entirely
                options.ViewLocationFormats.Clear();

                options.ViewLocationFormats.Add("/Features/{3}/{1}/Views/{0}.cshtml");
                options.ViewLocationFormats.Add("/Features/{3}/Views/{0}.cshtml");
                options.ViewLocationFormats.Add("/Features/{3}/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Features/{3}/{0}.cshtml");

                options.ViewLocationFormats.Add("/Features/Shared/{0}.cshtml");
                options.ViewLocationFormats.Add("/Features/Shared/{1}/{0}.cshtml");

                options.ViewLocationExpanders.Add(new FeatureConvention());
            }).AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                {
                    return factory.Create(typeof(SharedResource));
                };
            }).AddViewLocalization(LanguageViewLocationExpanderFormat.SubFolder, options =>
            {
                options.ResourcesPath = "Resources";
            });

            // EMailSender
            services.Configure<MailOptions>(Configuration.GetSection("MailOptions"));
            services.TryAddTransient<IEmailSender, GMailSender>();

            // SmsSender
            services.TryAddTransient<ISmsSender, AuthMessageSender>();

            // Application services.
            services.TryAddSingleton<IDateTimeService, DateTimeService>();
            services.TryAddTransient<IControllerService, ControllerBaseService>();
            services.TryAddSingleton<IRoomWebSocketService, RoomWebSocketService>();
            services.TryAddSingleton<IRoomWSSender, RoomWSSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IAntiforgery antiforgery)
        {
            app.UseForwardedHeaders();
            app.UsePathBase(Configuration["PathBase"]);

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                loggerFactory.AddFile(Configuration.GetSection("Logging"));
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseWebSockets();

            app.UseMiddleware<RoomWebSocketServer>("/ws/rooms/connect");

            app.Use(next => context =>
            {
                var token = antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append(XSRF_TOKEN_NAME, token.RequestToken,
                    new CookieOptions()
                    {
                        HttpOnly = false,
                        Path = context.Request.PathBase,
                    });
                return next(context);
            });

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
