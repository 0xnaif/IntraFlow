using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Audit.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Audit.Queries.GetAuditEntriesForEntity;

public sealed class GetAuditEntriesForEntityHandler
{
    private readonly IAppDbContext _db;
    private readonly IUserLookupService _userLookupService;

    public GetAuditEntriesForEntityHandler(
        IAppDbContext db,
        IUserLookupService userLookupService)
    {
        _db = db;
        _userLookupService = userLookupService;
    }

    public async Task<List<AuditEntryDto>> Handle(
        GetAuditEntriesForEntityQuery query,
        CancellationToken ct = default)
    {
        var logs = await _db.AuditLogs
            .Where(x => x.EntityType == query.EntityType && x.EntityId == query.EntityId)
            .OrderBy(x => x.PerformedAt)
            .Select(x => new
            {
                x.Id,
                x.EntityType,
                x.EntityId,
                x.ActionType,
                x.PerformedByUserId,
                x.PerformedAt,
                x.OldValuesJson,
                x.NewValuesJson,
                x.Notes
            })
            .ToListAsync(ct);

        var userIds = logs
            .Where(x => !string.IsNullOrWhiteSpace(x.PerformedByUserId))
            .Select(x => x.PerformedByUserId!)
            .Distinct()
            .ToList();

        var fullNames = await _userLookupService.GetFullNamesByUserIdsAsync(userIds, ct);

        return logs
            .Select(x => new AuditEntryDto(
                x.Id,
                x.EntityType,
                x.EntityId,
                x.ActionType,
                x.PerformedByUserId,
                ResolveDisplayName(x.PerformedByUserId, fullNames),
                x.PerformedAt,
                AuditEntrySummaryBuilder.Build(
                    x.ActionType,
                    x.OldValuesJson,
                    x.NewValuesJson,
                    x.Notes),
                x.OldValuesJson,
                x.NewValuesJson,
                x.Notes))
            .ToList();
    }

    private static string ResolveDisplayName(
        string? userId,
        Dictionary<string, string> fullNames)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return "System";

        return fullNames.TryGetValue(userId, out var fullName)
            ? fullName
            : userId;
    }
}