
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using Task4Core.Models;

namespace CustomIdentityApp
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
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<AppDbContext>(options =>
              options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<User, IdentityRole>(opts => {
                opts.Password.RequiredLength = 1;   // минимальная длина
                opts.Password.RequireNonAlphanumeric = false;   // требуются ли не алфавитно-цифровые символы
                opts.Password.RequireLowercase = false; // требуются ли символы в нижнем регистре
                opts.Password.RequireUppercase = false; // требуются ли символы в верхнем регистре
                opts.Password.RequireDigit = false; // требуются ли цифры
            })
    .AddEntityFrameworkStores<ApplicationContext>();
            
            services.AddLocalization(opt =>
            {
                opt.ResourcesPath = "Resources";
            });
            services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix).AddDataAnnotationsLocalization();
            services.Configure<RequestLocalizationOptions>(opt =>
            {
                var supportedCulteres = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("ru")
                };

                opt.DefaultRequestCulture = new RequestCulture("en");
                opt.SupportedCultures = supportedCulteres;
                opt.SupportedUICultures = supportedCulteres;
            });
            services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "371566347867900";
                options.ClientSecret = "203b47bf8ba4621de5921f1879e80aef";
            })
            .AddGoogle(options=>
            {
                options.ClientId = "892513975969-9960oo2ga18sm0v5uangs36qsmr4lehj.apps.googleusercontent.com";
                options.ClientSecret = "hGcc4QLTkuC4_9vafKzX6URA";
            });
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddApplicationInsightsTelemetry();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();    // подключение аутентификации
            app.UseAuthorization();



            //var supportCultres = new[] { "en", "ru" };

            //var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportCultres[0]).AddSupportedCultures(supportCultres).AddSupportedUICultures(supportCultres);

            //app.UseRequestLocalization(localizationOptions);    
            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Collections}/{action=Index}/{id?}");
            });
        }
    }
}