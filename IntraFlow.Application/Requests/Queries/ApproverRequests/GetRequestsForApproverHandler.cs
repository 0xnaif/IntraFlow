using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.DTOs;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Queries.ApproverRequests;

public sealed class GetRequestsForApproverHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetRequestsForApproverHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<ApproverRequestListItemDto>> Handle(CancellationToken ct = default)
    {
        return await _db.Requests
            .Where(x =>
                (x.Status == RequestStatus.Submitted ||
                x.Status == RequestStatus.InReview) &&
                x.AssignedApproverUserId == _currentUser.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ApproverRequestListItemDto(
                x.Id,
                x.Title,
                x.Status,
                x.Priority,
                x.CreatedAt,
                x.CreatedByUserId))
            .ToListAsync(ct);
    }
}