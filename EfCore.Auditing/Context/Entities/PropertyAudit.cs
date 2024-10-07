namespace EfCore.Auditing.Context.Entities;
internal class PropertyAudit
{
    public PropertyAudit(string name, object? before, object? after)
    {
        Name = name;
        Before = before?.ToString();
        After = after?.ToString();
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string? Before { get; set; }
    public string? After { get; set; }

    public Guid EntityAuditId { get; set; }
    public EntityAudit? EntityAudit { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected PropertyAudit() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}