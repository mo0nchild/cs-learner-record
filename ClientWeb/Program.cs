using ClientWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Build.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;
using TransferLibrary.Export;

namespace ClientWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpClient().AddNetworkTransfer();

            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();

            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddDbContext<UsersContext>(options =>
            {
                options.UseNpgsql(builder.Configuration["DATABASE_USERS"]);
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/login"; options.AccessDeniedPath = "/Account/login";
                });

            builder.Services.AddAuthorization(opts =>
            {
                opts.AddPolicy("OnlyForAdmin", policy => policy.RequireClaim(ClaimTypes.Role, "Администратор"));

                opts.AddPolicy("OnlyForTeacher", policy => policy.RequireClaim(ClaimTypes.Role, "Преподователь"));
                opts.AddPolicy("OnlyForStudent", policy => policy.RequireClaim(ClaimTypes.Role, "Студент"));
            });

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/img")), 
                RequestPath = new PathString(""),
            });
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/css")),
                RequestPath = new PathString(""),
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();

            app.MapControllerRoute( name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");

            app.MapControllerRoute(name: "teacher", pattern: "/teacher", defaults: new { controller = "Home", action = "Teacher" });

            app.Run();
        }
    }
}