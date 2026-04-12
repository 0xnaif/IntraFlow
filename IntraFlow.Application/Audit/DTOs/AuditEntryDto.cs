namespace IntraFlow.Application.Audit.DTOs;

public sealed record AuditEntryDto(
    int Id,
    string EntityType,
    string EntityId,
    string ActionType,
    string? PerformedByUserId,
    string PerformedByDisplayName,
    DateTime PerformedAt,
    string Summary,
    string? OldValuesJson,
    string? NewValuesJson,
    string? Notes);
