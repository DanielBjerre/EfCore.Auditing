using EfCore.Auditing.Factory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EfCore.Auditing.Extensions.IServiceCollectionExtensions;
public static class RegisterAuditExtension
{
    public static IServiceCollection RegisterAudit(this IServiceCollection services)
    {
        services.TryAddScoped<AuditingInterceptor>();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddSingleton<IAuditContextFactory, AuditContextFactory>();

        return services;
    }
}
