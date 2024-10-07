using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfCore.Auditing.Context;
internal class AuditContextDesignTimeFactory : IDesignTimeDbContextFactory<AuditContext>
{
    public AuditContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditContext>()
            .UseSqlServer();

        return new AuditContext(optionsBuilder.Options);
    }
}
