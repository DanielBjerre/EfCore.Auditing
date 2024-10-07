using EfCore.Auditing.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Auditing.Context;
internal class AuditContext(DbContextOptions<AuditContext> options) : DbContext(options)
{
    public DbSet<Audit> Audits { get; set; }
    public DbSet<EntityAudit> EntityAudits { get; set; }
    public DbSet<PropertyAudit> PropertyAudits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Audit");
    }
}
