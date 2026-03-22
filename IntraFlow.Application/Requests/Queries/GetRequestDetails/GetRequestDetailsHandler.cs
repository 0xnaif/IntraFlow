using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Queries.GetRequestDetails;

public sealed class GetRequestDetailsHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetRequestDetailsHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RequestDetailsDto?> Handle(GetRequestDetailsQuery query, CancellationToken ct = default)
    {
        var request = await _db.Requests
            .Where(x => x.Id == query.RequestId)
            .Select(x => new RequestDetailsDto(
                x.Id,
                x.Title,
                x.Description,
                x.Priority,
                x.Status,
                x.CreatedByUserId,
                x.CreatedAt,
                x.AssignedApproverUserId))
            .FirstOrDefaultAsync(ct);

        if (request is null)
            return null;

        var canView =
            request.CreatedByUserId == _currentUser.UserId ||
            request.AssignedApproverUserId == _currentUser.UserId;

        if (!canView)
            throw new UnauthorizedAccessException("You are not authorized to view this request.");

        return request;
    }
}   