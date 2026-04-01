using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Queries.GetAttachments;

public sealed class GetRequestAttachmentsHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetRequestAttachmentsHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<RequestAttachmentDto>> Handle(GetRequestAttachmentsQuery query, CancellationToken ct = default)
    {
        var request = await _db.Requests
            .FirstOrDefaultAsync(x => x.Id == query.RequestId);

        if (request is null)
            throw new InvalidOperationException("Request not found.");

        if (request.CreatedByUserId != _currentUser.UserId && request.AssignedApproverUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Not allowed to view attachments.");

        return await _db.RequestAttachments
            .Where(x => x.RequestId == query.RequestId)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new RequestAttachmentDto(
                x.Id,
                x.FileName,
                x.FileSizeBytes
                ))
            .ToListAsync(ct);
        
    }

}
