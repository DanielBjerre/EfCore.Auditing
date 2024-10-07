using EfCore.Auditing.Context;

namespace EfCore.Auditing.Factory;
internal interface IAuditContextFactory
{
    public void AddAuditContext(Type dbContextType, string connectionString);
    public AuditContext GetAuditContext(Type dbContextType);
}
