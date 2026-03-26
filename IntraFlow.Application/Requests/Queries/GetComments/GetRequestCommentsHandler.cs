using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Queries.GetComments;

public sealed class GetRequestCommentsHandler
{
    private readonly IAppDbContext _db;
    private readonly IUserLookupService _userLookupService;

    public GetRequestCommentsHandler(IAppDbContext db, IUserLookupService userLookupService)
    {
        _db = db;
        _userLookupService = userLookupService;
    }

    public async Task<List<RequestCommentDto>> Handle(GetRequestCommentsQuery query, CancellationToken ct = default)
    {

        var comments = await _db.RequestComments
            .Where(c => c.RequestId == query.RequestId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.RequestId,
                c.Body,
                c.AuthorUserId,
                c.CreatedAt
            })
            .ToListAsync(ct);

        var fullNames = await _userLookupService.GetFullNamesByUserIdsAsync(
            comments.Select(c => c.AuthorUserId), ct);



        return comments
            .Select(c => new RequestCommentDto(
                c.Id,
                c.RequestId,
                c.Body,
                c.AuthorUserId,
                fullNames.TryGetValue(c.AuthorUserId, out var fullName) ? fullName : "RequestId",
                c.CreatedAt))
            .ToList();
    }
}