namespace IntraFlow.Application.Abstractions;

public interface IUserLookupService
{
    Task<Dictionary<string, string>> GetFullNamesByUserIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default);

    Task<string> RequireEmailByUserIdAsync(
        string userId,
        CancellationToken ct = default);
}