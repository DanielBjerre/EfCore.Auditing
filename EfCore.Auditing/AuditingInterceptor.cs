using EfCore.Auditing.Attributes;
using EfCore.Auditing.Context.Entities;
using EfCore.Auditing.Factory;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Diagnostics;
using System.Security.Claims;

namespace EfCore.Auditing;
internal class AuditingInterceptor(
    IAuditContextFactory auditContextFactory,
    TimeProvider timeProvider,
    IHttpContextAccessor httpContextAccessor) : ISaveChangesInterceptor
{
    private readonly IAuditContextFactory _auditContextFactory = auditContextFactory;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private readonly string _obfusactedAuditValue = "REDACTED";
    private Audit? _audit;

    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return result;
        }

        var entityEntriesWithChanges = GetChangedEntitiesWithAudit(eventData);

        if (!entityEntriesWithChanges.Any())
        {
            return result;
        }

        var entityAudits = GetEntityAudits(entityEntriesWithChanges);

        if (!entityAudits.Any())
        {
            return result;
        }

        _audit = new Audit(
            _timeProvider.GetUtcNow(),
            _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Activity.Current?.TraceId.ToString(),
            entityAudits);

        var auditContext = _auditContextFactory.GetAuditContext(eventData.Context.GetType());

        auditContext.Add(_audit);
        await auditContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is null)
        {
            return result;
        }

        var entityEntriesWithChanges = GetChangedEntitiesWithAudit(eventData);

        if (!entityEntriesWithChanges.Any())
        {
            return result;
        }

        var entityAudits = GetEntityAudits(entityEntriesWithChanges);

        if (!entityAudits.Any())
        {
            return result;
        }

        _audit = new Audit(
            _timeProvider.GetUtcNow(),
            _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Activity.Current?.TraceId.ToString(),
            entityAudits);

        var auditContext = _auditContextFactory.GetAuditContext(eventData.Context.GetType());

        auditContext.Add(_audit);
        auditContext.SaveChanges();

        return result;
    }

    private static IEnumerable<EntityEntry> GetChangedEntitiesWithAudit(DbContextEventData dbContextEventData)
    {
        return dbContextEventData.Context!.ChangeTracker
            .Entries()
            .Where(x => x.State is EntityState.Deleted or EntityState.Added or EntityState.Modified)
            .Where(x => !Attribute.IsDefined(x.Entity.GetType(), typeof(NoAuditAttribute)));
    }

    private IEnumerable<EntityAudit> GetEntityAudits(IEnumerable<EntityEntry> entityEntriesWithChanges)
    {
        foreach (var entry in entityEntriesWithChanges)
        {
            if (Attribute.IsDefined(entry.Entity.GetType(), typeof(NoAuditAttribute)))
            {
                continue;
            }

            var propertyAudits = GetPropertyAudits(entry);

            if (!propertyAudits.Any())
            {
                continue;
            }

            var (values, properties) = GetPrimaryKeyInformation(entry);

            var entityAudit = new EntityAudit(
                values,
                properties,
                entry.Metadata.GetTableName()!,
                entry.Metadata.Name,
                entry.State,
                propertyAudits);

            yield return entityAudit;
        }
    }

    private record PrimaryKeyInformation(string PrimaryKeyValues, string PrimaryKeyProperties);

    private static PrimaryKeyInformation GetPrimaryKeyInformation(EntityEntry entityEntry)
    {
        var primaryKeys = entityEntry.Properties
            .Where(x => x.Metadata.IsPrimaryKey())
            .Select(x => (x.CurrentValue, x.Metadata.Name));

        var primaryKeyValues = string.Join(',', primaryKeys.Select(x => x.CurrentValue));
        var primaryKeyProperties = string.Join(',', primaryKeys.Select(x => x.Name));

        var primaryKeyInformation = new PrimaryKeyInformation(primaryKeyValues, primaryKeyProperties);
        return primaryKeyInformation;
    }

    private IEnumerable<PropertyAudit> GetPropertyAudits(EntityEntry entityEntry)
    {
        var entityIsObfuscated = Attribute.IsDefined(entityEntry.Entity.GetType(), typeof(ObfusacteAuditAttribute));

        var entityProperties = entityEntry.Entity
            .GetType()
            .GetProperties();

        var propertiesWithAudit = entityProperties
            .Where(x => !Attribute.IsDefined(x, typeof(NoAuditAttribute)))
            .ToDictionary(x => x.Name, x => Attribute.IsDefined(x, typeof(ObfusacteAuditAttribute)) | entityIsObfuscated);

        var propertyEntriesWithAudit = entityEntry.Properties
            .Where(property => propertiesWithAudit.ContainsKey(property.Metadata.Name))
            .Select(x => new PropertyEntryWithAudit(x, propertiesWithAudit[x.Metadata.Name]));

        return entityEntry.State switch
        {
            EntityState.Added => CreateAddedPropertyAudits(propertyEntriesWithAudit),
            EntityState.Deleted => CreateDeletedPropertyAudits(propertyEntriesWithAudit),
            EntityState.Modified => CreateModifiedPropertyAudits(propertyEntriesWithAudit),
            _ => []
        };
    }

    private IEnumerable<PropertyAudit> CreateModifiedPropertyAudits(IEnumerable<PropertyEntryWithAudit> propertiesWithAudit)
    {
        foreach (var propertyWithAudit in propertiesWithAudit.Where(x => x.PropertyEntry.IsModified))
        {
            yield return propertyWithAudit.Obfuscate switch
            {
                true => new PropertyAudit(propertyWithAudit.PropertyEntry.Metadata.Name, _obfusactedAuditValue, _obfusactedAuditValue),
                false => new PropertyAudit(propertyWithAudit.PropertyEntry.Metadata.Name, propertyWithAudit.PropertyEntry.OriginalValue, propertyWithAudit.PropertyEntry.CurrentValue)
            };
        }
    }

    private IEnumerable<PropertyAudit> CreateAddedPropertyAudits(IEnumerable<PropertyEntryWithAudit> propertiesWithAudit)
    {
        foreach (var propertyWithAudit in propertiesWithAudit)
        {
            yield return propertyWithAudit.Obfuscate switch
            {
                true => new PropertyAudit(propertyWithAudit.PropertyEntry.Metadata.Name, null, _obfusactedAuditValue),
                false => new PropertyAudit(propertyWithAudit.PropertyEntry.Metadata.Name, null, propertyWithAudit.PropertyEntry.CurrentValue)
            };
        }
    }

    private IEnumerable<PropertyAudit> CreateDeletedPropertyAudits(IEnumerable<PropertyEntryWithAudit> propertiesWithAudit)
    {
        foreach (var propertyWithAudit in propertiesWithAudit)
        {
            yield return propertyWithAudit.Obfuscate switch
            {
                true => new PropertyAudit(propertyWithAudit.PropertyEntry.Metadata.Name, _obfusactedAuditValue, null),
                false => new PropertyAudit(propertyWithAudit.PropertyEntry.Metadata.Name, propertyWithAudit.PropertyEntry.OriginalValue, null)
            };
        }
    }

    private record PropertyEntryWithAudit(PropertyEntry PropertyEntry, bool Obfuscate);
}