namespace IntraFlow.Application.Audit.Queries.GetAuditEntriesForEntity;

public sealed record GetAuditEntriesForEntityQuery(
    string EntityType,
    string EntityId);