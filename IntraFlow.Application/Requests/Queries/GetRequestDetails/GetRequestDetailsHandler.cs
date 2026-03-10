using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Queries.GetRequestDetails;

public sealed class GetRequestDetailsHandler
{
    private readonly IAppDbContext _db;

    public GetRequestDetailsHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<RequestDetailsDto?> Handle(GetRequestDetailsQuery query, CancellationToken ct = default)
    {
        return await _db.Requests
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
    }
}   