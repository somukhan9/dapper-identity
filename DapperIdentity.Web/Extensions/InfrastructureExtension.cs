using Configuration.DapperConfiguration.Abstractions;
using DapperIdentity.Configuration.DapperConfiguration;

namespace DapperIdentity.Web.Extensions;

public static class InfrastructureExtension
{
    public static WebApplicationBuilder AddInfrastructureToServiceContainer(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBaseDapperContext, BaseDapperContext>();

        return builder;
    }
}