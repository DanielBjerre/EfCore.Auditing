using EfCore.Auditing.Context;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Auditing.Factory;
internal class AuditContextFactory : IAuditContextFactory
{
    private readonly Dictionary<Type, string> _connectionStrings = [];
    private readonly HashSet<string> _migratedConnectionStrings = [];

    public void AddAuditContext(Type dbContextType, string connectionString)
    {
        if (!_connectionStrings.TryAdd(dbContextType, connectionString))
        {
            return;
        }

        if (_migratedConnectionStrings.Contains(connectionString))
        {
            return;
        }

        var context = GetAuditContext(dbContextType);
        context.Database.Migrate();
        _migratedConnectionStrings.Add(connectionString);
    }

    public AuditContext GetAuditContext(Type dbContextType)
    {
        var connectionString = _connectionStrings[dbContextType];

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<AuditContext>()
            .UseSqlServer(connectionString);

        return new AuditContext(dbContextOptionsBuilder.Options);
    }
}
