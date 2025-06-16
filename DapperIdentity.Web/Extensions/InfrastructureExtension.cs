
using Configuration.DapperConfiguration;
using Configuration.DapperConfiguration.Abstractions;

namespace DapperIdentity.Web.Extensions;

public static class InfrastructureExtension
{
    public static WebApplicationBuilder AddInfrastructureToServiceContainer(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBaseDapperContext, BaseDapperContext>();

        return builder;
    }
}