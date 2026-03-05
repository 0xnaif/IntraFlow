using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Queries.GetComments;

public sealed class GetRequestCommentsHandler
{
    private readonly IAppDbContext _db;

    public GetRequestCommentsHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RequestCommentDto>> Handle(GetRequestCommentsQuery query, CancellationToken ct = default)
    {
        return await _db.RequestComments
            .Where(x => x.RequestId == query.RequestId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new RequestCommentDto(
                x.Id,
                x.AuthorUserId,
                x.Body,
                x.CreatedAt))
            .ToListAsync(ct);
    }
}