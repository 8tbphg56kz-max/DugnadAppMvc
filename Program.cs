using DugnadAppMvc.Configuration;
using DugnadAppMvc.Data;
using DugnadAppMvc.Models;
using DugnadAppMvc.Services;
using DugnadAppMvc.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DugnadAppMvc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    // Passordkrav
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;

                    // Brukernavn
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";

                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
            });

            // MVC
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpContextAccessor();

            builder.Services.Configure<EmailSettings>(
            builder.Configuration.GetSection("EmailSettings"));

            builder.Services.Configure<DefaultAdminOptions>(
            builder.Configuration.GetSection("DefaultAdmin"));

            builder.Services.AddScoped<EmailService>();

            builder.Services.AddScoped<UserProvisioningService>();

            builder.Services.AddScoped<IUserAdministrationService, UserAdministrationService>();

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                await RoleSeeder.SeedAsync(scope.ServiceProvider);
                await UserSeeder.SeedAsync(scope.ServiceProvider);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseForwardedHeaders();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Identity
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Dashboard}/{action=Index}/{id?}");

            app.Run();
        }
    }
}