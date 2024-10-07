using EfCore.Auditing.Context;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Auditing.Factory;
internal class AuditContextFactory : IAuditContextFactory
{
    private readonly Dictionary<Type, string> _connectionStrings = [];

    public void AddAuditContext(Type dbContextType, string connectionString)
    {
        if (_connectionStrings.ContainsKey(dbContextType)) 
        {
            return;
        }

        var dataBaseHasAlreadyBeenMigrated = _connectionStrings.ContainsValue(connectionString);
        _connectionStrings.Add(dbContextType, connectionString);

        if (dataBaseHasAlreadyBeenMigrated) 
        {
            return;
        }

        var context = GetAuditContext(dbContextType);
        context.Database.Migrate();
    }

    public AuditContext GetAuditContext(Type dbContextType)
    {
        var connectionString = _connectionStrings[dbContextType];

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<AuditContext>()
            .UseSqlServer(connectionString);

        return new AuditContext(dbContextOptionsBuilder.Options);
    }
}
