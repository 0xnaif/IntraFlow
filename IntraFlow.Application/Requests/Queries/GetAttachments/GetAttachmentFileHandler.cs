using IntraFlow.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.Requests.Queries.GetAttachments;

public sealed class GetAttachmentFileHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetAttachmentFileHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<(byte[] Data, string ContentType, string FileName, int RequestId)> Handle(GetAttachmentFileQuery query, CancellationToken ct = default)
    {
        var attachment = await _db.RequestAttachments
            .Where(x => x.Id == query.AttachmentId)
            .Select(x => new
            {
                x.FileData,
                x.ContentType,
                x.FileName,
                x.RequestId
            })
            .FirstOrDefaultAsync();

        if (attachment is null)
            throw new InvalidOperationException("Attachment not found.");

        var request = await _db.Requests
            .FirstOrDefaultAsync(x => x.Id == attachment.RequestId, ct);

        if (request is null)
            throw new InvalidOperationException("Request not found.");

        if (request.CreatedByUserId != _currentUser.UserId && request.AssignedApproverUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Not allowed to view attachments.");

        return (attachment.FileData, attachment.ContentType, attachment.FileName, attachment.RequestId);
    }
}
