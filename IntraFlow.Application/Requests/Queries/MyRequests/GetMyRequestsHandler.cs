using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Queries.MyRequests;

public sealed class GetMyRequestsHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyRequestsHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<MyRequestListItemDto>> Handle(CancellationToken ct = default)
    {
        return await _db.Requests
            .Where(x => x.CreatedByUserId == _currentUser.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new MyRequestListItemDto(
                x.Id,
                x.Title,
                x.Status.ToString(),
                x.Priority.ToString(),
                x.CreatedAt))
            .ToListAsync(ct);
    }
}