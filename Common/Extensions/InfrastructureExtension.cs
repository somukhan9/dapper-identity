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
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IEmailSender, EmailSender>();
        services.AddSingleton<IBaseDapperContext>(_ => new BaseDapperContext(config));
    }

    public static void AddApplicationIdentity(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddUserStore<DapperUserStore>()
            .AddRoleStore<DapperRoleStore>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = $"/identity/account/login";
            options.LogoutPath = $"/identity/account/logout";
            options.AccessDeniedPath = $"/identity/account/accessdenied";
        });
    }
}