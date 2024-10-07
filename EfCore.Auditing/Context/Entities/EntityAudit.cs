using Microsoft.EntityFrameworkCore;

namespace EfCore.Auditing.Context.Entities;
internal class EntityAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PrimaryKeyValues { get; set; }
    public string PrimaryKeyProperties { get; set; }
    public string TableName { get; set; }
    public string EntityName { get; set; }
    public string Action { get; set; }

    public Guid AuditId { get; set; }
    public Audit? Audit { get; set; }

    public ICollection<PropertyAudit> AuditedPropertes { get; set; } = [];

    public EntityAudit(
        string primaryKeyValues,
        string primaryKeyProperties,
        string tableName,
        string entityName,
        EntityState entityState,
        IEnumerable<PropertyAudit> propertyAudits)
    {
        PrimaryKeyValues = primaryKeyValues;
        PrimaryKeyProperties = primaryKeyProperties;
        TableName = tableName;
        EntityName = entityName;
        Action = entityState.ToString();
        AuditedPropertes = propertyAudits.ToList();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected EntityAudit() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}