namespace EfCore.Auditing.Context.Entities;
internal class Audit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? User { get; set; }
    public DateTimeOffset AuditStart { get; set; }
    public DateTimeOffset? AuditEnd { get; set; }
    public string? TraceId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Succeeded { get; set; }

    public ICollection<EntityAudit> AuditedEntities { get; set; } = [];

    public Audit(DateTimeOffset auditStart, string? user, string? traceId, IEnumerable<EntityAudit> entityAudits)
    {
        User = user;
        AuditStart = auditStart;
        TraceId = traceId;
        AuditedEntities = entityAudits.ToList();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected Audit() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
