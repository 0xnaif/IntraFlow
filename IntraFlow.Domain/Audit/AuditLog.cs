using IntraFlow.Domain.Common;

namespace IntraFlow.Domain.Audit;

public class AuditLog : AuditableEntity
{
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string ActionType { get; private set; } = null!;
    public string? PerformedByUserId { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string? OldValuesJson { get; private set; }
    public string? NewValuesJson { get; private set; }
    public string? Notes { get; private set; }

    private AuditLog() { }

    public AuditLog(
        string entityType,
        string entityId,
        string actionType,
        string? performedByUserId,
        string? oldValuesJson,
        string? newValuesJson,
        string? notes)
    {
        EntityType = entityType;
        EntityId = entityId;
        ActionType = actionType;
        PerformedByUserId = performedByUserId;
        OldValuesJson = oldValuesJson;
        NewValuesJson = newValuesJson;
        Notes = notes;
        PerformedAt = DateTime.UtcNow;
    }
}
