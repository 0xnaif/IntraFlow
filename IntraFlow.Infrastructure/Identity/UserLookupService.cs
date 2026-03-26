using System;
using System.Collections.Generic;
using System.Text;
using IntraFlow.Application.Abstractions;
using IntraFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Infrastructure.Identity;

public sealed class UserLookupService : IUserLookupService
{
    private readonly AppDbContext _db;

    public UserLookupService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<string, string>> GetFullNamesByUserIdsAsync(
        IEnumerable<string> userIds, 
        CancellationToken ct = default)
    {
        var ids = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        return await _db.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
    }
}
