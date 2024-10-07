using EfCore.Auditing.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EfCore.Auditing.Extensions.DbContextOptionsBuilderExtensions;
public static class WithAuditExtension
{
    public static DbContextOptionsBuilder WithAudit(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        var sqlServerOptions = builder.Options.FindExtension<SqlServerOptionsExtension>();
#pragma warning restore EF1001 // Internal EF Core API usage.

        return builder.WithAudit(serviceProvider, sqlServerOptions!.ConnectionString!);
    }

    public static DbContextOptionsBuilder WithAudit(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider, string connectionString)
    {
        var auditContextFactory = serviceProvider.GetRequiredService<IAuditContextFactory>();
        auditContextFactory.AddAuditContext(builder.Options.ContextType, connectionString);

        builder.AddInterceptors(serviceProvider.GetRequiredService<AuditingInterceptor>());
        return builder;
    }


}

