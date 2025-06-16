namespace DapperIdentity.Configuration.Extensions;

public class InfrastructureExtension
{
    public static ICollectionService(this ICollectionService service)
    {
        service.AddScoped<IBaseDapperContext, IBaseDapperContext>();
    }
}