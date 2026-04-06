using IntraFlow.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Tests.Application.Fakes;

public sealed class FakeUserLookupService : IUserLookupService
{
    private readonly Dictionary<string, string> _fullNames = new();
    private readonly Dictionary<string, string> _emails = new();

    public void SetUser(string userId, string fullName, string email)
    {
        _fullNames[userId] = fullName;
        _emails[userId] = email;
    }

    public Task<Dictionary<string, string>> GetFullNamesByUserIdsAsync(IEnumerable<string> userIds, CancellationToken ct = default)
    {
        var result = userIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .Where(id => _fullNames.ContainsKey(id))
            .ToDictionary(id => id, id => _fullNames[id]);

        return Task.FromResult(result);
    }

    public Task<string> RequireEmailByUserIdAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("User id is required to resolve email.");

        if (!_emails.TryGetValue(userId, out var email))
            throw new InvalidOperationException($"User '{userId}' was not found.");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException($"User '{userId}' does not have a valid email.");

        return Task.FromResult(email);
    }
}
