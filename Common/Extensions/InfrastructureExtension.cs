using Common.EmailService;
using DapperIdentity.Configuration.DapperConfiguration;
using DapperIdentity.Configuration.DapperConfiguration.Abstractions;
using DapperIdentity.Configuration.IdentityConfiguration;
using DapperIdentity.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions;

public static class InfrastructureExtension
{
    public static void AddInfrastructureToServiceContainer(this IServiceCollection services, IConfiguration config)
    {
        // Identity Services
        services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.SignIn.RequireConfirmedEmail = true;
            })
            .AddUserStore<DapperUserStore>()
            .AddRoleStore<DapperRoleStore>()
            .AddDefaultTokenProviders();

        // Configure Application Cookie
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = $"/identity/account/login";
            options.LogoutPath = $"/identity/account/logout";
            options.AccessDeniedPath = $"/identity/account/accessdenied";

            /*options.LoginPath = $"/Identity/Account/Login";
            options.LogoutPath = $"/Identity/Account/Logout";
            options.AccessDeniedPath = $"/Identity/Account/AccessDenied";*/
        });

        // Single Tone Service
        services.AddSingleton<IEmailSender, EmailSender>();

        // Scope Services
        services.AddScoped<IBaseDapperContext>(_ => new BaseDapperContext(config));
    }
}